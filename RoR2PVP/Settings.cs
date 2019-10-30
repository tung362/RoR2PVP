using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using RoR2;

namespace RoR2PVP
{
    class Settings
    {
        /*In-game*/
        public static bool IsGracePeriod = true;
        public static float GraceTimer;
        public static float CashGrantTimer = 0;
        public static float CurrentGraceTimeReminder;
        public static bool PVPEnded = false;
        public static bool UsedTeleporter = false;
        //Unused for now
        public static List<PVPTeamTrackerStruct> PVPTeams = new List<PVPTeamTrackerStruct>();

        /*Config*/
        //Multiplayer settings
        public static bool BypassAPIRestrictions = false;
        public static bool ForceHost = false;
        public static int MaxMultiplayerCount = 4;
        //PVP settings
        public static float GraceTimerDuration = 120;
        public static float CashDelay = 10;
        public static uint CashGrantAmount = 50u;
        public static int RespawnsPerRound = 2;
        public static bool RandomTeams = true;
        public static bool CompanionsShareItems = true;
        public static bool DisableMobSpawn = true;
        public static bool CustomInteractablesSpawner = true; //Unfinished
        //Characters settings
        public static bool CustomPlayableCharacters = true;
        public static List<string> PlayableCharactersList = new List<string>();
        //Ban item settings
        public static bool BanItems = true;
        public static List<string> BannedItemList = new List<string>();

        static List<string> OutputBodies()
        {
            List<string> bodiesList = new List<string>();
            //Character slots
            bodiesList.Add("# Custom playable character for commando slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for MUL-T slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Huntress slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Engineer slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Artificer slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Mercenary slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for REX slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Loader slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            //Output all body names
            bodiesList.Add("# -----------------------------------------------------------------------------------------");
            bodiesList.Add("# List of bodies for selecting custom characters for each playable character slot (copy and paste to a slot)");
            bodiesList.Add("# -----------------------------------------------------------------------------------------");
            List<GameObject> bodies = BodyCatalog.allBodyPrefabs.ToList();
            for (int i = 0; i < bodies.Count; i++)
            {
                bodiesList.Add("# " + bodies[i].name);
            }
            return bodiesList;
        }

        static List<string> DefaultBannedItemsList()
        {
            List<string> bannedItems = new List<string>();
            bannedItems.Add("# Make sure each item id is on it's own line");
            bannedItems.Add("# Do not paste item's full name, only paste the id provided below");
            //Items
            bannedItems.Add("Behemoth");
            bannedItems.Add("NovaOnHeal");
            bannedItems.Add("ShockNearby");
            bannedItems.Add("SprintWisp");
            //Equipment
            bannedItems.Add("Meteor");
            bannedItems.Add("BurnNearby");
            bannedItems.Add("BFG");
            bannedItems.Add("Blackhole");
            bannedItems.Add("Lightning");
            bannedItems.Add("CommandMissile");
            bannedItems.Add("FireBallDash");
            bannedItems.Add("GoldGat");
            //Output all item names
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            bannedItems.Add("# List of item ids and it's full name");
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            for (int i = 0; i < (int)ItemIndex.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef((ItemIndex)i);
                bannedItems.Add("# ID: " + (ItemIndex)i + " FullName: " + Language.GetString(itemDef.nameToken));
            }
            //Output all equipment names
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            bannedItems.Add("# List of equipment ids and it's full name");
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            for (int i = 0; i < (int)EquipmentIndex.Count; i++)
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)i);
                bannedItems.Add("# ID: " + (EquipmentIndex)i + " FullName: " + Language.GetString(equipmentDef.nameToken));
            }
            return bannedItems;
        }

        public static void LoadConfig(ConfigFile config)
        {
            //Init

            //Multiplayer settings
            BypassAPIRestrictions = config.Wrap<bool>("Multiplayer Settings", "Bypass API Restrictions", "Not for the faint of heart", false).Value;
            ForceHost = config.Wrap<bool>("Multiplayer Settings", "Force Host", "Go back before its too late", false).Value;
            MaxMultiplayerCount = config.Wrap<int>("Multiplayer Settings", "Max Multiplayer Count", "Max amount of players that can join your game (16 max)", 4).Value;

            //PVP settings
            GraceTimerDuration = config.Wrap<float>("PVP Settings", "Grace Timer Duration", "Grace period duration before enabling pvp in seconds", 120).Value;
            CashDelay = config.Wrap<float>("PVP Settings", "Cash Delay", "Cash grant delay in seconds", 10).Value;
            CashGrantAmount = config.Wrap<uint>("PVP Settings", "Cash Grant Amount", "Amount of cash granted after each delay", 50u).Value;
            RespawnsPerRound = config.Wrap<int>("PVP Settings", "Respawns Per Round", "Amount of free revives per round", 2).Value;
            RandomTeams = config.Wrap<bool>("PVP Settings", "Random Teams", "Shuffles team members every round", true).Value;
            CompanionsShareItems = config.Wrap<bool>("PVP Settings", "Companions Share Items", "Companions(drones, etc) share items with their owner", true).Value;
            DisableMobSpawn = config.Wrap<bool>("PVP Settings", "Disable Mob Spawn", "Disables mobs from spawning in this game mode", true).Value;
            CustomInteractablesSpawner = config.Wrap<bool>("PVP Settings", "Custom Interactables Spawner", "Use custom interactables(chests, drones, etc) spawners instead of default ones in this game mode (changing not recommended)", true).Value;

            //Characters Settings
            CustomPlayableCharacters = config.Wrap<bool>("Characters Settings", "Custom Playable Characters", "Enables the ability to change the playable characters in the character select menu with custom picked characters when starting the game (see TeamPVPCustomPlayableCharacters.cfg)", true).Value;

            //Ban items setting
            BanItems = config.Wrap<bool>("Ban Item Settings", "Ban Items", "Enables the ability to ban certain items from this game mode to balance it more (see TeamPVPBannedItemList.cfg)", true).Value;
        }

        public static void LoadCustomPlayableCharactersConfig(string configPath)
        {
            //Init
            bool firstTime = false;

            //If config doesn't exist
            if (!File.Exists(configPath)) firstTime = true;

            //If first time then fill config with default values
            if (firstTime)
            {
                //Create config with default values
                List<string> config = OutputBodies();
                File.WriteAllLines(configPath, config);
            }

            //Load config
            string[] configLines = File.ReadAllLines(configPath);
            for (int i = 0; i < configLines.Length; i++)
            {
                //If not empty
                if (configLines[i].Length != 0)
                {
                    //If not a comment
                    if (configLines[i][0] != '#') PlayableCharactersList.Add(configLines[i]);
                }
            }
        }

        public static void LoadBannedItemListConfig(string configPath)
        {
            bool firstTime = false;

            //If config doesn't exist
            if (!File.Exists(configPath)) firstTime = true;

            //If first time then fill config with default banned items
            if (firstTime)
            {
                //Create config with default banned items
                List<string> config = DefaultBannedItemsList();
                File.WriteAllLines(configPath, config);
            }

            //Load config
            string[] configLines = File.ReadAllLines(configPath);
            for (int i = 0; i < configLines.Length; i++)
            {
                //If not empty
                if(configLines[i].Length != 0)
                {
                    //If not a comment
                    if (configLines[i][0] != '#') BannedItemList.Add(configLines[i]);
                }
            }
        }
    }
}
