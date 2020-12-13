using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Extended
{
    /// <summary>
    /// A slot sorter that sorts by a string list
    /// </summary>
    public class SlotSorter
    {
        /// <summary>
        /// Finished sorting the slots
        /// </summary>
        public bool FinishedSorting { get; private set; }
        /// <summary>
        /// Is currently in the sorting process
        /// </summary>
        public bool IsSorting { get; private set; }

        ILobbyDataHandler _data;
        ILobby _lobby;
        string[] _slotOrder;

        List<Action> _sortActions;
        int _sortIndex;

        /// <summary>
        /// A slot sorter that sorts by a string list
        /// </summary>
        /// <param name="slotOrder">Slots in order, use irc nicknames</param>
        public SlotSorter(ILobby lobby, List<string> slotOrder) : this(lobby, slotOrder.ToArray())
        {
        }

        /// <summary>
        /// A slot sorter that sorts by a string list
        /// </summary>
        /// <param name="slotOrder">Slots in order, use irc nicknames</param>
        public SlotSorter(ILobby lobby, string[] slotOrder)
        {
            if (slotOrder == null)
                throw new ArgumentNullException(nameof(slotOrder), "slotOrder cannot be null");
            else if (slotOrder.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(slotOrder), "slotOrder cannot be empty");

            _lobby = lobby;
            _slotOrder = slotOrder;
            _data = lobby.LobbyData;
        }

        void InternalSort()
        {
            if (_sortActions == null)
                _sortActions = new List<Action>();

            _sortActions.Clear();
            _sortIndex = 0;

            string[] slotOrder = new string[_slotOrder.Length];
            _slotOrder.CopyTo(slotOrder, 0);

            for (int i = 0; i < slotOrder.Length; i++)
            {
                ISlot userSlot = _data.GetSlot(slotOrder[i]);

                //User already at correct position
                if (userSlot.Id == i + 1)
                    continue;

                ISlot destSlot = _data.GetSlot(i);

                //Slot is free, only move player
                if (!destSlot.IsUsed)
                {
                    _sortActions.Add(new Action(() => _lobby.SetSlot(userSlot.Nickname, destSlot.Id)));
                }
                //Slot is not free, move player out of the way and then move player in
                else
                {
                    ISlot freeSlot = _data.GetFirstUnusedSlot();

                    _sortActions.Add(new Action(() => _lobby.SetSlot(destSlot.Nickname, freeSlot.Id)));
                    _sortActions.Add(new Action(() => _lobby.SetSlot(userSlot.Nickname, destSlot.Id)));
                }
            }
        }

        /// <summary>
        /// Starts the sort process, you should call <see cref="Sort"/> on every <see cref="Events.IUpdate.Update"/> while it's sorting, see <see cref="IsSorting"/> and <see cref="FinishedSorting"/>
        /// </summary>
        public void StartSort()
        {
            if (IsSorting)
                return;

            InternalSort();

            IsSorting = true;
        }

        /// <summary>
        /// Stops the sorting process
        /// </summary>
        public void StopSort()
        {
            if (!IsSorting)
                return;

            IsSorting = false;
        }

        /// <summary>
        /// Advances a step in the sorting process, best to call this is in <see cref="Events.IUpdate.Update"/>, do not call this multiple times per tick!
        /// </summary>
        public void Sort()
        {
            if (IsSorting)
            {
                if (_sortIndex < _sortActions.Count)
                {
                    Action sortAc = _sortActions[_sortIndex];
                    _sortIndex++;

                    sortAc();
                }

                if (_sortIndex >= _sortActions.Count)
                {
                    IsSorting = false;
                    FinishedSorting = true;
                }
            }
        }
    }
}
