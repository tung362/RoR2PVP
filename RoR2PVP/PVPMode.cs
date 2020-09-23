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
        //Unused for now
        public static List<PVPTeamTrackerStruct> PVPTeams = new List<PVPTeamTrackerStruct>();

        #region Core
        public static void Reset()
        {
            PVPMode.IsGracePeriod = true;
            PVPMode.GraceTimer = Settings.GraceTimerDuration;
            PVPMode.CashGrantTimer = 0;
            PVPMode.CurrentGraceTimeReminder = PVPMode.GraceTimer;
            PVPMode.PVPEnded = false;
            PVPMode.UsedTeleporter = false;
            PVPMode.PVPTeams.Clear();
        }

        public static void Tick()
        {
            if (NetworkServer.active)
            {
                //Failsafe death plane
                if(VoteAPI.VoteResults.HasVote(Settings.UseDeathPlaneFailsafe.Item2)) BruteforceDeathPlane();

                //Gold shores as per usual but with the pvp mechanic in place
                if (SceneInfo.instance.sceneDef.baseSceneName == "goldshores")
                {
                    CashGrantTimerTick(1000u);
                    return;
                }

                //Safe zones
                if (SceneInfo.instance.sceneDef.baseSceneName == "bazaar" || SceneInfo.instance.sceneDef.baseSceneName == "mysteryspace" || SceneInfo.instance.sceneDef.baseSceneName == "moon") return;

                /*Grace period*/
                if (PVPMode.IsGracePeriod)
                {
                    /*Grace period countdown*/
                    if (PVPMode.GraceTimer >= 0f)
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
                        if(VoteAPI.VoteResults.HasVote(Settings.RandomTeams.Item2)) Shuffle<PlayerCharacterMasterController>(players);
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (!players[i].master.IsDeadAndOutOfLivesServer() && players[i].master.GetBody() != null)
                            {
                                //Split players into 2 teams
                                if (i % 2 == 0)
                                {
                                    players[i].master.teamIndex = TeamIndex.Player;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                    PVPMode.PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Player));
                                }
                                else
                                {
                                    players[i].master.teamIndex = TeamIndex.Neutral;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                    PVPMode.PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Neutral));
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

                        PVPMode.IsGracePeriod = false;
                    }
                }
                else
                {
                    /*PVP period*/
                    if (!PVPMode.PVPEnded)
                    {
                        TeamIndex survivingTeamIndex;

                        //Check for surviving teams
                        if (DeathCheck(out survivingTeamIndex))
                        {
                            AnnounceWinningTeam(survivingTeamIndex);
                            ChangeTeams(TeamIndex.Player);
                            UpdateCompanionTeams();
                            if(TeleporterInteraction.instance)
                            {
                                Type chargingState = typeof(TeleporterInteraction).GetNestedType("ChargingState", BindingFlags.NonPublic);
                                object chargingStateInstance = Activator.CreateInstance(chargingState);
                                TeleporterInteraction.instance.mainStateMachine.state.outer.SetNextState((EntityState)chargingStateInstance);
                                TeleporterInteraction.instance.holdoutZoneController.Network_charge = 0.98f;
                                TeleporterInteraction.instance.bonusDirector = null;
                                TeleporterInteraction.instance.bossDirector = null;
                            }
                            PVPMode.PVPEnded = true;
                        }
                    }
                }
            }
        }

        public static void Teleport(TeleporterInteraction self)
        {
            if (!PVPMode.UsedTeleporter)
            {
                if (PVPMode.PVPEnded)
                {
                    ApplyExp();
                    self.GetComponent<SceneExitController>().Begin();
                    PVPMode.UsedTeleporter = true;
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
        static void Countdown()
        {
            PVPMode.GraceTimer -= Time.fixedDeltaTime;

            //Send a 5 second warning to players before grace period runs out
            if (PVPMode.GraceTimer <= 5f)
            {
                if (PVPMode.GraceTimer <= PVPMode.CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(PVPMode.CurrentGraceTimeReminder.ToString(), new Color32(255, 106, 0, 255)) + " Seconds left of grace period!"
                    });
                    PVPMode.CurrentGraceTimeReminder -= 1f;
                }
            }
            else
            {
                //Reminds players of remaining time every 30 seconds
                if (PVPMode.GraceTimer <= PVPMode.CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(PVPMode.CurrentGraceTimeReminder.ToString(), new Color32(255, 216, 0, 255)) + " Seconds left of grace period!"
                    });
                    float nextTimeReminder = PVPMode.CurrentGraceTimeReminder - 30f;
                    if (nextTimeReminder <= 5f) PVPMode.CurrentGraceTimeReminder = 5f;
                    else PVPMode.CurrentGraceTimeReminder = nextTimeReminder;
                }
            }
        }

        static void FilterDCedPlayers(List<PlayerCharacterMasterController> listToFilter)
        {
            for (int i = listToFilter.Count - 1; i >= 0; i--)
            {
                if (listToFilter[i].master.IsDeadAndOutOfLivesServer()) listToFilter.RemoveAt(i);
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
                    Tools.SendPM(PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner, new Chat.SimpleChatMessage
                    {
                        baseToken = text
                    });
                }
            }
        }

        static void CashGrantTimerTick(uint cashAmount)
        {
            PVPMode.CashGrantTimer += Time.fixedDeltaTime;

            //Grant cash to each player
            if (PVPMode.CashGrantTimer >= Settings.CashDelay)
            {
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++) PlayerCharacterMasterController.instances[i].master.GiveMoney(cashAmount);
                PVPMode.CashGrantTimer -= Settings.CashDelay;
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
                if (companions[i].ownerMaster && companions[i].GetComponent<CharacterMaster>() != null)
                {
                    CharacterBody companion = companions[i].GetComponent<CharacterMaster>().GetBody();
                    CharacterBody player = companions[i].ownerMaster.GetBody();

                    companions[i].GetComponent<CharacterMaster>().teamIndex = companions[i].ownerMaster.teamIndex;
                    if (companion != null && player != null) companion.teamComponent.teamIndex = player.teamComponent.teamIndex;
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
            bool allDead = true;

            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                //GetBody() null on dc
                //Master still exists even after dc
                //Isalive is false when player dc or full dead
                if (!PlayerCharacterMasterController.instances[i].master.IsDeadAndOutOfLivesServer() && PlayerCharacterMasterController.instances[i].master.GetBody())
                {
                    if (survivingTeamIndex == TeamIndex.None) survivingTeamIndex = PlayerCharacterMasterController.instances[i].master.teamIndex;
                    else
                    {
                        if (survivingTeamIndex != PlayerCharacterMasterController.instances[i].master.teamIndex)
                        {
                            allDead = false;
                            break;
                        }
                    }
                }
            }
            teamIndexOutput = survivingTeamIndex;
            return allDead;
        }

        static void AnnounceWinningTeam(TeamIndex teamIndex)
        {
            string str = "";
            switch (teamIndex)
            {
                case TeamIndex.Player:
                    str = "Team 1";
                    break;
                case TeamIndex.Neutral:
                    str = "Team 2";
                    break;
                case TeamIndex.Monster:
                    str = "Team 3";
                    break;
                default:
                    str = "Team 4";
                    break;
            }
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(str, new Color32(76, byte.MaxValue, 0, byte.MaxValue)) + " has survived... But can they keep it up?"
            });
            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString("Head towards the teleporter for the next round.", new Color32(0, 255, 255, 255))
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

        //Shuffles the players
        static void Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random();
            int i = list.Count;
            while (i > 1)
            {
                int index = random.Next(i);
                i--;
                T value = list[index];
                list[index] = list[i];
                list[i] = value;
            }
        }
        #endregion
    }
}
