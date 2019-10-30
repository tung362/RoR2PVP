using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;

namespace RoR2PVP
{
    [BepInPlugin("TeamPVP", "Team PVP Mode", "1.0.0")]
    public class Setup : BaseUnityPlugin
    {
        public static string ConfigRootPath;

        public void Awake()
        {
            //Get root path
            ConfigRootPath = Config.ConfigFilePath;
            ConfigRootPath = ConfigRootPath.Remove(Config.ConfigFilePath.Count() - "TeamPVP.cfg".Length);
        }

        public void Start()
        {
            Settings.LoadConfig(Config);
            Settings.LoadCustomPlayableCharactersConfig(ConfigRootPath + "TeamPVPCustomPlayableCharacters.cfg");
            Settings.LoadBannedItemListConfig(ConfigRootPath + "TeamPVPBannedItemList.cfg");
            Hooks.Preassign();
            Hooks.CoreHooks();
            Hooks.ExtraHooks();
        }
    }
}
