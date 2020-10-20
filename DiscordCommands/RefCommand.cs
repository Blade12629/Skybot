using SkyBot;
using SkyBot.Osu.AutoRef;
using SkyBot.Osu.AutoRef.Match;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class RefCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "ref";

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public CommandType CommandType => CommandType.None;

        public string Description => "DEBUG TOOL, Manage the bots autoref";

        public string Usage => "No Description Available";

        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        private static LobbyController _lobby;
        private static MatchController _controller;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            switch(args.Parameters[0])
            {
                case "create":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    string mpName = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');

                    CreateMatch(args, mpName);
                    return;

                case "close":
                    _lobby.CloseMatch();
                    _lobby = null;

                    args.Channel.SendMessageAsync("Closed room").ConfigureAwait(false);
                    return;

                case "joinclose":
                    Program.IRC.SendCommandAsync("JOIN", args.Parameters[1]).ConfigureAwait(false).GetAwaiter().GetResult();
                    Program.IRC.SendMessageAsync(args.Parameters[1], "!mp close").ConfigureAwait(false).GetAwaiter().GetResult();
                    args.Channel.SendMessageAsync("Closed match " + args.Parameters[1]).ConfigureAwait(false).GetAwaiter().GetResult();
                    return;

                case "settings":
                    _lobby.RefreshSettings();
                    return;

                case "invite":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    string inviteName = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');

                    _lobby.Invite(inviteName);
                    args.Channel.SendMessageAsync("Invited user").ConfigureAwait(false);
                    return;

                case "addref":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    string addrefName = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');
                    _lobby.AddRef(addrefName);
                    args.Channel.SendMessageAsync("Added ref").ConfigureAwait(false);
                    return;

                case "removeref":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    string remrefName = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');

                    _lobby.RemoveRef(remrefName);
                    args.Channel.SendMessageAsync("Removed ref").ConfigureAwait(false);
                    return;

                case "roll":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    var roll = _lobby.RequestRoll(args.Parameters[1]);

                    args.Channel.SendMessageAsync($"{roll.Nickname} rolled {roll.Rolled} (Min {roll.Min} Max {roll.Max}");
                    return;

                case "pick":
                    if (args.Parameters.Count == 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                        return;
                    }

                    long? pick = _lobby.RequestPick(args.Parameters[1]);

                    args.Channel.SendMessageAsync($"Skyfly picked: {(pick ?? -1)}");
                    return;

                case "testmatch":
                    SetTestMatchUp(args, args.Parameters[1], args.Parameters[2]);
                    return;
            }

            args.Channel.SendMessageAsync("Unkown parameters").ConfigureAwait(false);
        }

        private void SetTestMatchUp(CommandEventArg args, string userA, string userB)
        {
            _lobby = new LobbyController(Program.IRC);
            _lobby.OnLobbyCreated += (s, e) => args.Channel.SendMessageAsync($"Room testmatch ({_lobby?.Settings.ChannelName ?? "null"}) created").ConfigureAwait(false);

            MatchSettings settings = new MatchSettings()
            {
                MatchStartTime = DateTime.UtcNow.AddMinutes(0),
                MatchCreationDelay = TimeSpan.FromMinutes(0),
                MatchInviteDelay = TimeSpan.FromMinutes(0),
                MatchEndDelay = TimeSpan.FromMinutes(4),
                PlayersReadyUpDelay = TimeSpan.FromMinutes(1),

                MatchName = "test match",

                PlayersBlue = null,
                PlayersRed = null,
                CaptainBlue = userA,
                CaptainRed = userB,

                TotalWarmups = 2,
                TotalRounds = 4,
                SubmissionChannel = args.Channel.Id,
                IsTestRun = true
            };

            _controller = new MatchController(_lobby, settings);
            _lobby.CreateMatch(settings.MatchName);


            System.Threading.Tasks.Task.Run(() =>
            {
                if (!_controller.TryRun(out Exception ex))
                    args.Channel.SendMessageAsync(ex.ToString());
            });
        }

        private void CreateMatch(CommandEventArg args, string mpName)
        {
            _lobby = new LobbyController(Program.IRC);
            _lobby.OnLobbyCreated += (s, e) => args.Channel.SendMessageAsync($"Room {mpName} ({_lobby?.Settings.ChannelName ?? "null"}) created").ConfigureAwait(false);
            _lobby.CreateMatch(mpName);
        }
    }
}
