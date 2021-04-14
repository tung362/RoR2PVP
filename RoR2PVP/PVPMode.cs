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
using RoR2.CharacterAI;
using EntityStates;
using APIExtension.VoteAPI;
using RoR2PVP.UI;

namespace RoR2PVP
{
    public static class PVPMode
    {
        /*Data*/
        public static bool IsGracePeriod = true;
        public static float GraceTimer;
        public static float CashGrantTimer = 0;
        public static float CurrentGraceTimeReminder;
        public static bool PVPEnded = false;
        public static bool UsedTeleporter = false;
        public static bool PreventGameOver = false;
        public static TeleporterInteraction SecondaryTeleporter;

        public static List<SceneDef> Destinations = new List<SceneDef>();
        public static SceneDef FinalDestination;

        public static Dictionary<PlayerCharacterMasterController, TeamPicker.StateType> PlayerStates = new Dictionary<PlayerCharacterMasterController, TeamPicker.StateType>();

        #region Core
        public static void Reset()
        {
            IsGracePeriod = true;
            GraceTimer = Settings.GraceTimerDuration;
            CashGrantTimer = 0;
            CurrentGraceTimeReminder = GraceTimer;
            PVPEnded = false;
            UsedTeleporter = false;
            PreventGameOver = false;
            SecondaryTeleporter = null;

            if (NetworkServer.active)
            {
                //Spawns a normal teleporter on the stage to prevent being forced to use a specific teleporter or being softlocked ending the run
                if (SceneInfo.instance.sceneDef.baseSceneName == "skymeadow" ||
                SceneInfo.instance.sceneDef.baseSceneName == "artifactworld" ||
                SceneInfo.instance.sceneDef.baseSceneName == "goldshores")
                {
                    SecondaryTeleporter = Tools.CustomGenerate("SpawnCards/InteractableSpawnCard/iscTeleporter", -1, Run.instance.stageRng).GetComponent<TeleporterInteraction>();
                }
            }
        }

        public static void Tick()
        {
            if (NetworkServer.active)
            {
                //Failsafe death plane
                if(VoteAPI.VoteResults.HasVote(Settings.UseDeathPlaneFailsafe.Item2)) BruteforceDeathPlane();

                //Safe zones
                if (SceneInfo.instance.sceneDef.baseSceneName == "bazaar" ||
                    SceneInfo.instance.sceneDef.baseSceneName == "mysteryspace" ||
                    SceneInfo.instance.sceneDef.baseSceneName == "moon" ||
                    SceneInfo.instance.sceneDef.baseSceneName == "arena") return;

                /*Grace period*/
                if (IsGracePeriod)
                {
                    /*Grace period countdown*/
                    if (GraceTimer >= 0f)
                    {
                        CashGrantTimerTick(VoteAPI.VoteResults.HasVote(Settings.CustomInteractablesSpawner.Item2) ? Settings.CashGrantAmount : (uint)Run.instance.GetDifficultyScaledCost((int)Settings.CashGrantAmount));
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
                        //Free for all pvp
                        if(VoteAPI.VoteResults.HasVote(Settings.FreeForAllPVPToggle.Item2))
                        {
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
                        }
                        //Team pvp
                        else
                        {
                            //Random teams
                            if (VoteAPI.VoteResults.HasVote(Settings.RandomTeams.Item2))
                            {
                                //Shuffle players
                                Tools.Shuffle<PlayerCharacterMasterController>(players);

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
                            //Free for all pvp
                            if (VoteAPI.VoteResults.HasVote(Settings.FreeForAllPVPToggle.Item2))
                            {
                                AnnounceWinningPlayer(survivingPlayer);
                                FreeForAll(false);

                                if (!survivingPlayer) teleportCheck = false;
                            }
                            //Team pvp
                            else
                            {
                                AnnounceWinningTeam(survivingTeamIndex);
                                ChangeTeams(TeamIndex.Player);
                                UpdateCompanionTeams();

                                if (survivingTeamIndex == TeamIndex.None || survivingTeamIndex == TeamIndex.Count) teleportCheck = false;
                            }
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

        public static void Teleport(TeleporterInteraction self)
        {
            if (!UsedTeleporter)
            {
                if (PVPEnded)
                {
                    ApplyExp();
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
        #endregion;

        #region Core Functions
        public static void LoadFixedTeams()
        {
            PlayerStates.Clear();
            //Assign teams
            int team1Count = 0;
            int team2Count = 0;
            List<NetworkUser> pendingPlayers = new List<NetworkUser>();
            foreach(KeyValuePair<NetworkUser, TeamPicker.Slot> kv in TeamPicker.PlayerStates)
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
                    else if(kv.Value.State == TeamPicker.StateType.Team2)
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

        public static void LoadDestinations()
        {
            Destinations.Clear();
            FinalDestination = null;
            Tools.TryAddStage("artifactworld", Destinations);
            Tools.TryAddStage("blackbeach", Destinations);
            Tools.TryAddStage("dampcavesimple", Destinations);
            Tools.TryAddStage("foggyswamp", Destinations);
            Tools.TryAddStage("frozenwall", Destinations);
            Tools.TryAddStage("goldshores", Destinations);
            Tools.TryAddStage("golemplains", Destinations);
            Tools.TryAddStage("goolake", Destinations);
            Tools.TryAddStage("shipgraveyard", Destinations);
            Tools.TryAddStage("wispgraveyard", Destinations);
            Tools.TryAddStage("rootjungle", Destinations);

            if (Tools.TryGetStage("skymeadow", out SceneDef finalStage)) FinalDestination = finalStage;
        }

        public static void CustomStageTransition()
        {
            if (VoteAPI.VoteResults.HasVote(Settings.WiderStageTransitions.Item2))
            {
                if (Destinations.Contains(SceneInfo.instance.sceneDef))
                {
                    Destinations.Remove(SceneInfo.instance.sceneDef);
                    if (Destinations.Count == 0)
                    {
                        LoadDestinations();
                        Tools.Shuffle<SceneDef>(Destinations);
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

        static void Countdown()
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

        static void FreeForAll(bool toggle)
        {
            RunArtifactManager.instance.SetArtifactEnabledServer(ArtifactCatalog.FindArtifactDef("FriendlyFire"), toggle);
            TeamCatalog.GetTeamDef(TeamIndex.Player).friendlyFireScaling = 1.0f;
            TeamCatalog.GetTeamDef(TeamIndex.Neutral).friendlyFireScaling = 1.0f;
            TeamCatalog.GetTeamDef(TeamIndex.Monster).friendlyFireScaling = 1.0f;
        }

        static void FilterDCedPlayers(List<PlayerCharacterMasterController> listToFilter)
        {
            for (int i = listToFilter.Count - 1; i >= 0; i--)
            {
                if (listToFilter[i].master.IsDeadAndOutOfLivesServer() || listToFilter[i].master.GetBody() == null) listToFilter.RemoveAt(i);
            }
        }

        static void PMAboutAllies()
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
                    if(PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner != null)
                    {
                        Tools.SendPM(PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner, new Chat.SimpleChatMessage
                        {
                            baseToken = text
                        });
                    }
                }
            }
        }

        static void CashGrantTimerTick(uint cashAmount)
        {
            CashGrantTimer += Time.fixedDeltaTime;

            //Grant cash to each player
            if (CashGrantTimer >= Settings.CashDelay)
            {
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++) PlayerCharacterMasterController.instances[i].master.GiveMoney(cashAmount);
                CashGrantTimer -= Settings.CashDelay;
            }
        }

        static void ChangeTeams(TeamIndex teamIndex)
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

        static void UpdateCompanionTeams()
        {
            AIOwnership[] companions = GameObject.FindObjectsOfType<AIOwnership>();
            for (int i = 0; i < companions.Length; i++)
            {
                CharacterMaster master = companions[i].GetComponent<CharacterMaster>();
                if (companions[i].ownerMaster && master != null)
                {
                    if(master.playerCharacterMasterController == null)
                    {
                        CharacterBody companion = master.GetBody();
                        CharacterBody player = companions[i].ownerMaster.GetBody();

                        master.teamIndex = companions[i].ownerMaster.teamIndex;
                        if (companion != null && player != null) companion.teamComponent.teamIndex = player.teamComponent.teamIndex;
                    }
                }
            }
        }

        static void DestroyAllPurchaseables()
        {
            PurchaseInteraction[] purchaseables = GameObject.FindObjectsOfType<PurchaseInteraction>();
            for (int i = 0; i < purchaseables.Length; i++)
            {
                if(purchaseables[i].costType == CostTypeIndex.Money ||
                   purchaseables[i].costType == CostTypeIndex.PercentHealth ||
                   purchaseables[i].costType == CostTypeIndex.WhiteItem ||
                   purchaseables[i].costType == CostTypeIndex.GreenItem ||
                   purchaseables[i].costType == CostTypeIndex.RedItem ||
                   purchaseables[i].costType == CostTypeIndex.LunarCoin) NetworkServer.Destroy(purchaseables[i].gameObject);
            }
        }

        static bool DeathCheck(out TeamIndex teamIndexOutput, out CharacterMaster playerOutput)
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

                    //Free for all pvp
                    if (VoteAPI.VoteResults.HasVote(Settings.FreeForAllPVPToggle.Item2))
                    {
                        //Checks if any player is still alive
                        if (!playerOutput) playerOutput = PlayerCharacterMasterController.instances[i].master;
                        else Win = false;
                    }
                    //Team pvp
                    else
                    {
                        //Checks if any team member are still alive
                        if (teamIndexOutput == TeamIndex.None) teamIndexOutput = PlayerCharacterMasterController.instances[i].master.teamIndex;
                        else
                        {
                            if (teamIndexOutput != PlayerCharacterMasterController.instances[i].master.teamIndex) Win = false;
                        }
                    }
                }
            }

            //If all real players are dead but there's still bots that's alive, prevent a game over and declare a draw
            if (allRealPlayersDead && !EveryoneIsDead)
            {
                PreventGameOver = true;
                teamIndexOutput = TeamIndex.None;
                playerOutput = null;
            }

            return Win || allRealPlayersDead ? true : false;
        }

        static void AnnounceWinningTeam(TeamIndex teamIndex)
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

        static void AnnounceWinningPlayer(CharacterMaster player)
        {
            string playerText = "No real player";
            string resultText = "It's a draw!";
            string conclusionText = "Teleporting...";
            if(player)
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

        static void ApplyExp()
        {
            TeamManager.instance.SetTeamLevel(TeamIndex.Player, TeamManager.instance.GetTeamLevel(TeamIndex.Player) + 5u);
            TeamManager.instance.SetTeamLevel(TeamIndex.Neutral, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
            TeamManager.instance.SetTeamLevel(TeamIndex.Monster, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
        }

        static void BruteforceDeathPlane()
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                {
                    if(PlayerCharacterMasterController.instances[i].master.GetBody().transform.position.y <= -2200) PlayerCharacterMasterController.instances[i].master.GetBody().healthComponent.Suicide();
                }
            }
        }
        #endregion
    }
}
