using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using APIExtension.VoteAPI;

namespace RoR2PVP
{
    [BepInPlugin("TeamPVP", "Team PVP Mode", "1.4.3")]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [R2APISubmoduleDependency(new string[]
    {
        "AssetPlus",
        "ResourcesAPI",
    })]
    public class Setup : BaseUnityPlugin
    {
        public static string ConfigRootPath;

        public void Awake()
        {
            //Get root path
            ConfigRootPath = Config.ConfigFilePath;
            ConfigRootPath = ConfigRootPath.Remove(Config.ConfigFilePath.Count() - "TeamPVP.cfg".Length);

            /*API Extenstions*/
            VoteAPI.SetHook();

            /*Mod menu options*/
            Hooks.Init();
        }

        public void Start()
        {
            /*Loads Config Settings*/
            Settings.LoadConfig(Config);
            Settings.LoadCustomPlayableCharactersConfig(ConfigRootPath + "TeamPVPCustomPlayableCharacters.cfg");
            Settings.LoadBannedItemListConfig(ConfigRootPath + "TeamPVPBannedItemList.cfg");
            Settings.LoadCustomInteractablesSpawnerConfig(new ConfigFile(ConfigRootPath + "TeamPVPCustomInteractablesSpawner.cfg", true));
            /*Setup*/
            Hooks.SetupHook();
        }
    }
}
