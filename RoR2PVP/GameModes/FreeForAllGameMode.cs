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
    /// Custom free for all pvp game mode
    /// </summary>
    public class FreeForAllGameMode : PVPGameMode
    {
        #region GameModeAPI Managed
        /// <summary>
        /// Constructor, assigns the game mode's name
        /// </summary>
        /// <param name="gameModeName">Game mode's name, ensure each name is unique</param>
        public FreeForAllGameMode(string gameModeName) : base(gameModeName) {}

        protected override void Start(Stage self)
        {
            base.Start(self);
        }

        protected override void Update(Stage self)
        {
            base.Update(self);
            if (NetworkServer.active)
            {
                //Failsafe death plane
                if (UseDeathPlaneFailsafe) BruteforceDeathPlane();

                //Safe zones
                if (SafeZoneStages.Contains(SceneInfo.instance.sceneDef.baseSceneName)) return;

                /*Grace period*/
                if (IsGracePeriod)
                {
                    /*Grace period countdown*/
                    if (GraceTimer >= 0f)
                    {
                        CashGrantTimerTick(AllowVanillaInteractableSpawns ? (uint)Run.instance.GetDifficultyScaledCost((int)Settings.CashGrantAmount) : Settings.CashGrantAmount);
                        Countdown();
                    }
                    else
                    {
                        /*Transitioning to pvp period*/
                        Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                        {
                            baseToken = Util.GenerateColoredString("PVP Enabled. Fight!", new Color32(255, 0, 0, 255))
                        });

                        List<PlayerCharacterMasterController> players = Enumerable.ToList<PlayerCharacterMasterController>(PlayerCharacterMasterController.instances);
                        FilterDCedPlayers(players);
                        FreeForAll(true);

                        //Grant respawn items, and reset money count
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (!players[i].master.IsDeadAndOutOfLivesServer() && players[i].master.GetBody() != null)
                            {
                                //Grant respawns
                                if (Settings.RespawnsPerRound != 0)
                                {
                                    players[i].master.GetBody().inventory.RemoveItem(RoR2Content.Items.ExtraLife, 9999);
                                    players[i].master.GetBody().inventory.RemoveItem(RoR2Content.Items.ExtraLifeConsumed, 9999);
                                    players[i].master.GetBody().inventory.GiveItem(RoR2Content.Items.ExtraLife, Settings.RespawnsPerRound);
                                }
                                //Reset cash
                                players[i].master.money = 0u;
                            }
                        }
                        DestroyAllPurchaseables();
                        IsGracePeriod = false;
                    }
                }
                else
                {
                    /*PVP period*/
                    if (!PVPEnded)
                    {
                        //Check for surviving teams
                        if (DeathCheck(out TeamIndex survivingTeamIndex, out CharacterMaster survivingPlayer))
                        {
                            bool teleportCheck = true;
                            AnnounceWinningPlayer(survivingPlayer);
                            FreeForAll(false);

                            if (!survivingPlayer) teleportCheck = false;
                            CustomStageTransition();

                            if (SecondaryTeleporter)
                            {
                                Type chargingState = typeof(TeleporterInteraction).GetNestedType("ChargingState", BindingFlags.NonPublic);
                                object chargingStateInstance = Activator.CreateInstance(chargingState);
                                SecondaryTeleporter.mainStateMachine.state.outer.SetNextState((EntityState)chargingStateInstance);
                                SecondaryTeleporter.holdoutZoneController.Network_charge = 0.98f;
                                SecondaryTeleporter.bonusDirector = null;
                                SecondaryTeleporter.bossDirector = null;

                                if (SecondaryTeleporter.sceneExitController && !teleportCheck)
                                {
                                    SecondaryTeleporter.sceneExitController.Begin();
                                    teleportCheck = true;
                                }
                            }
                            if (TeleporterInteraction.instance)
                            {
                                Type chargingState = typeof(TeleporterInteraction).GetNestedType("ChargingState", BindingFlags.NonPublic);
                                object chargingStateInstance = Activator.CreateInstance(chargingState);
                                TeleporterInteraction.instance.mainStateMachine.state.outer.SetNextState((EntityState)chargingStateInstance);
                                TeleporterInteraction.instance.holdoutZoneController.Network_charge = 0.98f;
                                TeleporterInteraction.instance.bonusDirector = null;
                                TeleporterInteraction.instance.bossDirector = null;

                                if (TeleporterInteraction.instance.sceneExitController && !teleportCheck)
                                {
                                    TeleporterInteraction.instance.sceneExitController.Begin();
                                    teleportCheck = true;
                                }
                            }
                            //If teleporter failed then manually switch stages
                            if (!teleportCheck) Run.instance.AdvanceStage(Run.instance.nextStageScene);
                            PVPEnded = true;
                        }
                    }
                }
            }
        }

        protected override void OnTeleporterInteraction(TeleporterInteraction self, Interactor activator)
        {
            base.OnTeleporterInteraction(self, activator);
        }

        protected override void OnGameOver(Run self, GameEndingDef gameEndingDef)
        {
            base.OnGameOver(self, gameEndingDef);
        }

        protected override void OnPlayerRespawn(Stage self, CharacterMaster characterMaster)
        {
            base.OnPlayerRespawn(self, characterMaster);
        }

        protected override void OnStagePopulate(SceneDirector self)
        {
            base.OnStagePopulate(self);
        }

        public override void SetExtraHooks()
        {
            base.SetExtraHooks();
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += CustomAITargetFilter;
        }

        public override void UnsetExtraHooks()
        {
            base.UnsetExtraHooks();
            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox -= CustomAITargetFilter;
        }
        #endregion

        #region Extra Hooks
        /// <summary>
        /// Hook for custom ai filter search behavior
        /// </summary>
        HurtBox CustomAITargetFilter(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, RoR2.CharacterAI.BaseAI self, float maxDistance, bool full360Vision, bool filterByLoS)
        {
            if (!self.body) return null;

            bool defaultSearch = true;
            bool isPlayerBot = self.master.playerCharacterMasterController ? true : false;
            bool isMinion = self.master.minionOwnership.ownerMaster ? true : false;
            bool isPlayerMinion = false;

            if (isMinion)
            {
                if (self.master.minionOwnership.ownerMaster.playerCharacterMasterController) isPlayerMinion = true;
            }

            BullseyeSearch enemySearch = self.GetFieldValue<BullseyeSearch>("enemySearch");
            enemySearch.viewer = self.body;
            enemySearch.teamMaskFilter = TeamMask.all;
            enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
            enemySearch.minDistanceFilter = 0f;
            enemySearch.maxDistanceFilter = maxDistance;
            enemySearch.searchOrigin = self.bodyInputBank.aimOrigin;
            enemySearch.searchDirection = self.bodyInputBank.aimDirection;
            enemySearch.maxAngleFilter = (full360Vision ? 180f : 90f);
            enemySearch.filterByLoS = filterByLoS;

            //Player bot AI  or player minion
            if (isPlayerBot || isPlayerMinion)
            {
                if (IsGracePeriod) enemySearch.teamMaskFilter.RemoveTeam(self.master.teamIndex);
                enemySearch.RefreshCandidates();

                enemySearch.FilterOutGameObject(self.body.gameObject);
                if (isMinion)
                {
                    CharacterBody ownerBody = self.master.minionOwnership.ownerMaster.GetBody();
                    if (ownerBody) enemySearch.FilterOutGameObject(ownerBody.gameObject);
                    enemySearch.FilterOutMinionGroup(self.master.minionOwnership.ownerMaster.netId);
                }
                else enemySearch.FilterOutMinionGroup(self.master.netId);

                defaultSearch = false;
            }

            //Regular mobs AI
            if (defaultSearch)
            {
                enemySearch.teamMaskFilter.RemoveTeam(self.master.teamIndex);
                enemySearch.RefreshCandidates();
            }

            return enemySearch.GetResults().FirstOrDefault<HurtBox>();
        }
        #endregion

        #region Functions
        /// <summary>
        /// Checks if winning conditions are met and outputs the winner
        /// </summary>
        /// <param name="teamIndexOutput">Winning team output
        /// <para>Outputs TeamIndex.None if there is no winner</para></param>
        /// <param name="playerOutput">Winning player output
        /// <para>Outputs null if there is no winner</para></param>
        /// <returns></returns>
        bool DeathCheck(out TeamIndex teamIndexOutput, out CharacterMaster playerOutput)
        {
            teamIndexOutput = TeamIndex.None;
            playerOutput = null;
            bool Win = true;
            bool allRealPlayersDead = true;
            bool EveryoneIsDead = true;

            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                //Checks if any team members are still alive (both real players and bots)
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody())
                {
                    EveryoneIsDead = false;
                    //Checks if any real players are still alive
                    if (PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner != null) allRealPlayersDead = false;

                    //Checks if any player is still alive
                    if (!playerOutput) playerOutput = PlayerCharacterMasterController.instances[i].master;
                    else Win = false;
                }
            }

            //If all real players are dead but there's still bots that's alive, prevent a game over and declare a draw
            if (allRealPlayersDead && !EveryoneIsDead)
            {
                AllowVanillaGameOver = false;
                teamIndexOutput = TeamIndex.None;
                playerOutput = null;
            }

            return Win || allRealPlayersDead ? true : false;
        }
        #endregion

        #region Utils
        /// <summary>
        /// Announces the winning player
        /// </summary>
        /// <param name="player">The winning player</param>
        void AnnounceWinningPlayer(CharacterMaster player)
        {
            string playerText = "No real player";
            string resultText = "It's a draw!";
            string conclusionText = "Teleporting...";
            if (player)
            {
                playerText = player.GetBody().GetUserName();
                resultText = "But can they keep it up?";
                conclusionText = "Head towards the teleporter for the next round.";
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(playerText, new Color32(76, byte.MaxValue, 0, byte.MaxValue)) + " has survived... " + resultText
            });
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(conclusionText, new Color32(0, 255, 255, 255))
            });
        }

        /// <summary>
        /// Enables/disables free for all pvp
        /// </summary>
        /// <param name="toggle">Toggle pvp</param>
        void FreeForAll(bool toggle)
        {
            RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.FindArtifactDef("FriendlyFire"), toggle);
            TeamCatalog.GetTeamDef(TeamIndex.Player).friendlyFireScaling = 1.0f;
            TeamCatalog.GetTeamDef(TeamIndex.Neutral).friendlyFireScaling = 1.0f;
            TeamCatalog.GetTeamDef(TeamIndex.Monster).friendlyFireScaling = 1.0f;
        }
        #endregion
    }
}
