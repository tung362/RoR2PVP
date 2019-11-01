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
        public static int MaxMultiplayerCount = 4;

        //PVP settings
        public static float GraceTimerDuration = 120;
        public static float CashDelay = 10;
        public static uint CashGrantAmount = 50u;
        public static int RespawnsPerRound = 2;
        public static bool RandomTeams = true;
        public static bool CompanionsShareItems = true;
        public static bool DisableMobSpawn = true;
        public static bool CustomInteractablesSpawner = true;
        public static bool UseDeathPlaneFailsafe = true;

        //Characters settings
        public static bool CustomPlayableCharacters = true;
        public static List<string> PlayableCharactersList = new List<string>();

        //Ban item settings
        public static bool BanItems = true;
        public static List<string> BannedItemList = new List<string>();

        //Interactables Spawner Settings
        //Drones
        public static int MegaDroneAmount = 0;
        public static int MegaDronePrice = 300;
        public static int GunnerDroneAmount = 0;
        public static int GunnerDronePrice = -1;
        public static int MissileDroneAmount = 0;
        public static int MissileDronePrice = -1;
        public static int HealerDroneAmount = 8;
        public static int HealerDronePrice = -1;
        public static int EquipmentDroneAmount = 0;
        public static int FlameDroneAmount = 0;
        public static int FlameDronePrice = -1;
        public static int TurretAmount = 0;
        public static int TurretPrice = -1;
        //Shrines
        public static int ShrineOfOrderAmount = 2;
        public static int ShrineOfBloodAmount = 0;
        public static int ShrineOfChanceAmount = 3;
        public static int ShrineOfChancePrice = -1;
        public static int ShrineOfCombatAmount = 0;
        public static int ShrineOfHealingAmount = 0;
        public static int ShrineOfHealingPrice = -1;
        public static int GoldShrineAmount = 0;
        public static int GoldShrinePrice = -1;
        //Misc
        public static int CapsuleAmount = 0;
        public static int RadarTowerAmount = 1;
        public static int RadarTowerPrice = -1;
        public static int CelestialPortalAmount = 0;
        public static int ShopPortalAmount = 0;
        //Duplicators
        public static int DuplicatorAmount = 2;
        public static int DuplicatorLargeAmount = 1;
        public static int DuplicatorMilitaryAmount = 0;
        //Chests
        public static int GoldChestAmount = 2;
        public static int GoldChestPrice = -1;
        public static int SmallChestAmount = 16;
        public static int SmallChestPrice = -1;
        public static int LargeChestAmount = 8;
        public static int LargeChestPrice = -1;
        public static int DamageChestAmount = 4;
        public static int DamageChestPrice = -1;
        public static int HealingChestAmount = 4;
        public static int HealingChestPrice = -1;
        public static int UtilityChestAmount = 4;
        public static int UtilityChestPrice = -1;
        public static int TripleShopAmount = 3;
        public static int TripleShopPrice = -1;
        public static int TripleShopLargeAmount = 3;
        public static int TripleShopLargePrice = -1;
        public static int EquipmentBarrelAmount = 6;
        public static int EquipmentBarrelPrice = -1;
        public static int LockboxAmount = 4;
        public static int LunarChestAmount = 4;

        static List<string> OutputBodies()
        {
            List<string> bodiesList = new List<string>();
            //Character slots
            bodiesList.Add("# Custom playable character for commando slot");
            bodiesList.Add("CommandoBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for MUL-T slot");
            bodiesList.Add("CommandoBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Huntress slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Engineer slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Artificer slot");
            bodiesList.Add("BanditBody");
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
            //Multiplayer settings
            MaxMultiplayerCount = config.Wrap<int>("Multiplayer Settings", "Max Multiplayer Count", "Max amount of players that can join your game (16 max)", MaxMultiplayerCount).Value;

            //PVP settings
            GraceTimerDuration = config.Wrap<float>("PVP Settings", "Grace Timer Duration", "Grace period duration before enabling pvp in seconds", GraceTimerDuration).Value;
            CashDelay = config.Wrap<float>("PVP Settings", "Cash Delay", "Cash grant delay in seconds", CashDelay).Value;
            CashGrantAmount = config.Wrap<uint>("PVP Settings", "Cash Grant Amount", "Amount of cash granted after each delay", CashGrantAmount).Value;
            RespawnsPerRound = config.Wrap<int>("PVP Settings", "Respawns Per Round", "Amount of free revives per round", RespawnsPerRound).Value;
            RandomTeams = config.Wrap<bool>("PVP Settings", "Random Teams", "Shuffles team members every round", RandomTeams).Value;
            CompanionsShareItems = config.Wrap<bool>("PVP Settings", "Companions Share Items", "Companions(drones, etc) share items with their owner", CompanionsShareItems).Value;
            DisableMobSpawn = config.Wrap<bool>("PVP Settings", "Disable Mob Spawn", "Disables mobs from spawning in this game mode", DisableMobSpawn).Value;
            UseDeathPlaneFailsafe = config.Wrap<bool>("PVP Settings", "Use Death Plane Failsafe", "Creates a kill zone at height -2200 just in case the vanilla kill zone doesn't stop the player from falling off the map, prevents softlock", UseDeathPlaneFailsafe).Value;

            //Characters Settings
            CustomPlayableCharacters = config.Wrap<bool>("Characters Settings", "Custom Playable Characters", "Enables the ability to change the playable characters in the character select menu with custom picked characters when starting the game (see TeamPVPCustomPlayableCharacters.cfg)", CustomPlayableCharacters).Value;

            //Ban items setting
            BanItems = config.Wrap<bool>("Ban Item Settings", "Ban Items", "Enables the ability to ban certain items from this game mode to balance it more (see TeamPVPBannedItemList.cfg)", BanItems).Value;

            //Interactables Spawner Settings
            CustomInteractablesSpawner = config.Wrap<bool>("Interactables Spawner Settings", "Custom Interactables Spawner", "Use custom interactables(chests, drones, etc) spawners instead of default ones in this game mode (See TeamPVPCustomInteractablesSpawner.cfg)", CustomInteractablesSpawner).Value;
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
            //Init
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

        public static void LoadCustomInteractablesSpawnerConfig(ConfigFile config)
        {
            //Drones
            MegaDroneAmount = config.Wrap<int>("Drones", "Mega Drone Amount", "Amount to attempt to spawn in each stage", MegaDroneAmount).Value;
            MegaDronePrice = config.Wrap<int>("Drones", "Mega Drone Price", "custom cost to buy, leave at -1 for vanilla cost", MegaDronePrice).Value;
            GunnerDroneAmount = config.Wrap<int>("Drones", "Gunner Drone Amount", "Amount to attempt to spawn in each stage", GunnerDroneAmount).Value;
            GunnerDronePrice = config.Wrap<int>("Drones", "Gunner Drone Price", "custom cost to buy, leave at -1 for vanilla cost", GunnerDronePrice).Value;
            MissileDroneAmount = config.Wrap<int>("Drones", "Missile Drone Amount", "Amount to attempt to spawn in each stage", MissileDroneAmount).Value;
            MissileDronePrice = config.Wrap<int>("Drones", "Missile Drone Price", "custom cost to buy, leave at -1 for vanilla cost", MissileDronePrice).Value;
            HealerDroneAmount = config.Wrap<int>("Drones", "Healer Drone Amount", "Amount to attempt to spawn in each stage", HealerDroneAmount).Value;
            HealerDronePrice = config.Wrap<int>("Drones", "Healer Drone Price", "custom cost to buy, leave at -1 for vanilla cost", HealerDronePrice).Value;
            EquipmentDroneAmount = config.Wrap<int>("Drones", "Equipment Drone Amount", "Amount to attempt to spawn in each stage", EquipmentDroneAmount).Value;
            FlameDroneAmount = config.Wrap<int>("Drones", "Flame Drone Amount", "Amount to attempt to spawn in each stage", FlameDroneAmount).Value;
            FlameDronePrice = config.Wrap<int>("Drones", "Flame Drone Price", "custom cost to buy, leave at -1 for vanilla cost", FlameDronePrice).Value;
            TurretAmount = config.Wrap<int>("Drones", "Turret Amount", "Amount to attempt to spawn in each stage", TurretAmount).Value;
            TurretPrice = config.Wrap<int>("Drones", "Turret Price", "custom cost to buy, leave at -1 for vanilla cost", TurretPrice).Value;
            //Shrines
            ShrineOfOrderAmount = config.Wrap<int>("Shrines", "Shrine Of Order Amount", "Amount to attempt to spawn in each stage", ShrineOfOrderAmount).Value;
            ShrineOfBloodAmount = config.Wrap<int>("Shrines", "Shrine Of Blood Amount", "Amount to attempt to spawn in each stage", ShrineOfBloodAmount).Value;
            ShrineOfChanceAmount = config.Wrap<int>("Shrines", "Shrine Of Chance Amount", "Amount to attempt to spawn in each stage", ShrineOfChanceAmount).Value;
            ShrineOfChancePrice = config.Wrap<int>("Shrines", "Shrine Of Chance Price", "custom cost to buy, leave at -1 for vanilla cost", ShrineOfChancePrice).Value;
            ShrineOfCombatAmount = config.Wrap<int>("Shrines", "Shrine Of Combat Amount", "Amount to attempt to spawn in each stage", ShrineOfCombatAmount).Value;
            ShrineOfHealingAmount = config.Wrap<int>("Shrines", "Shrine Of Healing Amount", "Amount to attempt to spawn in each stage", ShrineOfHealingAmount).Value;
            ShrineOfHealingPrice = config.Wrap<int>("Shrines", "Shrine Of Healing Price", "custom cost to buy, leave at -1 for vanilla cost", ShrineOfHealingPrice).Value;
            GoldShrineAmount = config.Wrap<int>("Shrines", "Gold Shrine Amount", "Amount to attempt to spawn in each stage", GoldShrineAmount).Value;
            GoldShrinePrice = config.Wrap<int>("Shrines", "Gold Shrine Price", "custom cost to buy, leave at -1 for vanilla cost", GoldShrinePrice).Value;
            //Misc
            CapsuleAmount = config.Wrap<int>("Misc", "Capsule Amount", "Amount to attempt to spawn in each stage", CapsuleAmount).Value;
            RadarTowerAmount = config.Wrap<int>("Misc", "Radar Tower Amount", "Amount to attempt to spawn in each stage", RadarTowerAmount).Value;
            RadarTowerPrice = config.Wrap<int>("Misc", "Radar Tower Price", "custom cost to buy, leave at -1 for vanilla cost", RadarTowerPrice).Value;
            CelestialPortalAmount = config.Wrap<int>("Misc", "Celestial Portal Amount", "Amount to attempt to spawn in each stage", CelestialPortalAmount).Value;
            ShopPortalAmount = config.Wrap<int>("Misc", "Shop Portal Amount", "Amount to attempt to spawn in each stage", ShopPortalAmount).Value;
            //Duplicators
            DuplicatorAmount = config.Wrap<int>("Duplicators", "Duplicator Amount", "Amount to attempt to spawn in each stage", DuplicatorAmount).Value;
            DuplicatorLargeAmount = config.Wrap<int>("Duplicators", "Duplicator Large Amount", "Amount to attempt to spawn in each stage", DuplicatorLargeAmount).Value;
            DuplicatorMilitaryAmount = config.Wrap<int>("Duplicators", "Duplicator Military Amount", "Amount to attempt to spawn in each stage", DuplicatorMilitaryAmount).Value;
            //Chests
            GoldChestAmount = config.Wrap<int>("Chests", "Gold Chest Amount", "Amount to attempt to spawn in each stage", GoldChestAmount).Value;
            GoldChestPrice = config.Wrap<int>("Chests", "Gold Chest Price", "custom cost to buy, leave at -1 for vanilla cost", GoldChestPrice).Value;
            SmallChestAmount = config.Wrap<int>("Chests", "Small Chest Amount", "Amount to attempt to spawn in each stage", SmallChestAmount).Value;
            SmallChestPrice = config.Wrap<int>("Chests", "Small Chest Price", "custom cost to buy, leave at -1 for vanilla cost", SmallChestPrice).Value;
            LargeChestAmount = config.Wrap<int>("Chests", "Large Chest Amount", "Amount to attempt to spawn in each stage", LargeChestAmount).Value;
            LargeChestPrice = config.Wrap<int>("Chests", "Large Chest Price", "custom cost to buy, leave at -1 for vanilla cost", LargeChestPrice).Value;
            DamageChestAmount = config.Wrap<int>("Chests", "Damage Chest Amount", "Amount to attempt to spawn in each stage", DamageChestAmount).Value;
            DamageChestPrice = config.Wrap<int>("Chests", "Damage Chest Price", "custom cost to buy, leave at -1 for vanilla cost", DamageChestPrice).Value;
            HealingChestAmount = config.Wrap<int>("Chests", "Healing Chest Amount", "Amount to attempt to spawn in each stage", HealingChestAmount).Value;
            HealingChestPrice = config.Wrap<int>("Chests", "Healing Chest Price", "custom cost to buy, leave at -1 for vanilla cost", HealingChestPrice).Value;
            UtilityChestAmount = config.Wrap<int>("Chests", "Utility Chest Amount", "Amount to attempt to spawn in each stage", UtilityChestAmount).Value;
            UtilityChestPrice = config.Wrap<int>("Chests", "Utility Chest Price", "custom cost to buy, leave at -1 for vanilla cost", UtilityChestPrice).Value;
            TripleShopAmount = config.Wrap<int>("Chests", "Triple Shop Amount", "Amount to attempt to spawn in each stage", TripleShopAmount).Value;
            TripleShopPrice = config.Wrap<int>("Chests", "Triple Shop Price", "custom cost to buy, leave at -1 for vanilla cost", TripleShopPrice).Value;
            TripleShopLargeAmount = config.Wrap<int>("Chests", "Triple Shop Large Amount", "Amount to attempt to spawn in each stage", TripleShopLargeAmount).Value;
            TripleShopLargePrice = config.Wrap<int>("Chests", "Triple Shop Large Price", "custom cost to buy, leave at -1 for vanilla cost", TripleShopLargePrice).Value;
            EquipmentBarrelAmount = config.Wrap<int>("Chests", "Equipment Barrel Amount", "Amount to attempt to spawn in each stage", EquipmentBarrelAmount).Value;
            EquipmentBarrelPrice = config.Wrap<int>("Chests", "Equipment Barrel Price", "custom cost to buy, leave at -1 for vanilla cost", EquipmentBarrelPrice).Value;
            LockboxAmount = config.Wrap<int>("Chests", "Lockbox Amount", "Amount to attempt to spawn in each stage", LockboxAmount).Value;
            LunarChestAmount = config.Wrap<int>("Chests", "Lunar Chest Amount", "Amount to attempt to spawn in each stage", LunarChestAmount).Value;
        }
    }
}
