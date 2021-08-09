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
using RoR2;
using RoR2.ContentManagement;
using R2API;
using R2API.Utils;
using RoR2GameModeAPI;
using RoR2GameModeAPI.Utils;
using RoR2PVP.GameModes;

namespace RoR2PVP
{
    /// <summary>
    /// Keeps track of databases and cached values used by the mod along with handling the pre core inits
    /// </summary>
    public static class Settings
    {
        /*Paths*/
        public static string ConfigRootPath
        {
            get
            {
                string configPath = RoR2PVP.instance.Config.ConfigFilePath;
                return configPath.Remove(configPath.Count() - $"{RoR2PVP.PluginGUID}.cfg".Length);
            }
        }

        /*Assets*/
        public static AssetBundle Assets;
        public static AssetBundleResourcesProvider Provider;

        /*Votes*/
        public const string VotePollName = "RoR2PVPVotePoll";
        //Default values and registered vote indexes for voteable mod options
        public static readonly Tuple<bool, int> RandomTeams = Tuple.Create(true, 1);
        public static readonly Tuple<bool, int> MobSpawn = Tuple.Create(false, 2);
        public static readonly Tuple<bool, int> BanItems = Tuple.Create(true, 3);
        public static readonly Tuple<bool, int> CompanionsShareItems = Tuple.Create(true, 4);
        public static readonly Tuple<bool, int> CustomPlayableCharacters = Tuple.Create(true, 5);
        public static readonly Tuple<bool, int> CustomInteractablesSpawner = Tuple.Create(true, 6);
        public static readonly Tuple<bool, int> UseDeathPlaneFailsafe = Tuple.Create(true, 7);
        public static readonly Tuple<bool, int> WiderStageTransitions = Tuple.Create(false, 8);

        /*Game Mode Information*/
        public static TeamGameMode TeamPVPGameMode = new TeamGameMode("Team PVP");
        public static FreeForAllGameMode FreeForAllPVPGameMode = new FreeForAllGameMode("Free For All");

        /*Config*/
        //PVP settings
        public static float GraceTimerDuration = 60;
        public static float CashDelay = 10;
        public static uint CashGrantAmount = 70u;
        public static int RespawnsPerRound = 2;

        //Characters settings
        public static List<string> PlayableCharactersList = new List<string>();

        //Ban item settings
        public static HashSet<ItemIndex> BannedItems = new HashSet<ItemIndex>();
        public static HashSet<EquipmentIndex> BannedEquipments = new HashSet<EquipmentIndex>();

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
        public static int GoldChestPrice = 300;
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

        /// <summary>
        /// Pre core inits. Registers vote options, game modes, and asset bundles
        /// </summary>
        public static void Init()
        {
            //Register asset bundles
            EmbeddedUtils.LoadAssetBundle("RoR2PVP.Assets.teampvp.ui", "@TeamPVP", ref Assets, ref Provider);

            //Register game mode
            GameModeAPI.RegisterGameMode("Team PVP Game Mode", "@TeamPVP:Assets/Resources/UI/FreeForAllPVPSelected.png", TeamPVPGameMode);
            GameModeAPI.RegisterGameMode("Free For All Game Mode", "@TeamPVP:Assets/Resources/UI/TeamPVPSelected.png", FreeForAllPVPGameMode);

            //Register vote option polls
            VoteAPI.AddVotePoll(VotePollName);

            //Register vote option headers
            RuleCategoryDef pvpHeader = VoteAPI.AddVoteHeader("PVP", new Color(1.0f, 0.0f, 0.0f, 1.0f), false);

            //Register vote option selections and choices
            RuleDef randomTeamsSelection = VoteAPI.AddVoteSelection(pvpHeader, "Random Teams", new ChoiceMenu("Random Teams On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Teams will be shuffled every round (only for team pvp game mode)", Color.black, "@TeamPVP:Assets/Resources/UI/RandomTeamsSelected.png", RandomTeams.Item2, VotePollName));
            VoteAPI.AddVoteChoice(randomTeamsSelection, new ChoiceMenu("Random Teams Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Teams will stay the same every round (only for team pvp game mode)", Color.black, "@TeamPVP:Assets/Resources/UI/RandomTeamsDeselected.png", -1, VotePollName));
            randomTeamsSelection.defaultChoiceIndex = Settings.RandomTeams.Item1 ? 0 : 1;

            RuleDef mobSpawnSelection = VoteAPI.AddVoteSelection(pvpHeader, "Mob Spawn", new ChoiceMenu("Mob Spawn On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Mobs will spawn", Color.black, "@TeamPVP:Assets/Resources/UI/MobSpawnSelected.png", MobSpawn.Item2, VotePollName));
            VoteAPI.AddVoteChoice(mobSpawnSelection, new ChoiceMenu("Mob Spawn Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Mobs will not spawn", Color.black, "@TeamPVP:Assets/Resources/UI/MobSpawnDeselected.png", -1, VotePollName));
            mobSpawnSelection.defaultChoiceIndex = Settings.MobSpawn.Item1 ? 0 : 1;

            RuleDef banItemsSelection = VoteAPI.AddVoteSelection(pvpHeader, "Ban Items", new ChoiceMenu("Ban Items On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Banned items configured in the config will be blacklisted", Color.black, "@TeamPVP:Assets/Resources/UI/BanItemsSelected.png", BanItems.Item2, VotePollName));
            VoteAPI.AddVoteChoice(banItemsSelection, new ChoiceMenu("Ban Items Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Banned Items will not be blacklisted", Color.black, "@TeamPVP:Assets/Resources/UI/BanItemsDeselected.png", -1, VotePollName));
            banItemsSelection.defaultChoiceIndex = Settings.BanItems.Item1 ? 0 : 1;

            RuleDef companionsShareItemsSelection = VoteAPI.AddVoteSelection(pvpHeader, "Companions Share Items", new ChoiceMenu("Companions Share Items On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Items picked up by the player will be shared with their drones etc", Color.black, "@TeamPVP:Assets/Resources/UI/CompanionsShareItemsSelected.png", CompanionsShareItems.Item2, VotePollName));
            VoteAPI.AddVoteChoice(companionsShareItemsSelection, new ChoiceMenu("Companions Share Items Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Items will not be shared with drones etc", Color.black, "@TeamPVP:Assets/Resources/UI/CompanionsShareItemsDeselected.png", -1, VotePollName));
            companionsShareItemsSelection.defaultChoiceIndex = Settings.CompanionsShareItems.Item1 ? 0 : 1;

            RuleDef customPlayableCharactersSelection = VoteAPI.AddVoteSelection(pvpHeader, "Custom Playable Characters", new ChoiceMenu("Custom Playable Characters On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Play custom characters configured in the config", Color.black, "@TeamPVP:Assets/Resources/UI/CustomPlayableCharactersSelected.png", CustomPlayableCharacters.Item2, VotePollName));
            VoteAPI.AddVoteChoice(customPlayableCharactersSelection, new ChoiceMenu("Custom Playable Characters Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Play the default vanilla characters (unbalanced)", Color.black, "@TeamPVP:Assets/Resources/UI/CustomPlayableCharactersDeselected.png", -1, VotePollName));
            customPlayableCharactersSelection.defaultChoiceIndex = Settings.CustomPlayableCharacters.Item1 ? 0 : 1;

            RuleDef customInteractablesSpawnerSelection = VoteAPI.AddVoteSelection(pvpHeader, "Custom Interactables Spawner", new ChoiceMenu("Custom Interactables Spawner On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Spawn custom objects (chests, drones, etc) at custom rates configured in the config", Color.black, "@TeamPVP:Assets/Resources/UI/CustomInteractablesSpawnerSelected.png", CustomInteractablesSpawner.Item2, VotePollName));
            VoteAPI.AddVoteChoice(customInteractablesSpawnerSelection, new ChoiceMenu("Custom Interactables Spawner Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Spawn objects (chests, drones, etc) normally as vanilla", Color.black, "@TeamPVP:Assets/Resources/UI/CustomInteractablesSpawnerDeselected.png", -1, VotePollName));
            customInteractablesSpawnerSelection.defaultChoiceIndex = Settings.CustomInteractablesSpawner.Item1 ? 0 : 1;

            RuleDef useDeathPlaneFailsafeSelection = VoteAPI.AddVoteSelection(pvpHeader, "Use Death Plane Failsafe", new ChoiceMenu("Use Death Plane Failsafe On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Force players to die should they fall off the map to prevent softlock", Color.black, "@TeamPVP:Assets/Resources/UI/UseDeathPlaneFailsafeSelected.png", UseDeathPlaneFailsafe.Item2, VotePollName));
            VoteAPI.AddVoteChoice(useDeathPlaneFailsafeSelection, new ChoiceMenu("Use Death Plane Failsafe Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Disables the death plane. Only turn off if your using a custom map!", Color.black, "@TeamPVP:Assets/Resources/UI/UseDeathPlaneFailsafeDeselected.png", -1, VotePollName));
            useDeathPlaneFailsafeSelection.defaultChoiceIndex = Settings.UseDeathPlaneFailsafe.Item1 ? 0 : 1;

            RuleDef widerStageTransitionsSelection = VoteAPI.AddVoteSelection(pvpHeader, "Wider Stage Transitions", new ChoiceMenu("Wider Stage Transitions On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Transitions through a wider selection of stages when using the teleporter", Color.black, "@TeamPVP:Assets/Resources/UI/WiderStageTransitionsSelected.png", WiderStageTransitions.Item2, VotePollName));
            VoteAPI.AddVoteChoice(widerStageTransitionsSelection, new ChoiceMenu("Wider Stage Transitions Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Vanilla stage transitions. Also turn off when using custom stages", Color.black, "@TeamPVP:Assets/Resources/UI/WiderStageTransitionsDeselected.png", -1, VotePollName));
            widerStageTransitionsSelection.defaultChoiceIndex = Settings.WiderStageTransitions.Item1 ? 0 : 1;
        }

        #region Config Generation And Loading
        /// <summary>
        /// Formats the newly created config file for loading custom playable characters
        /// </summary>
        /// <returns></returns>
        static List<string> OutputBodies()
        {
            List<string> bodiesList = new List<string>();
            //Character slots
            bodiesList.Add("# Custom playable character for Commando slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Huntress slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Bandit slot");
            bodiesList.Add("BanditBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for MUL-T slot");
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
            bodiesList.Add("# Custom playable character for Acrid slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            bodiesList.Add("# Custom playable character for Captain slot");
            bodiesList.Add("SniperBody");
            bodiesList.Add("");
            //Output all body names
            bodiesList.Add("# -----------------------------------------------------------------------------------------");
            bodiesList.Add("# List of bodies for selecting custom characters for each playable character slot (copy and paste to a slot)");
            bodiesList.Add("# -----------------------------------------------------------------------------------------");
            List<GameObject> bodies = BodyCatalog.allBodyPrefabs.ToList();
            for (int i = 0; i < BodyCatalog.bodyCount; i++)
            {
                bodiesList.Add("# " + bodies[i].name);
            }
            return bodiesList;
        }

        /// <summary>
        /// Formats the newly created config file for loading banned items
        /// </summary>
        static List<string> DefaultBannedItemsList()
        {
            List<string> bannedItems = new List<string>();
            bannedItems.Add("# Make sure each item id is on it's own line");
            bannedItems.Add("# Do not paste item's full name, only paste the id provided below");
            //Items
            bannedItems.Add("# --Items--");
            bannedItems.Add("I13"); //Brilliant Behemoth
            bannedItems.Add("I85"); //N'kuhana's Opinion
            bannedItems.Add("I105"); //Unstable Tesla Coil
            bannedItems.Add("I112"); //Little Disciple
            bannedItems.Add("I122"); //Razorwire
            bannedItems.Add("I76"); //Strides of Heresy
            //Equipment
            bannedItems.Add("# --Equipments--");
            bannedItems.Add("E39"); //Spinel Tonic
            bannedItems.Add("E29"); //Glowing Meteorite
            bannedItems.Add("E11"); //Helfire Tincture
            bannedItems.Add("E9"); //Preon Accumulator
            bannedItems.Add("E10"); //Primordial Cube
            bannedItems.Add("E27"); //Royal Capacitor
            bannedItems.Add("E13"); //Disposable Missile Launcher
            bannedItems.Add("E19"); //Volcanic Egg
            bannedItems.Add("E24"); //The Crowdfunder
            bannedItems.Add("E17"); //The Back-up
            bannedItems.Add("E16"); //Forgive Me Please
            //Output all item names
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            bannedItems.Add("# List of item ids and it's full name");
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            List<ItemIndex> items = ItemCatalog.allItems.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                ItemDef itemDef = ItemCatalog.GetItemDef(items[i]);
                bannedItems.Add("# ID: I" + itemDef.itemIndex + " FullName: " + Language.GetString(itemDef.nameToken));
            }
            //Output all equipment names
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            bannedItems.Add("# List of equipment ids and it's full name");
            bannedItems.Add("# -----------------------------------------------------------------------------------------");
            List<EquipmentIndex> equipments = EquipmentCatalog.allEquipment.ToList();
            for (int i = 0; i < equipments.Count; i++)
            {
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipments[i]);
                bannedItems.Add("# ID: E" + equipmentDef.equipmentIndex + " FullName: " + Language.GetString(equipmentDef.nameToken));
            }
            return bannedItems;
        }

        /// <summary>
        /// Create and loads the base mod config
        /// </summary>
        /// <param name="config"></param>
        public static void LoadConfig(ConfigFile config)
        {
            //PVP settings
            GraceTimerDuration = config.Bind<float>("PVP Settings", "Grace Timer Duration", GraceTimerDuration, "Grace period duration before enabling pvp in seconds").Value;
            CashDelay = config.Bind<float>("PVP Settings", "Cash Delay", CashDelay, "Cash grant delay in seconds").Value;
            CashGrantAmount = config.Bind<uint>("PVP Settings", "Cash Grant Amount", CashGrantAmount, "Amount of cash granted after each delay").Value;
            RespawnsPerRound = config.Bind<int>("PVP Settings", "Respawns Per Round", RespawnsPerRound, "Amount of free revives per round").Value;
        }

        /// <summary>
        /// Create and loads the custom playable characters config
        /// </summary>
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

        /// <summary>
        /// Create and loads the banned items config
        /// </summary>
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
                    if (configLines[i][0] != '#')
                    {
                        char identifier = char.ToUpperInvariant(configLines[i][0]);
                        string entryID = configLines[i].Substring(1, configLines[i].Length - 1);
                        if (identifier == char.ToUpperInvariant('I'))
                        {
                            if (int.TryParse(entryID, out int itemIndex))
                            {
                                ItemIndex index = (ItemIndex)itemIndex;
                                if (!BannedItems.Contains(index)) BannedItems.Add(index);
                            }
                        }
                        else if(identifier == char.ToUpperInvariant('E'))
                        {
                            if (int.TryParse(entryID, out int equipmentIndex))
                            {
                                EquipmentIndex index = (EquipmentIndex)equipmentIndex;
                                if (!BannedEquipments.Contains(index)) BannedEquipments.Add(index);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create and loads the custom interactables spawner config
        /// </summary>
        public static void LoadCustomInteractablesSpawnerConfig(ConfigFile config)
        {
            //Drones
            MegaDroneAmount = config.Bind<int>("Drones", "Mega Drone Amount", MegaDroneAmount, "Amount to attempt to spawn in each stage").Value;
            MegaDronePrice = config.Bind<int>("Drones", "Mega Drone Price", MegaDronePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            GunnerDroneAmount = config.Bind<int>("Drones", "Gunner Drone Amount", GunnerDroneAmount, "Amount to attempt to spawn in each stage").Value;
            GunnerDronePrice = config.Bind<int>("Drones", "Gunner Drone Price", GunnerDronePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            MissileDroneAmount = config.Bind<int>("Drones", "Missile Drone Amount", MissileDroneAmount, "Amount to attempt to spawn in each stage").Value;
            MissileDronePrice = config.Bind<int>("Drones", "Missile Drone Price", MissileDronePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            HealerDroneAmount = config.Bind<int>("Drones", "Healer Drone Amount", HealerDroneAmount, "Amount to attempt to spawn in each stage").Value;
            HealerDronePrice = config.Bind<int>("Drones", "Healer Drone Price", HealerDronePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            EquipmentDroneAmount = config.Bind<int>("Drones", "Equipment Drone Amount", EquipmentDroneAmount, "Amount to attempt to spawn in each stage").Value;
            FlameDroneAmount = config.Bind<int>("Drones", "Flame Drone Amount", FlameDroneAmount, "Amount to attempt to spawn in each stage").Value;
            FlameDronePrice = config.Bind<int>("Drones", "Flame Drone Price", FlameDronePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            TurretAmount = config.Bind<int>("Drones", "Turret Amount", TurretAmount, "Amount to attempt to spawn in each stage").Value;
            TurretPrice = config.Bind<int>("Drones", "Turret Price", TurretPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            //Shrines
            ShrineOfOrderAmount = config.Bind<int>("Shrines", "Shrine Of Order Amount", ShrineOfOrderAmount, "Amount to attempt to spawn in each stage").Value;
            ShrineOfBloodAmount = config.Bind<int>("Shrines", "Shrine Of Blood Amount", ShrineOfBloodAmount, "Amount to attempt to spawn in each stage").Value;
            ShrineOfChanceAmount = config.Bind<int>("Shrines", "Shrine Of Chance Amount", ShrineOfChanceAmount, "Amount to attempt to spawn in each stage").Value;
            ShrineOfChancePrice = config.Bind<int>("Shrines", "Shrine Of Chance Price", ShrineOfChancePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            ShrineOfCombatAmount = config.Bind<int>("Shrines", "Shrine Of Combat Amount", ShrineOfCombatAmount, "Amount to attempt to spawn in each stage").Value;
            ShrineOfHealingAmount = config.Bind<int>("Shrines", "Shrine Of Healing Amount", ShrineOfHealingAmount, "Amount to attempt to spawn in each stage").Value;
            ShrineOfHealingPrice = config.Bind<int>("Shrines", "Shrine Of Healing Price", ShrineOfHealingPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            GoldShrineAmount = config.Bind<int>("Shrines", "Gold Shrine Amount", GoldShrineAmount, "Amount to attempt to spawn in each stage").Value;
            GoldShrinePrice = config.Bind<int>("Shrines", "Gold Shrine Price", GoldShrinePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            //Misc
            CapsuleAmount = config.Bind<int>("Misc", "Capsule Amount", CapsuleAmount, "Amount to attempt to spawn in each stage").Value;
            RadarTowerAmount = config.Bind<int>("Misc", "Radar Tower Amount", RadarTowerAmount, "Amount to attempt to spawn in each stage").Value;
            RadarTowerPrice = config.Bind<int>("Misc", "Radar Tower Price", RadarTowerPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            CelestialPortalAmount = config.Bind<int>("Misc", "Celestial Portal Amount", CelestialPortalAmount, "Amount to attempt to spawn in each stage").Value;
            ShopPortalAmount = config.Bind<int>("Misc", "Shop Portal Amount", ShopPortalAmount, "Amount to attempt to spawn in each stage").Value;
            //Duplicators
            DuplicatorAmount = config.Bind<int>("Duplicators", "Duplicator Amount", DuplicatorAmount, "Amount to attempt to spawn in each stage").Value;
            DuplicatorLargeAmount = config.Bind<int>("Duplicators", "Duplicator Large Amount", DuplicatorLargeAmount, "Amount to attempt to spawn in each stage").Value;
            DuplicatorMilitaryAmount = config.Bind<int>("Duplicators", "Duplicator Military Amount", DuplicatorMilitaryAmount, "Amount to attempt to spawn in each stage").Value;
            //Chests
            GoldChestAmount = config.Bind<int>("Chests", "Gold Chest Amount", GoldChestAmount, "Amount to attempt to spawn in each stage").Value;
            GoldChestPrice = config.Bind<int>("Chests", "Gold Chest Price", GoldChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            SmallChestAmount = config.Bind<int>("Chests", "Small Chest Amount", SmallChestAmount, "Amount to attempt to spawn in each stage").Value;
            SmallChestPrice = config.Bind<int>("Chests", "Small Chest Price", SmallChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            LargeChestAmount = config.Bind<int>("Chests", "Large Chest Amount", LargeChestAmount, "Amount to attempt to spawn in each stage").Value;
            LargeChestPrice = config.Bind<int>("Chests", "Large Chest Price", LargeChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            DamageChestAmount = config.Bind<int>("Chests", "Damage Chest Amount", DamageChestAmount, "Amount to attempt to spawn in each stage").Value;
            DamageChestPrice = config.Bind<int>("Chests", "Damage Chest Price", DamageChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            HealingChestAmount = config.Bind<int>("Chests", "Healing Chest Amount", HealingChestAmount, "Amount to attempt to spawn in each stage").Value;
            HealingChestPrice = config.Bind<int>("Chests", "Healing Chest Price", HealingChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            UtilityChestAmount = config.Bind<int>("Chests", "Utility Chest Amount", UtilityChestAmount, "Amount to attempt to spawn in each stage").Value;
            UtilityChestPrice = config.Bind<int>("Chests", "Utility Chest Price", UtilityChestPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            TripleShopAmount = config.Bind<int>("Chests", "Triple Shop Amount", TripleShopAmount, "Amount to attempt to spawn in each stage").Value;
            TripleShopPrice = config.Bind<int>("Chests", "Triple Shop Price", TripleShopPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            TripleShopLargeAmount = config.Bind<int>("Chests", "Triple Shop Large Amount", TripleShopLargeAmount, "Amount to attempt to spawn in each stage").Value;
            TripleShopLargePrice = config.Bind<int>("Chests", "Triple Shop Large Price", TripleShopLargePrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            EquipmentBarrelAmount = config.Bind<int>("Chests", "Equipment Barrel Amount", EquipmentBarrelAmount, "Amount to attempt to spawn in each stage").Value;
            EquipmentBarrelPrice = config.Bind<int>("Chests", "Equipment Barrel Price", EquipmentBarrelPrice, "custom cost to buy, leave at -1 for vanilla cost").Value;
            LockboxAmount = config.Bind<int>("Chests", "Lockbox Amount", LockboxAmount, "Amount to attempt to spawn in each stage").Value;
            LunarChestAmount = config.Bind<int>("Chests", "Lunar Chest Amount", LunarChestAmount, "Amount to attempt to spawn in each stage").Value;
        }
        #endregion
    }
}
