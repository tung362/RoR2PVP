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
using RoR2PVP.UnityScripts;

namespace RoR2PVP
{
    class PVPMode
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
        //Unused for now
        public static List<PVPTeamTrackerStruct> PVPTeams = new List<PVPTeamTrackerStruct>();

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
            PVPTeams.Clear();

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
                        //Shuffle teams, grant respawn items, and reset money count
                        if(VoteAPI.VoteResults.HasVote(Settings.RandomTeams.Item2)) Tools.Shuffle<PlayerCharacterMasterController>(players);
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (!players[i].master.IsDeadAndOutOfLivesServer() && players[i].master.GetBody() != null)
                            {
                                //Split players into 2 teams
                                if (i % 2 == 0)
                                {
                                    players[i].master.teamIndex = TeamIndex.Player;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                    PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Player));
                                }
                                else
                                {
                                    players[i].master.teamIndex = TeamIndex.Neutral;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                    PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Neutral));
                                }
                                //Grant respawns
                                if(Settings.RespawnsPerRound != 0)
                                {
                                    players[i].master.GetBody().inventory.RemoveItem(ItemIndex.ExtraLife, 9999);
                                    players[i].master.GetBody().inventory.RemoveItem(ItemIndex.ExtraLifeConsumed, 9999);
                                    players[i].master.GetBody().inventory.GiveItem(ItemIndex.ExtraLife, Settings.RespawnsPerRound);
                                }
                                //Reset cash
                                players[i].master.money = 0u;
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
                        if (DeathCheck(out TeamIndex survivingTeamIndex))
                        {
                            AnnounceWinningTeam(survivingTeamIndex);
                            ChangeTeams(TeamIndex.Player);
                            UpdateCompanionTeams();
                            CustomStageTransition();

                            bool teleportCheck = survivingTeamIndex == TeamIndex.None || survivingTeamIndex == TeamIndex.Count ? false : true;
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

        static bool DeathCheck(out TeamIndex teamIndexOutput)
        {
            TeamIndex survivingTeamIndex = TeamIndex.None;
            bool allTeammatesDead = true;
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
                    if (survivingTeamIndex == TeamIndex.None) survivingTeamIndex = PlayerCharacterMasterController.instances[i].master.teamIndex;
                    else
                    {
                        if (survivingTeamIndex != PlayerCharacterMasterController.instances[i].master.teamIndex) allTeammatesDead = false;
                    }
                }
            }

            //If all real players are dead but there's still bots that's alive, prevent a game over and declare a draw
            if (allRealPlayersDead && !EveryoneIsDead)
            {
                PreventGameOver = true;
                survivingTeamIndex = TeamIndex.None;
            }

            teamIndexOutput = survivingTeamIndex;
            return allTeammatesDead || allRealPlayersDead ? true : false;
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

        static void PingTeleporter()
        {
            PingerController.PingInfo teleporterPing = new PingerController.PingInfo
            {
                active = true,
                normal = new Vector3(0, 1, 0),
                targetNetworkIdentity = TeleporterInteraction.instance.GetComponent<NetworkIdentity>()
            };

            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].GetComponent<PingerController>().CallCmdPing(teleporterPing);
            }
        }

        static void ApplyExp()
        {
            TeamManager.instance.SetTeamLevel(TeamIndex.Player, TeamManager.instance.GetTeamLevel(TeamIndex.Player) + 5u);
            TeamManager.instance.SetTeamLevel(TeamIndex.Neutral, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
            TeamManager.instance.SetTeamLevel(TeamIndex.Monster, TeamManager.instance.GetTeamLevel(TeamIndex.Player));
        }

        static void CreateDeathPlane(Vector3 pos)
        {
            /*Ghetto death plane*/
            //Note: if you still somehow get past this death plane, then there was either no character body component or you desynced from the server in which case i can't do anything for you.
            //Create death plane
            GameObject DeathPlane = new GameObject();
            DeathPlane.transform.localScale = new Vector3(2000, 2, 2000);
            DeathPlane.transform.position = pos;
            DeathPlane.layer = 10;
            //Create collider
            BoxCollider boxCollider = DeathPlane.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(1, 1, 1);
            DeathPlane.AddComponent<RoR2TeamPVPDeathPlane>();
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
