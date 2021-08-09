using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates;
using RoR2;
using R2API;
using R2API.Utils;
using RoR2GameModeAPI;
using RoR2GameModeAPI.Utils;
using RoR2PVP.UI;

namespace RoR2PVP.GameModes
{
    /// <summary>
    /// Represents a generic pvp game mode expanded from the base game mode, handles all the shared behaviors of the other pvp game modes
    /// <para>This class is ment to be used as the base for other pvp game modes, should be inherited from and not used directly</para>
    /// </summary>
    public class PVPGameMode : GameMode
    {
        /*Extended Game Mode Settings*/
        public bool CompanionsShareItems = Settings.CompanionsShareItems.Item1;
        public bool CustomPlayableCharacters = Settings.CustomPlayableCharacters.Item1;
        public bool UseDeathPlaneFailsafe = Settings.UseDeathPlaneFailsafe.Item1;
        public bool WiderStageTransitions = Settings.WiderStageTransitions.Item1;
        public HashSet<string> WiderTransitionStages = new HashSet<string>() { { "artifactworld" }, { "blackbeach" }, { "dampcavesimple" }, { "foggyswamp" }, { "frozenwall" }, { "goldshores" }, { "golemplains" }, { "goolake" }, { "shipgraveyard" }, { "wispgraveyard" }, { "rootjungle" } };
        public HashSet<string> SafeZoneStages = new HashSet<string>() { { "bazaar" }, { "mysteryspace" }, { "arena" }, { "moon" }, { "moon2" } };
        public HashSet<string> AddTeleporterStages = new HashSet<string>() { { "skymeadow" }, { "artifactworld" }, { "goldshores" } };
        public HashSet<string> StagePopulateBlackList = new HashSet<string>() { { "bazaar" }, { "mysteryspace" }, { "arena" } };

        /*Cache*/
        protected bool IsGracePeriod = true;
        protected float GraceTimer = 0;
        protected float CashGrantTimer = 0;
        protected float CurrentGraceTimeReminder;
        protected bool PVPEnded = false;
        protected bool UsedTeleporter = false;
        protected TeleporterInteraction SecondaryTeleporter;

        //Randomized stage transition tracker
        protected List<SceneDef> Destinations = new List<SceneDef>();
        protected SceneDef FinalDestination = null;

        #region GameModeAPI Managed

        /// <summary>
        /// Constructor, assigns the game mode's name, and default game mode settings
        /// </summary>
        /// <param name="gameModeName">Game mode's name, ensure each name is unique</param>
        public PVPGameMode(string gameModeName) : base(gameModeName)
        {
            AllowVanillaTeleport = false;
        }

        protected override void Start(Stage self)
        {
            AllowVanillaGameOver = true;
            IsGracePeriod = true;
            GraceTimer = Settings.GraceTimerDuration;
            CashGrantTimer = 0;
            CurrentGraceTimeReminder = GraceTimer;
            PVPEnded = false;
            UsedTeleporter = false;
            SecondaryTeleporter = null;

            if (NetworkServer.active)
            {
                //Spawns a normal teleporter on the stage to prevent being forced to use a specific teleporter or being softlocked ending the run
                if (AddTeleporterStages.Contains(SceneInfo.instance.sceneDef.baseSceneName))
                {
                    SecondaryTeleporter = GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTeleporter", -1, Run.instance.stageRng).GetComponent<TeleporterInteraction>();
                }
            }
        }

        protected override void Update(Stage self)
        {

        }

        protected override void OnTeleporterInteraction(TeleporterInteraction self, Interactor activator)
        {
            if (!UsedTeleporter)
            {
                if (PVPEnded)
                {
                    AddLevel(5u);
                    self.GetComponent<SceneExitController>().Begin();
                    UsedTeleporter = true;
                    return;
                }

                Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                {
                    baseToken = "You cannot use the teleporter until all enemy players are dead."
                });
            }
        }

        protected override void OnGameOver(Run self, GameEndingDef gameEndingDef)
        {

        }

        protected override void OnPlayerRespawn(Stage self, CharacterMaster characterMaster)
        {
            if (!characterMaster) return;

            if (CustomPlayableCharacters)
            {
                self.usePod = false;
                switch (characterMaster.bodyPrefab.name)
                {
                    case "CommandoBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[0]);
                        break;
                    case "HuntressBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[1]);
                        break;
                    case "Bandit2Body":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[2]);
                        break;
                    case "ToolbotBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[3]);
                        break;
                    case "EngiBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[4]);
                        break;
                    case "MageBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[5]);
                        break;
                    case "MercBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[6]);
                        break;
                    case "TreebotBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[7]);
                        break;
                    case "LoaderBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[8]);
                        break;
                    case "CrocoBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[9]);
                        break;
                    case "CaptainBody":
                        characterMaster.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[10]);
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void OnStagePopulate(SceneDirector self)
        {
            if (AllowVanillaInteractableSpawns || !Run.instance || StagePopulateBlackList.Contains(SceneInfo.instance.sceneDef.baseSceneName)) return;

            //Custom spawn
            //Drones
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMegaDrone", Settings.MegaDroneAmount, Settings.MegaDronePrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone1", Settings.GunnerDroneAmount, Settings.GunnerDronePrice, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone2", Settings.HealerDroneAmount, Settings.HealerDronePrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMissileDrone", Settings.MissileDroneAmount, Settings.MissileDronePrice, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenEquipmentDrone", Settings.EquipmentDroneAmount, -1, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenFlameDrone", Settings.FlameDroneAmount, Settings.FlameDronePrice, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenTurret1", Settings.TurretAmount, Settings.TurretPrice, Run.instance.stageRng); //Non default

            //Shrines
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineRestack", Settings.ShrineOfOrderAmount, -1, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineBlood", Settings.ShrineOfBloodAmount, -1, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineChance", Settings.ShrineOfChanceAmount, Settings.ShrineOfChancePrice, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineCombat", Settings.ShrineOfCombatAmount, -1, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineHealing", Settings.ShrineOfHealingAmount, Settings.ShrineOfHealingPrice, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess", Settings.GoldShrineAmount, Settings.GoldShrinePrice, Run.instance.stageRng); //Non default

            //Misc
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBarrel1", Settings.CapsuleAmount, -1, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscRadarTower", Settings.RadarTowerAmount, Settings.RadarTowerPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscMSPortal", Settings.CelestialPortalAmount, -1, Run.instance.stageRng); //Non default
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShopPortal", Settings.ShopPortalAmount, -1, Run.instance.stageRng); //Non default

            //Duplicators
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicator", Settings.DuplicatorAmount, -1, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", Settings.DuplicatorLargeAmount, -1, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorMilitary", Settings.DuplicatorMilitaryAmount, -1, Run.instance.stageRng); //Non default

            //Chests
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscGoldChest", Settings.GoldChestAmount, Settings.GoldChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest1", Settings.SmallChestAmount, Settings.SmallChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest2", Settings.LargeChestAmount, Settings.LargeChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestDamage", Settings.DamageChestAmount, Settings.DamageChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestHealing", Settings.HealingChestAmount, Settings.HealingChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestUtility", Settings.UtilityChestAmount, Settings.UtilityChestPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShop", Settings.TripleShopAmount, Settings.TripleShopPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShopLarge", Settings.TripleShopLargeAmount, Settings.TripleShopLargePrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscEquipmentBarrel", Settings.EquipmentBarrelAmount, Settings.EquipmentBarrelPrice, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscLockbox", Settings.LockboxAmount, -1, Run.instance.stageRng);
            GameModeUtils.CustomGenerate("SpawnCards/InteractableSpawnCard/iscLunarChest", Settings.LunarChestAmount, -1, Run.instance.stageRng);
        }

        public override void SetExtraHooks()
        {
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += CompanionShareInventory;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += PreventRevivesShuffle;
            On.EntityStates.GhostUtilitySkillState.OnEnter += NerfStridesOfHeresy;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnEnter += BuffSniper;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnExit += RemoveSniperBuff;
            On.RoR2.MapZone.TryZoneStart += FixDeathPlanes;
            On.RoR2.GlobalEventManager.OnInteractionBegin += PreventTeleporterFireworks;
        }

        public override void UnsetExtraHooks()
        {
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster -= CompanionShareInventory;
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= PreventRevivesShuffle;
            On.EntityStates.GhostUtilitySkillState.OnEnter -= NerfStridesOfHeresy;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnEnter -= BuffSniper;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnExit -= RemoveSniperBuff;
            On.RoR2.MapZone.TryZoneStart -= FixDeathPlanes;
            On.RoR2.GlobalEventManager.OnInteractionBegin -= PreventTeleporterFireworks;
        }
        #endregion

        #region Setup
        /// <summary>
        /// Attempts to load all the stages that will be used when WiderStageTransitions is enabled and also shuffles them
        /// </summary>
        public void LoadDestinations()
        {
            //Reset
            Destinations.Clear();
            FinalDestination = null;

            //Attempt to add stages
            foreach (string stageName in WiderTransitionStages) GameModeUtils.TryAddStage(stageName, Destinations);
            if (GameModeUtils.TryGetStage("skymeadow", out SceneDef finalStage)) FinalDestination = finalStage;

            //Randomize
            RoR2PVPUtils.Shuffle(Destinations);
        }
        #endregion

        #region Extra Hooks
        /// <summary>
        /// Hook for minions to use their owner's inventory
        /// </summary>
        CharacterMaster CompanionShareInventory(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, SummonMasterBehavior self, Interactor activator)
        {
            if (NetworkServer.active)
            {
                //Copies owner's inventory items to companion
                CharacterMaster spawnedCompanion = orig(self, activator);
                if (CompanionsShareItems)
                {
                    spawnedCompanion.inventory.CopyItemsFrom(activator.GetComponent<CharacterBody>().master.inventory);
                    spawnedCompanion.inventory.ResetItem(RoR2Content.Items.WardOnLevel);
                    spawnedCompanion.inventory.ResetItem(RoR2Content.Items.BeetleGland);
                    spawnedCompanion.inventory.ResetItem(RoR2Content.Items.CrippleWardOnLevel);
                }
                return spawnedCompanion;
            }
            else return orig(self, activator);
        }

        /// <summary>
        /// Hook for preventing the bear item from being used when rolling the shrine of order
        /// </summary>
        void PreventRevivesShuffle(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                //Remove revives before shuffling so it doesn't add to the shuffle
                RoR2.Inventory playerInventory = interactor.GetComponent<CharacterBody>().master.inventory;
                playerInventory.RemoveItem(RoR2Content.Items.ExtraLife, 9999);
                playerInventory.RemoveItem(RoR2Content.Items.ExtraLifeConsumed, 9999);
                //Shuffle
                orig(self, interactor);
                //Reshuffle if shuffle landed on a revive item
                while (playerInventory.GetItemCount(RoR2Content.Items.ExtraLife) != 0)
                {
                    playerInventory.ShrineRestackInventory(Run.instance.treasureRng);
                }
            }
            else orig(self, interactor);
        }

        /// <summary>
        /// Hook for preventing the item "Strides Of Heresy" from healing the player
        /// </summary>
        void NerfStridesOfHeresy(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            if (NetworkServer.active) GhostUtilitySkillState.healFractionPerTick = 0;
            orig(self);
        }

        /// <summary>
        /// Hook for applying buffs to the sniper character
        /// </summary>
        void BuffSniper(On.EntityStates.Sniper.Scope.ScopeSniper.orig_OnEnter orig, EntityStates.Sniper.Scope.ScopeSniper self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                self.outer.commonComponents.characterBody.RemoveBuff(RoR2Content.Buffs.Slow50);
                self.outer.commonComponents.characterBody.AddBuff(RoR2Content.Buffs.AttackSpeedOnCrit);
            }
        }

        /// <summary>
        /// Hook for removing buffs from the sniper character
        /// </summary>
        void RemoveSniperBuff(On.EntityStates.Sniper.Scope.ScopeSniper.orig_OnExit orig, EntityStates.Sniper.Scope.ScopeSniper self)
        {
            if (NetworkServer.active)
            {
                self.outer.commonComponents.characterBody.AddBuff(RoR2Content.Buffs.Slow50);
                self.outer.commonComponents.characterBody.RemoveBuff(RoR2Content.Buffs.AttackSpeedOnCrit);
            }
            orig(self);
        }

        /// <summary>
        /// Hook for fixing the teleport planes for when players fall off the map, prevents occasional instant deaths
        /// </summary>
        void FixDeathPlanes(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
        {
            if (NetworkServer.active)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
                    {
                        if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                        {
                            if (body == PlayerCharacterMasterController.instances[i].master.GetBody())
                            {
                                TeamIndex PreviousTeam = body.teamComponent.teamIndex;
                                body.teamComponent.teamIndex = TeamIndex.Player;
                                orig(self, other);
                                body.teamComponent.teamIndex = PreviousTeam;
                                return;
                            }
                        }
                    }
                }
            }
            orig(self, other);
        }

        /// <summary>
        /// Hook for preventing players from spamming fireworks when interacting with the teleporter
        /// </summary>
        void PreventTeleporterFireworks(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            TeleporterInteraction teleporter = interactableObject.GetComponent<TeleporterInteraction>();
            if (!teleporter) orig(self, interactor, interactable, interactableObject);
        }
        #endregion

        #region Functions
        /// <summary>
        /// Counts down the grace timer
        /// <para>Call this in the Update callback</para>
        /// </summary>
        protected void Countdown()
        {
            GraceTimer -= Time.fixedDeltaTime;

            //Send a 5 second warning to players before grace period runs out
            if (GraceTimer <= 5f)
            {
                if (GraceTimer <= CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(CurrentGraceTimeReminder.ToString(), new Color32(255, 106, 0, 255)) + " Seconds left of grace period!"
                    });
                    CurrentGraceTimeReminder -= 1f;
                }
            }
            else
            {
                //Reminds players of remaining time every 30 seconds
                if (GraceTimer <= CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(CurrentGraceTimeReminder.ToString(), new Color32(255, 216, 0, 255)) + " Seconds left of grace period!"
                    });
                    float nextTimeReminder = CurrentGraceTimeReminder - 30f;
                    if (nextTimeReminder <= 5f) CurrentGraceTimeReminder = 5f;
                    else CurrentGraceTimeReminder = nextTimeReminder;
                }
            }
        }

        /// <summary>
        /// Counts down the cash grant timer
        /// <para>Call this in the Update callback</para>
        /// </summary>
        /// <param name="cashAmount">The amount of money every player will recieve when timer hits 0</param>
        protected void CashGrantTimerTick(uint cashAmount)
        {
            CashGrantTimer += Time.fixedDeltaTime;

            //Grant cash to each player
            if (CashGrantTimer >= Settings.CashDelay)
            {
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++) PlayerCharacterMasterController.instances[i].master.GiveMoney(cashAmount);
                CashGrantTimer -= Settings.CashDelay;
            }
        }

        /// <summary>
        /// Sets the teleporter's next stage destination if WiderStageTransitions is enabled
        /// </summary>
        protected void CustomStageTransition()
        {
            if (WiderStageTransitions)
            {
                if (Destinations.Contains(SceneInfo.instance.sceneDef))
                {
                    Destinations.Remove(SceneInfo.instance.sceneDef);
                    if (Destinations.Count == 0)
                    {
                        LoadDestinations();
                        if (FinalDestination)
                        {
                            Run.instance.nextStageScene = FinalDestination;
                            return;
                        }
                    }
                    Run.instance.nextStageScene = Destinations[0];
                }
            }
        }

        /// <summary>
        /// Checks every player to see if they fell off the map and force kill them if they did not teleport back up, prevents softlocking the game mode
        /// </summary>
        protected void BruteforceDeathPlane()
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                {
                    if (PlayerCharacterMasterController.instances[i].master.GetBody().transform.position.y <= -2200) PlayerCharacterMasterController.instances[i].master.GetBody().healthComponent.Suicide();
                }
            }
        }
        #endregion

        #region Utils
        /// <summary>
        /// Add level ups for each team
        /// </summary>
        /// <param name="level">The amount of levels to add</param>
        protected void AddLevel(uint level)
        {
            TeamManager.instance.SetTeamLevel(TeamIndex.Player, TeamManager.instance.GetTeamLevel(TeamIndex.Player) + level);
            TeamManager.instance.SetTeamLevel(TeamIndex.Neutral, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
            TeamManager.instance.SetTeamLevel(TeamIndex.Monster, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
        }

        /// <summary>
        /// Filter out disconnected players from a list of players
        /// </summary>
        /// <param name="listToFilter">List to filter</param>
        protected void FilterDCedPlayers(List<PlayerCharacterMasterController> listToFilter)
        {
            for (int i = listToFilter.Count - 1; i >= 0; i--)
            {
                if (listToFilter[i].master.IsDeadAndOutOfLivesServer() || listToFilter[i].master.GetBody() == null) listToFilter.RemoveAt(i);
            }
        }

        /// <summary>
        /// Destroys all purchasable interactables from the stage (chests, drones, shrines, etc)
        /// </summary>
        protected void DestroyAllPurchaseables()
        {
            PurchaseInteraction[] purchaseables = GameObject.FindObjectsOfType<PurchaseInteraction>();
            for (int i = 0; i < purchaseables.Length; i++)
            {
                if (purchaseables[i].costType == CostTypeIndex.Money ||
                    purchaseables[i].costType == CostTypeIndex.PercentHealth ||
                    purchaseables[i].costType == CostTypeIndex.WhiteItem ||
                    purchaseables[i].costType == CostTypeIndex.GreenItem ||
                    purchaseables[i].costType == CostTypeIndex.RedItem ||
                    purchaseables[i].costType == CostTypeIndex.LunarCoin) NetworkServer.Destroy(purchaseables[i].gameObject);
            }
        }
        #endregion
    }
}
