
function InvitePlayers() {
    Ref.InviteAllPlayers();
    return true;
}

function Wait(duration) {
    return Ref.PassedDelaySinceStart(duration);
}

function SetupTeams() {
    Lobby.Lock();
    Lobby.SortPlayers();
    return true;
}

function RequestRolls() {
    Ref.RequestRolls();
    return true;
}

function GetPick(ban) {
    const lastPick = Ref.GetLastPick();

    if (lastPick == 0 || !Ref.IsLastPickValid())
        return false;

    if (ban) {
        if (!Ref.Ban(lastPick)) {
            RequestPick();
            return false;
        }
    }
    else {
        if (!Ref.Pick(lastPick)) {
            RequestPick();
            return false;
        }
    }

    Ref.SetNextCaptainPick();

    return true;
}

function RequestPick() {
    Ref.RequestCurrentPick();
}

function GetBan() {
    return GetPick(true);
}

function GetMap() {
    return GetPick(false);
}

function WaitForMapEnd() {
    return Ref.MapFinished();
}

function Msg(msg) {
    Ref.SendMessage(msg);
    return true;
}

function PlayPhase() {
    var state = Ref.GetState();
    Msg("PlayPhase state " + state);

    switch (state) {
        default:
            return true;

        case 0:
            Ref.RequestCurrentPick();
            Ref.SetState(1);
            return false;

        case 1:
            if (GetPick()) {
                Ref.SetState(2);
                Ref.Play();
            }

            return false;

        case 2:
            if (PickAndSelectMap()) {
                Ref.SetState(3);
            }

            return false;

        case 3:
            if (Ref.MapFinished()) {
                Ref.SetState(0);
                Ref.SetNextCaptainPick();

                return true;
            }

            return false;
    }

    return false;
}

function SubmitResult() {
    Ref.SubmitResults();
}

function CloseLobby() {
    Ref.CloseLobby();
}

function Main() {
    Workflow.AddStep(() => Msg("First step, waiting for lobby creation"));
    Workflow.AddStep(() => Wait(5 * 60)); //wait till 5 minutes after lobby creation has happened
    Workflow.AddStep(() => Msg("Inviting players"));
    Workflow.AddStep(InvitePlayers); 

    Workflow.AddStep(() => Msg("Waiting for match start"));
    Workflow.AddStep(() => Wait(10 * 60));
    Workflow.AddStep(() => Msg("Setting up teams"));
    Workflow.AddStep(SetupTeams);

    Workflow.AddStep(() => Msg("Requesting rolls"));
    Workflow.AddStep(RequestRolls);

    Workflow.AddStep(() => Msg("Requesting pick"));
    Workflow.AddStep(RequestPick);
    Workflow.AddStep(() => Msg("Getting ban"));
    Workflow.AddStep(GetBan);
    Workflow.AddStep(() => Msg("Requesting pick"));
    Workflow.AddStep(RequestPick);
    Workflow.AddStep(() => Msg("Requesting ban"));
    Workflow.AddStep(GetBan);

    Workflow.AddStep(() => Msg("Play Phase"));
    Workflow.AddStep(PlayPhase);
    Workflow.AddStep(() => Msg("Submit results"));
    Workflow.AddStep(SubmitResults);
    Workflow.AddStep(() => Msg("Closing lobby"));
    Workflow.AddStep(CloseLobby);
}

function SoloTest() {
    Workflow.AddStep(() => Msg("First step, waiting for lobby creation"));
    Workflow.AddStep(() => Wait(10)); //wait till 10 seconds after lobby creation has happened
    Workflow.AddStep(() => Msg("Inviting players"));
    Workflow.AddStep(InvitePlayers);

    Workflow.AddStep(() => Msg("Waiting for match start"));
    Workflow.AddStep(() => Wait(20));
    Workflow.AddStep(() => Msg("Closing lobby"));
    Workflow.AddStep(CloseLobby);
}

function MainTest() {
    Workflow.AddStep(() => Msg("First step, waiting for lobby creation"));
    Workflow.AddStep(() => Wait(10)); //wait till 10 seconds after lobby creation has happened
    Workflow.AddStep(() => Msg("Inviting players"));
    Workflow.AddStep(InvitePlayers);

    Workflow.AddStep(() => Msg("Waiting for match start"));
    Workflow.AddStep(() => Wait(20));
    Workflow.AddStep(() => Msg("Setting up teams"));
    Workflow.AddStep(SetupTeams);

    Workflow.AddStep(() => Msg("Requesting rolls"));
    Workflow.AddStep(RequestRolls);

    Workflow.AddStep(() => Msg("Requesting pick"));
    Workflow.AddStep(RequestPick);
    Workflow.AddStep(() => Msg("Getting ban"));
    Workflow.AddStep(GetBan);
    Workflow.AddStep(() => Msg("Requesting pick"));
    Workflow.AddStep(RequestPick);
    Workflow.AddStep(() => Msg("Requesting ban"));
    Workflow.AddStep(GetBan);

    Workflow.AddStep(() => Msg("Finished Test Script"));
    Workflow.AddStep(() => Msg("Closing lobby"));
    Workflow.AddStep(CloseLobby);
}

MainTest();
//Main();