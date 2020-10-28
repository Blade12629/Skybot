
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
    Lobby.SetTeamColors();
    return true;
}

function RequestRolls() {
    Ref.RequestRolls();
    return true;
}

function WaitForRolls() {
    const firstPick = Ref.GetFirstPick();

    if (firstPick == null)
        return false;

    return true;
}

function GetPick(ban) {
    const lastPick = Ref.GetLastPick();

    if (lastPick == 0 || !Ref.IsLastPickValid())
        return false;

    if (ban)
        Ref.Ban(lastPick);
    else
        Ref.Pick(lastPick);

    Ref.SetNextCaptainPick();

    return true;
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

function PlayPhase() {
    var state = Ref.GetState();

    switch (state) {
        default:
            return true;

        case 0:
            if (GetPick()) {
                Ref.SetState(1);
                Ref.Play();
            }

            return false;

        case 1:
            if (PickAndSelectMap()) {
                Ref.SetState(2);
            }

            return false;

        case 2:
            if (Ref.MapFinished()) {
                Ref.SetState(0);

                return true;
            }

            return false;
    }
}

function SubmitResult() {
    Ref.SubmitResults();
}

function Main() {
    //5 ticks, each time a workflow step gets invoked wait X seconds to invoke the next step
    //1000 MS / desiredDelay MS = tickrate
    //This would set our delay to 200 ms, 100 is the minimum and 1 second is the max,
    //it's totally optional to set this up and the default value is 4 ticks (250 ms delay)
    Workflow.SetTicks(5); 

    Workflow.AddStep(Wait(5 * 60)); //wait till 5 minutes after lobby creation has happened
    Workflow.AddStep(InvitePlayers()); 

    Workflow.AddStep(Wait(10 * 60));
    Workflow.AddStep(SetupTeams());

    Workflow.AddStep(RequestRolls());
    Workflow.AddStep(WaitForRolls());

    Workflow.AddStep(GetBan());
    Workflow.AddStep(GetBan());

    Workflow.AddStep(PlayPhase());
    Workflow.AddStep(SubmitResults());
}

Main();