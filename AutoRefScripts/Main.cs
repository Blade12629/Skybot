using AutoRefTypes;
using AutoRefTypes.Extended;
using AutoRefTypes.Extended.Requests;
using AutoRefTypes.Events;
using AutoRefTypes.Google.SpreadSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoRefScripts
{
    public class Main : EventObject, IUpdate, IAllUsersReady, IMapEnd
    {
        ILobby _lobby;
        IDiscordHandler _discord;
        IEventRunner _eventRunner;
        ISpreadsheet _sheet;
        ScriptInput _input;

        int _state;
        LobbySort _sorter;

        RollRequest _rollRequestBlue;
        RollRequest _rollRequestRed;

        PickRequest _pickRequestBlue;
        PickRequest _pickRequestRed;

        ulong _firstMapPick;
        ulong _SecondMapPick;

        bool _allPlayersReady;
        bool _mapEnded;

        public Main(ILobby lobby, IEventRunner eventRunner, IDiscordHandler discord, ISpreadsheet sheet, ScriptInput input) : base(eventRunner)
        {
            _lobby = lobby;
            _discord = discord;
            _eventRunner = eventRunner;
            _sheet = sheet;
            _input = input;

            _sorter = new LobbySort(eventRunner, lobby, input.CaptainA, input.CaptainB);
        }

        public void OnAllUsersReady()
        {
            _allPlayersReady = true;
        }

        public void OnMapEnd()
        {
            _mapEnded = true;
        }

        public void Update()
        {
            switch(_state)
            {
                default:
                case -1:
                    return;

                #region Invite Players
                case 0:
                    _lobby.Lock();
                    _lobby.SetLobby(TeamMode.TeamVs, WinCondition.ScoreV2, 16);

                    _lobby.Invite(_input.CaptainA);
                    //_lobby.Invite(_input.CaptainB);
                    break;

                    // Wait for players
                case 1:
                    {
                        List<ISlot> slots = _lobby.LobbyData.GetUsedSlots();

                        if (!slots.Any(s => s.Nickname.Equals(_input.CaptainA)) /*||*/
                            /*!slots.Any(s => s.Nickname.Equals(_input.CaptainB))*/)
                            return;

                        _lobby.SendChannelMessage("All players joined, starting match");
                    }
                    break;
                #endregion

                #region Sort Players
                case 2:
                    //_sorter.Register(_eventRunner);
                    break;
                case 3:
                    //if (!_sorter.Finished)
                    //    return;
                    break;
                #endregion

                #region Rolls
                case 4:
                    _rollRequestBlue = new RollRequest(_lobby, _input.CaptainA, "Please roll " + _input.CaptainA);
                    _rollRequestBlue.Request();
                    break;
                case 5:
                    if (!_rollRequestBlue.RequestFinished)
                        return;
                    break;

                case 6:
                    //_rollRequestRed = new RollRequest(_lobby, _input.CaptainB, "Please roll " + _input.CaptainB);
                    //_rollRequestRed.Request();
                    break;
                case 7:
                    //if (!_rollRequestRed.RequestFinished)
                    //    return;
                    break;
                #endregion

                #region Picks
                case 8:
                    {
                        string winner = _input.CaptainA;
                        //string winner = _rollRequestBlue.Rolled > _rollRequestRed.Rolled ? _input.CaptainA : _input.CaptainB;

                        _lobby.SendChannelMessage($"{winner} won");

                        _pickRequestBlue = new PickRequest(_lobby, _input.CaptainA, false, $"{_input.CaptainA} please pick a map via !pick");
                        //_pickRequestRed = new PickRequest(_lobby, _input.CaptainB, false, $"{_input.CaptainB} please pick a map via !pick");

                        _pickRequestBlue.Request();
                        //_pickRequestRed.Request();
                    }
                    break;

                case 9:
                    if (!_pickRequestBlue.RequestFinished /*||*/
                        /*!_pickRequestRed.RequestFinished*/)
                        return;
                    break;

                case 10:
                    _firstMapPick = _pickRequestBlue.MapPick;
                    //if (_rollRequestBlue.Rolled > _rollRequestRed.Rolled)
                    //{
                    //    _firstMapPick = _pickRequestBlue.MapPick;
                    //    _SecondMapPick = _pickRequestRed.MapPick;
                    //}
                    //else
                    //{
                    //    _firstMapPick = _pickRequestRed.MapPick;
                    //    _SecondMapPick = _pickRequestBlue.MapPick;
                    //}

                    _lobby.SendChannelMessage($"First pick: {_firstMapPick}, Second pick: {_SecondMapPick}");
                    break;
                #endregion

                case 11:
                    _lobby.SetMap(_firstMapPick);
                    _lobby.SendChannelMessage("Please ready up");
                    break;

                case 12:
                    if (!_allPlayersReady)
                        return;

                    _lobby.StartMap(TimeSpan.FromSeconds(10));
                    _lobby.SendChannelMessage("Good luck!");
                    _allPlayersReady = false;
                    break;

                case 13:
                    {
                        if (!_mapEnded)
                            return;

                        IScore score = _lobby.LobbyData.Scores.First();

                        _lobby.SendChannelMessage($"{score.Username} achived {score.UserScore} score");

                        //IScore blueScore = _lobby.LobbyData.Scores.First(s => s.Username.Equals(_input.CaptainA, StringComparison.CurrentCultureIgnoreCase));
                        //IScore redScore = _lobby.LobbyData.Scores.First(s => s.Username.Equals(_input.CaptainB, StringComparison.CurrentCultureIgnoreCase));

                        //if (blueScore.UserScore > redScore.UserScore)
                        //    _lobby.SendChannelMessage($"{_input.CaptainA} won!");
                        //else
                        //    _lobby.SendChannelMessage($"{_input.CaptainB} won!");
                    }
                    break;

                case 14:
                    //_lobby.SetMap(_SecondMapPick);
                    //_lobby.SendChannelMessage("Please ready up");
                    break;

                case 15:
                    //if (!_allPlayersReady)
                    //    return;

                    //_lobby.StartMap(TimeSpan.FromSeconds(10));
                    //_lobby.SendChannelMessage("Good luck!");
                    //_allPlayersReady = false;
                    break;

                case 16:
                    {
                        //if (!_mapEnded)
                        //    return;

                        //IScore blueScore = _lobby.LobbyData.Scores.First(s => s.Username.Equals(_input.CaptainA, StringComparison.CurrentCultureIgnoreCase));
                        //IScore redScore = _lobby.LobbyData.Scores.First(s => s.Username.Equals(_input.CaptainB, StringComparison.CurrentCultureIgnoreCase));

                        //if (blueScore.UserScore > redScore.UserScore)
                        //    _lobby.SendChannelMessage($"{_input.CaptainA} won!");
                        //else
                        //    _lobby.SendChannelMessage($"{_input.CaptainB} won!");
                    }
                    break;

                case 17:
                    _lobby.DebugLog("Closing lobby");
                    _lobby.SendChannelMessage("Finished test run");
                    _lobby.Close();
                    break;

            }

            _state++;
        }
    }

    public class LobbySort : EventObject, IUpdate
    {
        public bool Finished => _sorter.FinishedSorting;

        SlotSorter _sorter;
        IEventRunner _ev;

        public LobbySort(IEventRunner ev, ILobby lobby, string captainA, string captainB) : base(ev, false)
        {
            _ev = ev;
            _sorter = new SlotSorter(lobby, new string[] { captainA, captainB });
        }

        public void Update()
        {
            if (_sorter.IsSorting)
                _sorter.Sort();
            else if (_sorter.FinishedSorting)
                Deregister(_ev);
        }
    }
}
