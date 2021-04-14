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
using UnityEngine.SceneManagement;
using TMPro;
using BepInEx;
using BepInEx.Configuration;
using Mono.Cecil.Cil;
using RoR2;
using RoR2.ContentManagement;
using HG.Reflection;
using R2API;
using R2API.Utils;
using APIExtension.VoteAPI;

namespace RoR2PVP
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("PVP", "PVP Mode", "1.5.2")]
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
            //Let me know if there's a better way of doing this
            SceneManager.sceneLoaded += ModSetup;
        }

        void ModSetup(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "title")
            {
                if (BodyCatalog.availability.available &&
                    ItemCatalog.availability.available &&
                    EquipmentCatalog.availability.available)
                {
                    /*Loads Config Settings*/
                    Settings.LoadConfig(Config);
                    Settings.LoadCustomPlayableCharactersConfig(ConfigRootPath + "PVPCustomPlayableCharacters.cfg");
                    Settings.LoadBannedItemListConfig(ConfigRootPath + "PVPBannedItemList.cfg");
                    Settings.LoadCustomInteractablesSpawnerConfig(new ConfigFile(ConfigRootPath + "PVPCustomInteractablesSpawner.cfg", true));
                    /*Setup*/
                    Hooks.SetupHook();
                    Debug.Log("RoR2PVP mod setup completed");
                }
                else Debug.LogError("Failed to load RoR2PVP mod, please let the developer know on \"https://github.com/tung362/RoR2PVP/issues\"");
                SceneManager.sceneLoaded -= ModSetup;
            }
        }
    }
}
