using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.CharacterAI;
using RoR2.UI;
using EntityStates;
using Facepunch.Steamworks;

namespace RoR2PVP
{
    class Hooks
    {
        public static void Preassign()
        {
            if (Settings.MaxMultiplayerCount != 4) typeof(RoR2Application).GetField("maxPlayers", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Static).SetValue(null, Settings.MaxMultiplayerCount);
        }

        public static void CoreHooks()
        {
            On.RoR2.Stage.Start += PVPReset;
            On.RoR2.Stage.FixedUpdate += PVPTick;
            On.RoR2.TeleporterInteraction.OnInteractionBegin += InstantTeleport;
        }

        public static void ExtraHooks()
        {
            On.RoR2.Networking.GameNetworkManager.OnServerAddPlayerInternal += DisplayCustomCharacters;
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
        }

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

        static void DisplayCustomCharacters(On.RoR2.Networking.GameNetworkManager.orig_OnServerAddPlayerInternal orig, GameNetworkManager self, NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            if (NetworkServer.active)
            {
                //Ghetto code to prevent duplicates
                if (self.playerPrefab == null) return;
                if (self.playerPrefab.GetComponent<NetworkIdentity>() == null) return;
                if ((int)playerControllerId < conn.playerControllers.Count && conn.playerControllers[(int)playerControllerId].IsValid && conn.playerControllers[(int)playerControllerId].gameObject != null) return;
                if (NetworkUser.readOnlyInstancesList.Count >= self.maxConnections) return;

                if(Settings.CustomPlayableCharacters)
                {
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
                                slotName = "MUL-T";
                                break;
                            case 2:
                                slotName = "Huntress";
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
                        }
                        text += Util.GenerateColoredString(slotName, new Color32(255, 255, 0, 255)) + " = " + BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(Settings.PlayableCharactersList[i])) + " ";

                        if (i + 1 % 3 == 0 || i + 1 == Settings.PlayableCharactersList.Count)
                        {
                            SendPM(conn, new Chat.SimpleChatMessage
                            {
                                baseToken = text
                            });
                            text = "";
                        }
                    }
                }
            }
            orig(self, conn, playerControllerId, extraMessageReader);
        }

        static void ControlRespawn(On.RoR2.Stage.orig_RespawnCharacter orig, Stage self, CharacterMaster characterMaster)
        {
            CharacterMaster playerCharater = characterMaster;
            if (NetworkServer.active)
            {
                if (!characterMaster) return;

                self.usePod = false;
                if (Settings.CustomPlayableCharacters)
                {
                    switch (playerCharater.bodyPrefab.name)
                    {
                        case "CommandoBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[0]);
                            break;
                        case "ToolbotBody":
                            playerCharater.bodyPrefab = BodyCatalog.FindBodyPrefab(Settings.PlayableCharactersList[1]);
                            break;
                        case "HuntressBody":
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
                if (Settings.CompanionsShareItems)
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
                        if (PlayerCharacterMasterController.instances[i].master.alive && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
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
            if (NetworkServer.active) orig(self, Settings.DisableMobSpawn ? 0 : deltaTime);
            else orig(self, deltaTime);
        }

        static void DisableDefaultSpawn(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            if (NetworkServer.active)
            {
                ClassicStageInfo sceneInfo = SceneInfo.instance.GetComponent<ClassicStageInfo>();

                //Disables default interactibles spawning
                if (Settings.CustomInteractablesSpawner)
                {
                    sceneInfo.sceneDirectorInteractibleCredits = 0;
                    sceneInfo.bonusInteractibleCreditObjects = null;
                }

                //Disables default mob spawning
                if (Settings.DisableMobSpawn) sceneInfo.sceneDirectorMonsterCredits = 0;
            }
            orig(self);
        }

        static void CustomSpawner(On.RoR2.SceneDirector.orig_PopulateScene orig, SceneDirector self)
        {
            if(NetworkServer.active)
            {
                if(Settings.CustomInteractablesSpawner)
                {
                    if (Run.instance && SceneInfo.instance.sceneDef.baseSceneName != "bazaar" || SceneInfo.instance.sceneDef.baseSceneName != "mysteryspace")
                    {
                        //Custom spawn
                        //Drones
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMegaDrone", Settings.MegaDroneAmount, Settings.MegaDronePrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone1", Settings.GunnerDroneAmount, Settings.GunnerDronePrice, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenDrone2", Settings.HealerDroneAmount, Settings.HealerDronePrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenMissileDrone", Settings.MissileDroneAmount, Settings.MissileDronePrice, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenEquipmentDrone", Settings.EquipmentDroneAmount, -1, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenFlameDrone", Settings.FlameDroneAmount, Settings.FlameDronePrice, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBrokenTurret1", Settings.TurretAmount, Settings.TurretPrice, Run.instance.stageRng); //Non default

                        //Shrines
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineRestack", Settings.ShrineOfOrderAmount, -1, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineBlood", Settings.ShrineOfBloodAmount, -1, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineChance", Settings.ShrineOfChanceAmount, Settings.ShrineOfChancePrice, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineCombat", Settings.ShrineOfCombatAmount, -1, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineHealing", Settings.ShrineOfHealingAmount, Settings.ShrineOfHealingPrice, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShrineGoldshoresAccess", Settings.GoldShrineAmount, Settings.GoldShrinePrice, Run.instance.stageRng); //Non default

                        //Misc
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscBarrel1", Settings.CapsuleAmount, -1, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscRadarTower", Settings.RadarTowerAmount, Settings.RadarTowerPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscMSPortal", Settings.CelestialPortalAmount, -1, Run.instance.stageRng); //Non default
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscShopPortal", Settings.ShopPortalAmount, -1, Run.instance.stageRng); //Non default

                        //Duplicators
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicator", Settings.DuplicatorAmount, -1, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorLarge", Settings.DuplicatorLargeAmount, -1, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscDuplicatorMilitary", Settings.DuplicatorMilitaryAmount, -1, Run.instance.stageRng); //Non default

                        //Chests
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscGoldChest", Settings.GoldChestAmount, Settings.GoldChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest1", Settings.SmallChestAmount, Settings.SmallChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscChest2", Settings.LargeChestAmount, Settings.LargeChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestDamage", Settings.DamageChestAmount, Settings.DamageChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestHealing", Settings.HealingChestAmount, Settings.HealingChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscCategoryChestUtility", Settings.UtilityChestAmount, Settings.UtilityChestPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShop", Settings.TripleShopAmount, Settings.TripleShopPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscTripleShopLarge", Settings.TripleShopLargeAmount, Settings.TripleShopLargePrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscEquipmentBarrel", Settings.EquipmentBarrelAmount, Settings.EquipmentBarrelPrice, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscLockbox", Settings.LockboxAmount, -1, Run.instance.stageRng);
                        CustomGenerate("SpawnCards/InteractableSpawnCard/iscLunarChest", Settings.LunarChestAmount, -1, Run.instance.stageRng);
                    }
                }
            }
            orig(self);
        }

        static void BanItems(On.RoR2.Run.orig_BuildDropTable orig, Run self)
        {
            if (NetworkServer.active)
            {
                ItemIndex itemToRemove;
                EquipmentIndex equipmentToRemove;

                //Remove all items and equipments that is mentioned in the ban list
                for (int i = 0; i < Settings.BannedItemList.Count; i++)
                {
                    if (Enum.TryParse(Settings.BannedItemList[i], out itemToRemove)) Run.instance.availableItems.RemoveItem(itemToRemove);
                    if (Enum.TryParse(Settings.BannedItemList[i], out equipmentToRemove)) Run.instance.availableEquipment.RemoveEquipment(equipmentToRemove);
                }
            }
            orig(self);
        }
        #endregion

        #region Hook Functions
        static void CustomGenerate(string prefabPath, int amountToTrySpawn, int Price, Xoroshiro128Plus rng)
        {
            for (int i = 0; i < amountToTrySpawn; i++)
            {
                //Amount of attempts to try spawning this prefab before moving on
                int tries = 0;
                while (tries < 10)
                {
                    DirectorPlacementRule placementRule = new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    };
                    //Spawn
                    GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest((InteractableSpawnCard)Resources.Load(prefabPath), placementRule, rng));

                    if (spawnedObject)
                    {
                        //Find PurchaseInteraction
                        PurchaseInteraction purchaseInteraction = spawnedObject.GetComponent<PurchaseInteraction>();
                        if (purchaseInteraction)
                        {
                            if(purchaseInteraction.costType == CostTypeIndex.Money)
                            {
                                //Apply unscaled cost
                                purchaseInteraction.Networkcost = Price == -1 ? purchaseInteraction.cost : Price;
                                break;
                            }
                        }

                        break;
                    }
                    else tries++;
                }
            }
        }

        public static void SendPM(NetworkConnection conn, Chat.ChatMessageBase message)
        {
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.StartMessage(59);
            networkWriter.Write(message.GetTypeIndex());
            networkWriter.Write(message);
            networkWriter.FinishMessage();
            conn.SendWriter(networkWriter, QosChannelIndex.chat.intVal);
        }

        //public static void SendCustomPing(NetworkConnection conn, NetworkIdentity networkIdentity, GameObject self, PingerController.PingInfo incomingPing)
        //{
        //    NetworkWriter networkWriter = new NetworkWriter();
        //    networkWriter.Write(0);
        //    networkWriter.Write((short)((ushort)5));
        //    networkWriter.WritePackedUInt32((uint)1170265357);
        //    networkWriter.Write(networkIdentity.netId);
        //    networkWriter.Write(incomingPing.active);
        //    networkWriter.Write(incomingPing.origin);
        //    networkWriter.Write(incomingPing.normal);
        //    networkWriter.Write(incomingPing.targetNetworkIdentity);
        //    networkWriter.FinishMessage();
        //    conn.SendWriter(networkWriter, QosChannelIndex.ping.intVal);
        //}
        #endregion
    }
}
