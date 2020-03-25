using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using RoR2;
using APIExtension.VoteAPI;

namespace RoR2PVP
{
    [BepInPlugin("TeamPVP", "Team PVP Mode", "1.3.0")]
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

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.K))
            {
                List<PlayerCharacterMasterController> players = Enumerable.ToList<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
                Debug.Log("Player Count: " + players.Count);
                if (players[1].master.alive && players[1].master.GetBody() != null)
                {
                    players[1].master.teamIndex = TeamIndex.Monster;
                    players[1].master.GetBody().teamComponent.teamIndex = TeamIndex.Monster;
                }
            }
        }
    }
}
