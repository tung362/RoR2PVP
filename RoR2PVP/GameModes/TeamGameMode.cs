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
using RoR2.CharacterAI;
using R2API;
using R2API.Utils;
using RoR2GameModeAPI;
using RoR2GameModeAPI.Utils;
using RoR2PVP.UI;

namespace RoR2PVP.GameModes
{
    /// <summary>
    /// Custom team pvp game mode
    /// </summary>
    public class TeamGameMode : PVPGameMode
    {
        /*Extended Game Mode Settings*/
        public bool RandomTeams = Settings.RandomTeams.Item1;

        /*Cache*/
        public Dictionary<PlayerCharacterMasterController, TeamPicker.StateType> PlayerStates = new Dictionary<PlayerCharacterMasterController, TeamPicker.StateType>();

        #region GameModeAPI Managed
        /// <summary>
        /// Constructor, assigns the game mode's name
        /// </summary>
        /// <param name="gameModeName">Game mode's name, ensure each name is unique</param>
        public TeamGameMode(string gameModeName) : base(gameModeName) {}

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

                        //Random teams
                        if (RandomTeams)
                        {
                            //Shuffle players
                            RoR2PVPUtils.Shuffle<PlayerCharacterMasterController>(players);

                            //Assign teams, grant respawn items, and reset money count
                            for (int i = 0; i < players.Count; i++)
                            {
                                if (!players[i].master.IsDeadAndOutOfLivesServer() && players[i].master.GetBody() != null)
                                {
                                    //Split players into 2 teams
                                    if (i % 2 == 0)
                                    {
                                        players[i].master.teamIndex = TeamIndex.Player;
                                        players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                    }
                                    else
                                    {
                                        players[i].master.teamIndex = TeamIndex.Neutral;
                                        players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                    }
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
                        }
                        //Fixed teams
                        else
                        {
                            //Assign teams, grant respawn items, and reset money count
                            int team1Count = 0;
                            int team2Count = 0;
                            List<PlayerCharacterMasterController> pendingPlayers = new List<PlayerCharacterMasterController>();
                            for (int i = 0; i < players.Count; i++)
                            {
                                if (!players[i].master.IsDeadAndOutOfLivesServer() && players[i].master.GetBody() != null)
                                {
                                    //Existing player
                                    if (PlayerStates.ContainsKey(players[i]))
                                    {
                                        switch (PlayerStates[players[i]])
                                        {
                                            case TeamPicker.StateType.Team1:
                                                players[i].master.teamIndex = TeamIndex.Player;
                                                players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                                team1Count++;
                                                break;
                                            case TeamPicker.StateType.Team2:
                                                players[i].master.teamIndex = TeamIndex.Neutral;
                                                players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                                team2Count++;
                                                break;
                                            default:
                                                Debug.LogWarning("Warning! Player state registered but is not assigned a team, this should never happen! @RoR2PVP");
                                                break;
                                        }
                                    }
                                    //New Player
                                    else
                                    {
                                        switch (TeamPicker.UnassignAction)
                                        {
                                            case TeamPicker.UnassignType.LeastMembers:
                                                pendingPlayers.Add(players[i]);
                                                break;
                                            case TeamPicker.UnassignType.Team1:
                                                PlayerStates.Add(players[i], TeamPicker.StateType.Team1);
                                                players[i].master.teamIndex = TeamIndex.Player;
                                                players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                                team1Count++;
                                                break;
                                            case TeamPicker.UnassignType.Team2:
                                                PlayerStates.Add(players[i], TeamPicker.StateType.Team2);
                                                players[i].master.teamIndex = TeamIndex.Neutral;
                                                players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                                team2Count++;
                                                break;
                                        }
                                    }
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

                            //Assign to the team with the least members
                            for (int i = 0; i < pendingPlayers.Count; i++)
                            {
                                if (!PlayerStates.ContainsKey(pendingPlayers[i]))
                                {
                                    if (team1Count <= team2Count)
                                    {
                                        PlayerStates.Add(pendingPlayers[i], TeamPicker.StateType.Team1);
                                        pendingPlayers[i].master.teamIndex = TeamIndex.Player;
                                        pendingPlayers[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                        team1Count++;
                                    }
                                    else
                                    {
                                        PlayerStates.Add(pendingPlayers[i], TeamPicker.StateType.Team2);
                                        pendingPlayers[i].master.teamIndex = TeamIndex.Neutral;
                                        pendingPlayers[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                        team2Count++;
                                    }
                                }
                            }
                        }
                        PMAboutAllies();
                        UpdateCompanionTeams();
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
                            AnnounceWinningTeam(survivingTeamIndex);
                            ChangeTeams(TeamIndex.Player);
                            UpdateCompanionTeams();

                            if (survivingTeamIndex == TeamIndex.None || survivingTeamIndex == TeamIndex.Count) teleportCheck = false;
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

        #region Setup
        /// <summary>
        /// Apply team loadouts set by the team picker during the lobby scene
        /// </summary>
        public void LoadFixedTeams()
        {
            PlayerStates.Clear();
            //Assign teams
            int team1Count = 0;
            int team2Count = 0;
            List<NetworkUser> pendingPlayers = new List<NetworkUser>();
            foreach (KeyValuePair<NetworkUser, TeamPicker.Slot> kv in TeamPicker.PlayerStates)
            {
                if (!PlayerStates.ContainsKey(kv.Key.masterController))
                {
                    //Team 1
                    if (kv.Value.State == TeamPicker.StateType.Team1)
                    {
                        PlayerStates.Add(kv.Key.masterController, kv.Value.State);
                        team1Count++;
                    }
                    //Team 2
                    else if (kv.Value.State == TeamPicker.StateType.Team2)
                    {
                        PlayerStates.Add(kv.Key.masterController, kv.Value.State);
                        team2Count++;
                    }
                    //Unassigned actions
                    else
                    {
                        switch (TeamPicker.UnassignAction)
                        {
                            case TeamPicker.UnassignType.LeastMembers:
                                pendingPlayers.Add(kv.Key);
                                break;
                            case TeamPicker.UnassignType.Team1:
                                PlayerStates.Add(kv.Key.masterController, TeamPicker.StateType.Team1);
                                team1Count++;
                                break;
                            case TeamPicker.UnassignType.Team2:
                                PlayerStates.Add(kv.Key.masterController, TeamPicker.StateType.Team2);
                                team2Count++;
                                break;
                        }
                    }
                }
            }

            //Assign to the team with the least members
            for (int i = 0; i < pendingPlayers.Count; i++)
            {
                if (!PlayerStates.ContainsKey(pendingPlayers[i].masterController))
                {
                    if (team1Count <= team2Count)
                    {
                        PlayerStates.Add(pendingPlayers[i].masterController, TeamPicker.StateType.Team1);
                        team1Count++;
                    }
                    else
                    {
                        PlayerStates.Add(pendingPlayers[i].masterController, TeamPicker.StateType.Team2);
                        team2Count++;
                    }
                }
            }
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

                    //Checks if any team member are still alive
                    if (teamIndexOutput == TeamIndex.None) teamIndexOutput = PlayerCharacterMasterController.instances[i].master.teamIndex;
                    else
                    {
                        if (teamIndexOutput != PlayerCharacterMasterController.instances[i].master.teamIndex) Win = false;
                    }
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
        /// Change all minion's team index to match those of thier owner's
        /// </summary>
        void UpdateCompanionTeams()
        {
            AIOwnership[] companions = GameObject.FindObjectsOfType<AIOwnership>();
            for (int i = 0; i < companions.Length; i++)
            {
                CharacterMaster master = companions[i].GetComponent<CharacterMaster>();
                if (companions[i].ownerMaster && master != null)
                {
                    if (master.playerCharacterMasterController == null)
                    {
                        CharacterBody companion = master.GetBody();
                        CharacterBody player = companions[i].ownerMaster.GetBody();

                        master.teamIndex = companions[i].ownerMaster.teamIndex;
                        if (companion != null && player != null) companion.teamComponent.teamIndex = player.teamComponent.teamIndex;
                    }
                }
            }
        }

        /// <summary>
        /// Sends a private message to all players informing them of thier allies
        /// </summary>
        void PMAboutAllies()
        {
            //PM ally list
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                {
                    string text = Util.GenerateColoredString("Your allies: ", new Color32(0, 255, 255, 255));
                    for (int j = 0; j < PlayerCharacterMasterController.instances.Count; j++)
                    {
                        if (!PlayerCharacterMasterController.instances[j].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[j].master.GetBody() != null)
                        {
                            if (PlayerCharacterMasterController.instances[i].master != PlayerCharacterMasterController.instances[j].master &&
                                PlayerCharacterMasterController.instances[i].master.GetBody().teamComponent.teamIndex == PlayerCharacterMasterController.instances[j].master.GetBody().teamComponent.teamIndex)
                            {
                                text += Util.GenerateColoredString(PlayerCharacterMasterController.instances[j].GetDisplayName(), new Color32(0, 255, 0, 255)) + ", ";
                            }
                        }
                    }
                    if (text == Util.GenerateColoredString("Your allies: ", new Color32(0, 255, 255, 255))) text += "None";
                    if (PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner != null)
                    {
                        RoR2PVPUtils.SendPM(PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner, new Chat.SimpleChatMessage
                        {
                            baseToken = text
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Announces the winning team
        /// </summary>
        /// <param name="teamIndex">The winning team</param>
        void AnnounceWinningTeam(TeamIndex teamIndex)
        {
            string teamText = "";
            string resultText = "But can they keep it up?";
            string conclusionText = "Head towards the teleporter for the next round.";
            switch (teamIndex)
            {
                case TeamIndex.Player:
                    teamText = "Team 1";
                    break;
                case TeamIndex.Neutral:
                    teamText = "Team 2";
                    break;
                case TeamIndex.Monster:
                    teamText = "Team 3";
                    break;
                default:
                    teamText = "No real player";
                    resultText = "It's a draw!";
                    conclusionText = "Teleporting...";
                    break;
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(teamText, new Color32(76, byte.MaxValue, 0, byte.MaxValue)) + " has survived... " + resultText
            });
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(conclusionText, new Color32(0, 255, 255, 255))
            });
        }

        /// <summary>
        /// Changes all player's team index to the specified team index
        /// </summary>
        /// <param name="teamIndex">Team index to change to</param>
        void ChangeTeams(TeamIndex teamIndex)
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].master.teamIndex = teamIndex;
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                {
                    PlayerCharacterMasterController.instances[i].master.GetBody().teamComponent.teamIndex = teamIndex;
                }
            }
        }
        #endregion
    }
}
