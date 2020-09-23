using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using EntityStates;
using APIExtension.VoteAPI;

namespace RoR2PVP
{
    class Hooks
    {
        public static void Init()
        {
            /*Assets*/
            RegisterAssets();

            /*Options menu for lobby*/
            //Header
            RuleCategoryDef teamPVPHeader = VoteAPI.AddVoteHeader("Team PVP", new Color(1.0f, 0.0f, 0.0f, 1.0f), false);

            //Selection
            RuleDef teamPVPSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Team PVP", new ChoiceMenu("Team PVP On", new Color(0.0f, 1.0f, 0.0f, 0.4f), "Enables Mod", Color.black, "@TeamPVP:Assets/Resources/UI/TeamPVPSelected.png", "artifact_teampvp", Settings.TeamPVPToggle.Item2));
            VoteAPI.AddVoteChoice(teamPVPSelection, new ChoiceMenu("Team PVP Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Disables Mod", Color.black, "@TeamPVP:Assets/Resources/UI/TeamPVPDeselected.png", "artifact_teampvp", -1));
            teamPVPSelection.defaultChoiceIndex = Settings.TeamPVPToggle.Item1 ? 0 : 1;

            RuleDef randomTeamsSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Random Teams", new ChoiceMenu("Random Teams On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Teams will be shuffled every round", Color.black, "@TeamPVP:Assets/Resources/UI/RandomTeamsSelected.png", "artifact_teampvp", Settings.RandomTeams.Item2));
            VoteAPI.AddVoteChoice(randomTeamsSelection, new ChoiceMenu("Random Teams Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Teams will stay the same every round", Color.black, "@TeamPVP:Assets/Resources/UI/RandomTeamsDeselected.png", "artifact_teampvp", -1));
            randomTeamsSelection.defaultChoiceIndex = Settings.RandomTeams.Item1 ? 0 : 1;

            RuleDef mobSpawnSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Mob Spawn", new ChoiceMenu("Mob Spawn On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Mobs will spawn", Color.black, "@TeamPVP:Assets/Resources/UI/MobSpawnSelected.png", "artifact_teampvp", Settings.MobSpawn.Item2));
            VoteAPI.AddVoteChoice(mobSpawnSelection, new ChoiceMenu("Mob Spawn Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Mobs will not spawn", Color.black, "@TeamPVP:Assets/Resources/UI/MobSpawnDeselected.png", "artifact_teampvp", -1));
            mobSpawnSelection.defaultChoiceIndex = Settings.MobSpawn.Item1 ? 0 : 1;

            RuleDef banItemsSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Ban Items", new ChoiceMenu("Ban Items On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Banned items configured in the config will be blacklisted", Color.black, "@TeamPVP:Assets/Resources/UI/BanItemsSelected.png", "artifact_teampvp", Settings.BanItems.Item2));
            VoteAPI.AddVoteChoice(banItemsSelection, new ChoiceMenu("Ban Items Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Banned Items will not be blacklisted", Color.black, "@TeamPVP:Assets/Resources/UI/BanItemsDeselected.png", "artifact_teampvp", -1));
            banItemsSelection.defaultChoiceIndex = Settings.BanItems.Item1 ? 0 : 1;

            RuleDef companionsShareItemsSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Companions Share Items", new ChoiceMenu("Companions Share Items On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Items picked up by the player will be shared with their drones etc", Color.black, "@TeamPVP:Assets/Resources/UI/CompanionsShareItemsSelected.png", "artifact_teampvp", Settings.CompanionsShareItems.Item2));
            VoteAPI.AddVoteChoice(companionsShareItemsSelection, new ChoiceMenu("Companions Share Items Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Items will not be shared with drones etc", Color.black, "@TeamPVP:Assets/Resources/UI/CompanionsShareItemsDeselected.png", "artifact_teampvp", -1));
            companionsShareItemsSelection.defaultChoiceIndex = Settings.CompanionsShareItems.Item1 ? 0 : 1;

            RuleDef customPlayableCharactersSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Custom Playable Characters", new ChoiceMenu("Custom Playable Characters On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Play custom characters configured in the config", Color.black, "@TeamPVP:Assets/Resources/UI/CustomPlayableCharactersSelected.png", "artifact_teampvp", Settings.CustomPlayableCharacters.Item2));
            VoteAPI.AddVoteChoice(customPlayableCharactersSelection, new ChoiceMenu("Custom Playable Characters Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Play the default vanilla characters (unbalanced)", Color.black, "@TeamPVP:Assets/Resources/UI/CustomPlayableCharactersDeselected.png", "artifact_teampvp", -1));
            customPlayableCharactersSelection.defaultChoiceIndex = Settings.CustomPlayableCharacters.Item1 ? 0 : 1;

            RuleDef customInteractablesSpawnerSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Custom Interactables Spawner", new ChoiceMenu("Custom Interactables Spawner On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Spawn custom objects(chests, drones, etc) at custom rates configured in the config", Color.black, "@TeamPVP:Assets/Resources/UI/CustomInteractablesSpawnerSelected.png", "artifact_teampvp", Settings.CustomInteractablesSpawner.Item2));
            VoteAPI.AddVoteChoice(customInteractablesSpawnerSelection, new ChoiceMenu("Custom Interactables Spawner Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Spawn objects(chests, drones, etc) normally as vanilla", Color.black, "@TeamPVP:Assets/Resources/UI/CustomInteractablesSpawnerDeselected.png", "artifact_teampvp", -1));
            customInteractablesSpawnerSelection.defaultChoiceIndex = Settings.CustomInteractablesSpawner.Item1 ? 0 : 1;

            RuleDef useDeathPlaneFailsafeSelection = VoteAPI.AddVoteSelection(teamPVPHeader, "Use Death Plane Failsafe", new ChoiceMenu("Use Death Plane Failsafe On", new Color(0.0f, 0.58f, 1.0f, 0.4f), "Force players to die should they fall off the map to prevent softlock", Color.black, "@TeamPVP:Assets/Resources/UI/UseDeathPlaneFailsafeSelected.png", "artifact_teampvp", Settings.UseDeathPlaneFailsafe.Item2));
            VoteAPI.AddVoteChoice(useDeathPlaneFailsafeSelection, new ChoiceMenu("Use Death Plane Failsafe Off", new Color(1.0f, 0.0f, 0.0f, 0.4f), "Disables the death plane. Only turn off if your using a custom map!", Color.black, "@TeamPVP:Assets/Resources/UI/UseDeathPlaneFailsafeDeselected.png", "artifact_teampvp", -1));
            useDeathPlaneFailsafeSelection.defaultChoiceIndex = Settings.UseDeathPlaneFailsafe.Item1 ? 0 : 1;
        }

        public static void SetupHook()
        {
            /*Preassign*/
            if (Settings.MaxMultiplayerCount != 4)
            {
                typeof(RoR2Application).GetField("maxPlayers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static).SetValue(null, Settings.MaxMultiplayerCount);
                GameNetworkManager.SvMaxPlayersConVar.instance.SetString(Settings.MaxMultiplayerCount.ToString());
                SteamworksLobbyManager.cvSteamLobbyMaxMembers.SetString(Settings.MaxMultiplayerCount.ToString());
            }
            if (!Settings.Modded) typeof(RoR2Application).GetField("isModded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static).SetValue(null, false);

            //Start mod
            On.RoR2.Networking.GameNetworkManager.OnServerAddPlayerInternal += DisplayCustomCharacters;
            On.RoR2.PreGameController.ResolveChoiceMask += DisplayArtifacts;
            On.RoR2.Stats.StatSheet.HasUnlockable += UnlockAll;
            On.RoR2.Run.Start += GameStart;
            RoR2.Run.onRunDestroyGlobal += GameEnd;
        }

        public static void SetCoreHooks()
        {
            On.RoR2.Stage.Start += PVPReset;
            On.RoR2.Stage.FixedUpdate += PVPTick;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += InstantTeleport;
        }

        public static void UnsetCoreHooks()
        {
            On.RoR2.Stage.Start -= PVPReset;
            On.RoR2.Stage.FixedUpdate -= PVPTick;
            On.RoR2.TeleporterInteraction.OnInteractionBegin -= InstantTeleport;
        }

        public static void SetExtraHooks()
        {
            On.RoR2.Stage.RespawnCharacter += ControlRespawn;
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster += CompanionShareInventory;
            On.RoR2.ShrineRestackBehavior.AddShrineStack += PreventRevivesShuffle;
            On.EntityStates.GhostUtilitySkillState.OnEnter += NerfStridesOfHeresy;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnEnter += BuffSniper;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnExit += RemoveSniperBuff;
            On.RoR2.MapZone.TryZoneStart += FixDeathPlanes;
            On.RoR2.CombatDirector.Simulate += DisableMobSpawn;
            On.RoR2.SceneDirector.Start += DisableDefaultSpawn;
            On.RoR2.SceneDirector.PopulateScene += CustomSpawner;
            On.RoR2.Run.BuildDropTable += BanItems;
            On.RoR2.GlobalEventManager.OnInteractionBegin += PreventTeleporterFireworks;
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += CustomAITargetFilter;
        }

        public static void UnsetExtraHooks()
        {
            On.RoR2.Stage.RespawnCharacter -= ControlRespawn;
            On.RoR2.SummonMasterBehavior.OpenSummonReturnMaster -= CompanionShareInventory;
            On.RoR2.ShrineRestackBehavior.AddShrineStack -= PreventRevivesShuffle;
            On.EntityStates.GhostUtilitySkillState.OnEnter -= NerfStridesOfHeresy;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnEnter -= BuffSniper;
            On.EntityStates.Sniper.Scope.ScopeSniper.OnExit -= RemoveSniperBuff;
            On.RoR2.MapZone.TryZoneStart -= FixDeathPlanes;
            On.RoR2.CombatDirector.Simulate -= DisableMobSpawn;
            On.RoR2.SceneDirector.Start -= DisableDefaultSpawn;
            On.RoR2.SceneDirector.PopulateScene -= CustomSpawner;
            On.RoR2.Run.BuildDropTable -= BanItems;
            On.RoR2.GlobalEventManager.OnInteractionBegin -= PreventTeleporterFireworks;
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox -= CustomAITargetFilter;
        }

        #region Init
        static void RegisterAssets()
        {
            using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoR2PVP.Assets.teampvp.ui"))
            {
                Settings.Assets = AssetBundle.LoadFromStream(manifestResourceStream);
                Settings.Provider = new AssetBundleResourcesProvider("@TeamPVP", Settings.Assets);
            }
            ResourcesAPI.AddProvider(Settings.Provider);
        }
        #endregion

        #region Startup
        static void DisplayCustomCharacters(On.RoR2.Networking.GameNetworkManager.orig_OnServerAddPlayerInternal orig, GameNetworkManager self, NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            if (NetworkServer.active)
            {
                //Ghetto code to prevent duplicates
                if (self.playerPrefab == null) return;
                if (self.playerPrefab.GetComponent<NetworkIdentity>() == null) return;
                if ((int)playerControllerId < conn.playerControllers.Count && conn.playerControllers[(int)playerControllerId].IsValid && conn.playerControllers[(int)playerControllerId].gameObject != null) return;
                if (NetworkUser.readOnlyInstancesList.Count >= self.maxConnections) return;

                string text = "";
                for (int i = 0; i < Settings.PlayableCharactersList.Count; i++)
                {
                    string slotName = "";
                    switch (i)
                    {
                        case 0:
                            slotName = "Commando";
                            break;
                        case 1:
                            slotName = "Huntress";
                            break;
                        case 2:
                            slotName = "MUL-T";
                            break;
                        case 3:
                            slotName = "Engineer";
                            break;
                        case 4:
                            slotName = "Artificer";
                            break;
                        case 5:
                            slotName = "Mercenary";
                            break;
                        case 6:
                            slotName = "REX";
                            break;
                        case 7:
                            slotName = "Loader";
                            break;
                        case 8:
                            slotName = "Acrid";
                            break;
                        case 9:
                            slotName = "Captain";
                            break;
                    }
                    text += Util.GenerateColoredString(slotName, new Color32(255, 255, 0, 255)) + " = " + BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(Settings.PlayableCharactersList[i])) + " ";

                    if (i + 1 % 3 == 0 || i + 1 == Settings.PlayableCharactersList.Count)
                    {
                        Tools.SendPM(conn, new Chat.SimpleChatMessage
                        {
                            baseToken = text
                        });
                        text = "";
                    }
                }
            }
            orig(self, conn, playerControllerId, extraMessageReader);
        }

        static void DisplayArtifacts(On.RoR2.PreGameController.orig_ResolveChoiceMask orig, PreGameController self)
        {
            RuleChoiceMask unlockedChoiceMask = R2API.Utils.Reflection.GetFieldValue<RuleChoiceMask>(self, "unlockedChoiceMask");
            for (int i = 0; i < RuleCatalog.choiceCount - 4; i++)
            {
                unlockedChoiceMask[i] = true;
                R2API.Utils.Reflection.SetFieldValue<RuleChoiceMask>(self, "unlockedChoiceMask", unlockedChoiceMask);
            }
            orig(self);
        }

        private static bool UnlockAll(On.RoR2.Stats.StatSheet.orig_HasUnlockable orig, RoR2.Stats.StatSheet self, UnlockableDef unlockableDef)
        {
            if (Settings.UnlockAll) return true;
            return orig(self, unlockableDef);
        }

        static void GameStart(On.RoR2.Run.orig_Start orig, Run self)
        {
            //Run mod if mod was enabled in the lobby menu
            if (VoteAPI.VoteResults.HasVote(Settings.TeamPVPToggle.Item2))
            {
                SetCoreHooks();
                SetExtraHooks();
            }
            orig(self);
        }

        static void GameEnd(Run self)
        {
            if (VoteAPI.VoteResults.HasVote(Settings.TeamPVPToggle.Item2))
            {
                UnsetCoreHooks();
                UnsetExtraHooks();
            }
        }
        #endregion

        #region Core
        static void PVPReset(On.RoR2.Stage.orig_Start orig, Stage self)
        {
            if (NetworkServer.active) PVPMode.Reset();
            orig(self);
        }

        static void PVPTick(On.RoR2.Stage.orig_FixedUpdate orig, Stage self)
        {
            if (NetworkServer.active) PVPMode.Tick();
            orig(self);
        }

        static void InstantTeleport(On.RoR2.TeleporterInteraction.orig_OnInteractionBegin orig, TeleporterInteraction self, Interactor activator)
        {
            if (NetworkServer.active) PVPMode.Teleport(self);
            else orig(self, activator);
        }
        #endregion

        #region Extras
        static void ControlRespawn(On.RoR2.Stage.orig_RespawnCharacter orig, Stage self, CharacterMaster characterMaster)
        {
            CharacterMaster playerCharater = characterMaster;
            if (NetworkServer.active)
            {
                if (!characterMaster) return;

                self.usePod = false;
                if (VoteAPI.VoteResults.HasVote(Settings.CustomPlayableCharacters.Item2))
                {
                    switch (playerCharater.bodyPrefab.name)
                    {
                        case "CommandoBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[0]);
                            break;
                        case "HuntressBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[1]);
                            break;
                        case "ToolbotBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[2]);
                            break;
                        case "EngiBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[3]);
                            break;
                        case "MageBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[4]);
                            break;
                        case "MercBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[5]);
                            break;
                        case "TreebotBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[6]);
                            break;
                        case "LoaderBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[7]);
                            break;
                        case "CrocoBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[8]);
                            break;
                        case "CaptainBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[9]);
                            break;
                        default:
                            break;
                    }
                }
            }
            orig(self, playerCharater);
        }

        static CharacterMaster CompanionShareInventory(On.RoR2.SummonMasterBehavior.orig_OpenSummonReturnMaster orig, SummonMasterBehavior self, Interactor activator)
        {
            if (NetworkServer.active)
            {
                //Copies owner's inventory items to companion
                CharacterMaster spawnedCompanion = orig(self, activator);
                if (VoteAPI.VoteResults.HasVote(Settings.CompanionsShareItems.Item2))
                {
                    spawnedCompanion.GetBody().inventory.CopyItemsFrom(activator.GetComponent<CharacterBody>().master.inventory);
                    spawnedCompanion.GetBody().inventory.ResetItem(ItemIndex.WardOnLevel);
                    spawnedCompanion.GetBody().inventory.ResetItem(ItemIndex.BeetleGland);
                    spawnedCompanion.GetBody().inventory.ResetItem(ItemIndex.CrippleWardOnLevel);
                }
                return spawnedCompanion;
            }
            else return orig(self, activator);
        }

        static void PreventRevivesShuffle(On.RoR2.ShrineRestackBehavior.orig_AddShrineStack orig, ShrineRestackBehavior self, Interactor interactor)
        {
            if (NetworkServer.active)
            {
                //Remove revives before shuffling so it doesn't add to the shuffle
                RoR2.Inventory playerInventory = interactor.GetComponent<CharacterBody>().master.inventory;
                playerInventory.RemoveItem(ItemIndex.ExtraLife, 9999);
                playerInventory.RemoveItem(ItemIndex.ExtraLifeConsumed, 9999);
                //Shuffle
                orig(self, interactor);
                //Reshuffle if shuffle landed on a revive item
                while (playerInventory.GetItemCount(ItemIndex.ExtraLife) != 0 || playerInventory.GetItemCount(ItemIndex.ExtraLife) != 0)
                {
                    playerInventory.ShrineRestackInventory(Run.instance.treasureRng);
                }
            }
            else orig(self, interactor);
        }

        static void NerfStridesOfHeresy(On.EntityStates.GhostUtilitySkillState.orig_OnEnter orig, GhostUtilitySkillState self)
        {
            if (NetworkServer.active) GhostUtilitySkillState.healFractionPerTick = 0;
            orig(self);
        }

        static void BuffSniper(On.EntityStates.Sniper.Scope.ScopeSniper.orig_OnEnter orig, EntityStates.Sniper.Scope.ScopeSniper self)
        {
            orig(self);
            if (NetworkServer.active)
            {
                self.outer.commonComponents.characterBody.RemoveBuff(BuffIndex.Slow50);
                self.outer.commonComponents.characterBody.AddBuff(BuffIndex.AttackSpeedOnCrit);
            }
        }

        static void RemoveSniperBuff(On.EntityStates.Sniper.Scope.ScopeSniper.orig_OnExit orig, EntityStates.Sniper.Scope.ScopeSniper self)
        {
            if (NetworkServer.active)
            {
                self.outer.commonComponents.characterBody.AddBuff(BuffIndex.Slow50);
                self.outer.commonComponents.characterBody.RemoveBuff(BuffIndex.AttackSpeedOnCrit);
            }
            orig(self);
        }

        static void FixDeathPlanes(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider other)
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

        static void DisableMobSpawn(On.RoR2.CombatDirector.orig_Simulate orig, CombatDirector self, float deltaTime)
        {
            if (NetworkServer.active) orig(self, VoteAPI.VoteResults.HasVote(Settings.MobSpawn.Item2) ? deltaTime : 0);
            else orig(self, deltaTime);
        }

        static void DisableDefaultSpawn(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            if (NetworkServer.active)
            {
                ClassicStageInfo sceneInfo = SceneInfo.instance.GetComponent<ClassicStageInfo>();

                //Disables default interactibles spawning
                if (VoteAPI.VoteResults.HasVote(Settings.CustomInteractablesSpawner.Item2))
                {
                    sceneInfo.sceneDirectorInteractibleCredits = 0;
                    sceneInfo.bonusInteractibleCreditObjects = null;
                }

                //Disables default mob spawning
                if (!VoteAPI.VoteResults.HasVote(Settings.MobSpawn.Item2)) sceneInfo.sceneDirectorMonsterCredits = 0;
            }
            orig(self);
        }

        static void CustomSpawner(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if(NetworkServer.active)
            {
                if(VoteAPI.VoteResults.HasVote(Settings.CustomInteractablesSpawner.Item2))
                {
                    if (Run.instance && SceneInfo.instance.sceneDef.baseSceneName != "bazaar" || SceneInfo.instance.sceneDef.baseSceneName != "mysteryspace")
                    {
                        //Custom spawn
                        //Drones
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMegaDrone", Settings.MegaDroneAmount, Settings.MegaDronePrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone1", Settings.GunnerDroneAmount, Settings.GunnerDronePrice, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone2", Settings.HealerDroneAmount, Settings.HealerDronePrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMissileDrone", Settings.MissileDroneAmount, Settings.MissileDronePrice, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenEquipmentDrone", Settings.EquipmentDroneAmount, -1, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenFlameDrone", Settings.FlameDroneAmount, Settings.FlameDronePrice, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenTurret1", Settings.TurretAmount, Settings.TurretPrice, Run.instance.stageRng); //Non default

                        //Shrines
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineRestack", Settings.ShrineOfOrderAmount, -1, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineBlood", Settings.ShrineOfBloodAmount, -1, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineChance", Settings.ShrineOfChanceAmount, Settings.ShrineOfChancePrice, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineCombat", Settings.ShrineOfCombatAmount, -1, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineHealing", Settings.ShrineOfHealingAmount, Settings.ShrineOfHealingPrice, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess", Settings.GoldShrineAmount, Settings.GoldShrinePrice, Run.instance.stageRng); //Non default

                        //Misc
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscBarrel1", Settings.CapsuleAmount, -1, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscRadarTower", Settings.RadarTowerAmount, Settings.RadarTowerPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscMSPortal", Settings.CelestialPortalAmount, -1, Run.instance.stageRng); //Non default
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscShopPortal", Settings.ShopPortalAmount, -1, Run.instance.stageRng); //Non default

                        //Duplicators
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicator", Settings.DuplicatorAmount, -1, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", Settings.DuplicatorLargeAmount, -1, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorMilitary", Settings.DuplicatorMilitaryAmount, -1, Run.instance.stageRng); //Non default

                        //Chests
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscGoldChest", Settings.GoldChestAmount, Settings.GoldChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest1", Settings.SmallChestAmount, Settings.SmallChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest2", Settings.LargeChestAmount, Settings.LargeChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestDamage", Settings.DamageChestAmount, Settings.DamageChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestHealing", Settings.HealingChestAmount, Settings.HealingChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestUtility", Settings.UtilityChestAmount, Settings.UtilityChestPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShop", Settings.TripleShopAmount, Settings.TripleShopPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShopLarge", Settings.TripleShopLargeAmount, Settings.TripleShopLargePrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscEquipmentBarrel", Settings.EquipmentBarrelAmount, Settings.EquipmentBarrelPrice, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscLockbox", Settings.LockboxAmount, -1, Run.instance.stageRng);
                        Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscLunarChest", Settings.LunarChestAmount, -1, Run.instance.stageRng);
                    }
                }
            }
            orig(self);
        }

        static void BanItems(On.RoR2.Run.orig_BuildDropTable orig, Run self)
        {
            if (NetworkServer.active)
            {
                if(VoteAPI.VoteResults.HasVote(Settings.BanItems.Item2))
                {
                    ItemIndex itemToRemove;
                    EquipmentIndex equipmentToRemove;

                    //Remove all items and equipments that is mentioned in the ban list
                    for (int i = 0; i < Settings.BannedItemList.Count; i++)
                    {
                        if (Enum.TryParse(Settings.BannedItemList[i], out itemToRemove)) Run.instance.availableItems.Remove(itemToRemove);
                        if (Enum.TryParse(Settings.BannedItemList[i], out equipmentToRemove)) Run.instance.availableEquipment.Remove(equipmentToRemove);
                    }
                }
            }
            orig(self);
        }

        static void PreventTeleporterFireworks(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            TeleporterInteraction teleporter = interactableObject.GetComponent<TeleporterInteraction>();
            if (!teleporter) orig(self, interactor, interactable, interactableObject);
        }

        static HurtBox CustomAITargetFilter(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, RoR2.CharacterAI.BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (!self.body) return null;
            BullseyeSearch enemySearch = self.GetFieldValue<BullseyeSearch>("enemySearch");
            enemySearch.viewer = self.body;
            enemySearch.teamMaskFilter = TeamMask.all;
            enemySearch.teamMaskFilter.RemoveTeam(self.master.teamIndex);
            enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
            enemySearch.minDistanceFilter = 0f;
            enemySearch.maxDistanceFilter = maxDistance;
            enemySearch.searchOrigin = self.bodyInputBank.aimOrigin;
            enemySearch.searchDirection = self.bodyInputBank.aimDirection;
            enemySearch.maxAngleFilter = (full360Vision ? 180f : 90f);
            enemySearch.filterByLoS = filterByLoS;
            enemySearch.RefreshCandidates();
            return enemySearch.GetResults().FirstOrDefault<HurtBox>();
        }
        #endregion
    }
}
