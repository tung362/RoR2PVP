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
using RoR2GameModeAPI;
using RoR2GameModeAPI.Utils;

namespace RoR2PVP
{
    /// <summary>
    /// Mod entry point, provides plugin information
    /// </summary>
    [BepInDependency("com.bepis.r2api")]
    [BepInDependency("GameModeAPI")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync)]
    [R2APISubmoduleDependency(new string[]
    {
        "AssetPlus",
        "ResourcesAPI",
    })]
    class RoR2PVP : BaseUnityPlugin
    {
        internal static RoR2PVP instance { get; private set; }

        /*Mod Information*/
        public const string PluginGUID = "PVP";
        public const string PluginName = "PVP Mode";
        public const string PluginVersion = "1.6.0";

        /// <summary>
        /// Constructor, automatically gets created when BepInEx loads the API
        /// </summary>
        public RoR2PVP()
        {
            if (!instance) instance = this;
            else Debug.LogWarning("Warning! Multiple instances of \"RoR2PVP\" @RoR2PVP");

            //Pre core inits
            Settings.Init();

            //Pre core hooks
            SceneManager.sceneLoaded += Init;
        }

        /// <summary>
        /// Loads configs and init core hooks when application has reached the title scene
        /// </summary>
        /// <param name="scene">Loaded scene</param>
        /// <param name="loadSceneMode">Loaded scene mode</param>
        void Init(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (scene.name == "title")
            {
                if (BodyCatalog.availability.available &&
                    ItemCatalog.availability.available &&
                    EquipmentCatalog.availability.available)
                {
                    //Loads Config Settings
                    Settings.LoadConfig(Config);
                    Settings.LoadCustomPlayableCharactersConfig($"{Settings.ConfigRootPath}PVPCustomPlayableCharacters.cfg");
                    Settings.LoadBannedItemListConfig($"{Settings.ConfigRootPath}PVPBannedItemList.cfg");
                    Settings.LoadCustomInteractablesSpawnerConfig(new ConfigFile($"{Settings.ConfigRootPath}PVPCustomInteractablesSpawner.cfg", true));

                    //Setup
                    Hooks.Init();
                    Debug.Log("RoR2PVP setup completed @RoR2PVP");
                }
                else Debug.LogError("Failed to load RoR2PVP, please let the developer know on \"https://github.com/tung362/RoR2PVP/issues\" @RoR2PVP");
                SceneManager.sceneLoaded -= Init;
            }
        }
    }
}
