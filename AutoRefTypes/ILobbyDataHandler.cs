using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    /// <summary>
    /// Contains lobby data like slots
    /// </summary>
    public interface ILobbyDataHandler
    {
        /// <summary>
        /// Contains all match rounds
        /// </summary>
        public IReadOnlyList<IMatchRound> MatchRounds { get; }
        /// <summary>
        /// Contains the scores from the last map, cleared on every map start
        /// </summary>
        public IReadOnlyList<IScore> Scores { get; }

        /// <summary>
        /// Gets a specific slot
        /// </summary>
        /// <param name="nickname">Irc nickname</param>
        public ISlot GetSlot(string nickname, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase);
        /// <summary>
        /// Gets a specific slot
        /// </summary>
        public ISlot GetSlot(int slot);
        /// <summary>
        /// Gets the first unused slot
        /// </summary>
        public ISlot GetFirstUnusedSlot();
        /// <summary>
        /// Gets the first used slot
        /// </summary>
        public ISlot GetFirstUsedSlot();
        /// <summary>
        /// Gets a specific slot
        /// </summary>
        public ISlot GetSlot(Func<ISlot, bool> predicate);

        /// <summary>
        /// Gets a list of specific slots
        /// </summary>
        public List<ISlot> GetSlots(Func<ISlot, bool> predicate);
        /// <summary>
        /// Gets a list of specific slots
        /// </summary>
        public List<ISlot> GetSlots();
        /// <summary>
        /// Gets a list of used slots
        /// </summary>
        public List<ISlot> GetUsedSlots();
        /// <summary>
        /// Gets a list of unused slots
        /// </summary>
        public List<ISlot> GetUnusedSlots();
    }
}
