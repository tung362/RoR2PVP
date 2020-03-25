using RoR2;

namespace RoR2PVP
{
    public struct PVPTeamTrackerStruct
    {
        public PVPTeamTrackerStruct(string newPlayerName, TeamIndex newRootTeam)
        {
            this.PlayerName = newPlayerName;
            this.RootTeam = newRootTeam;
        }

        public TeamIndex RootTeam;

        public string PlayerName;
    }
}
