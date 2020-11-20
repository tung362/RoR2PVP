using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using TMPro;
using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using R2API.Utils;
using RoR2;
using APIExtension.VoteAPI;
using R2API;
using UnityEngine.SceneManagement;

namespace RoR2PVP
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("PVP", "PVP Mode", "1.5.1")]
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
            ConfigRootPath = ConfigRootPath.Remove(Config.ConfigFilePath.Count() - "PVP.cfg".Length);

            /*Compatibility*/

            /*API Extenstions*/
            VoteAPI.SetHook();
            /*Mod menu options*/
            Hooks.Init();
        }

        public void Start()
        {
            /*Loads Config Settings*/
            Settings.LoadConfig(Config);
            Settings.LoadCustomPlayableCharactersConfig(ConfigRootPath + "PVPCustomPlayableCharacters.cfg");
            Settings.LoadBannedItemListConfig(ConfigRootPath + "PVPBannedItemList.cfg");
            Settings.LoadCustomInteractablesSpawnerConfig(new ConfigFile(ConfigRootPath + "PVPCustomInteractablesSpawner.cfg", true));
            /*Setup*/
            Hooks.SetupHook();
        }
    }
}
