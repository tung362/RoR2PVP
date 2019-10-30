using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
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
