using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.UI;

namespace RoR2PVP
{
    class PVPMode
    {
        #region Core
        public static void Reset()
        {
            Settings.IsGracePeriod = true;
            Settings.GraceTimer = Settings.GraceTimerDuration;
            Settings.CashGrantTimer = 0;
            Settings.CurrentGraceTimeReminder = Settings.GraceTimer;
            Settings.PVPEnded = false;
            Settings.UsedTeleporter = false;
            Settings.PVPTeams.Clear();
        }

        public static void Tick()
        {
            if (NetworkServer.active)
            {
                //Gold shores as per usual but with the pvp mechanic in place
                if (SceneInfo.instance.sceneDef.sceneName == "goldshores")
                {
                    CashGrantTimer(1000u);
                    return;
                }

                //Safe zones
                if (SceneInfo.instance.sceneDef.sceneName == "bazaar" || SceneInfo.instance.sceneDef.sceneName == "mysteryspace") return;

                /*Grace period*/
                if (Settings.IsGracePeriod)
                {
                    /*Grace period countdown*/
                    if (Settings.GraceTimer >= 0f)
                    {
                        CashGrantTimer(Settings.CashGrantAmount);
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
                        if(Settings.RandomTeams) Shuffle<PlayerCharacterMasterController>(players);
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (players[i].master.alive && players[i].master.GetBody() != null)
                            {
                                //Split players into 2 teams
                                if (i % 2 == 0)
                                {
                                    players[i].master.teamIndex = TeamIndex.Player;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                                    Settings.PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Player));
                                }
                                else
                                {
                                    players[i].master.teamIndex = TeamIndex.Neutral;
                                    players[i].master.GetBody().teamComponent.teamIndex = TeamIndex.Neutral;
                                    Settings.PVPTeams.Add(new PVPTeamTrackerStruct(players[i].GetDisplayName(), TeamIndex.Neutral));
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

                        Settings.IsGracePeriod = false;
                    }
                }
                else
                {
                    /*PVP period*/
                    if (!Settings.PVPEnded)
                    {
                        TeamIndex survivingTeamIndex;

                        //Check for surviving teams
                        if (DeathCheck(out survivingTeamIndex))
                        {
                            AnnounceWinningTeam(survivingTeamIndex);
                            ChangeTeams(TeamIndex.Player);
                            UpdateCompanionTeams();
                            TeleporterInteraction.instance.NetworkactivationStateInternal = 2u;
                            TeleporterInteraction.instance.remainingChargeTimer = 1;
                            TeleporterInteraction.instance.bonusDirector = null;
                            TeleporterInteraction.instance.bossDirector = null;
                            //PingTeleporter();
                            Settings.PVPEnded = true;
                        }
                    }
                }
            }
        }

        public static void Teleport(TeleporterInteraction self)
        {
            if (!Settings.UsedTeleporter)
            {
                if (Settings.PVPEnded)
                {
                    ApplyExp();
                    self.GetComponent<SceneExitController>().Begin();
                    Settings.UsedTeleporter = true;
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
            Settings.GraceTimer -= Time.fixedDeltaTime;

            //Send a 5 second warning to players before grace period runs out
            if (Settings.GraceTimer <= 5f)
            {
                if (Settings.GraceTimer <= Settings.CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(Settings.CurrentGraceTimeReminder.ToString(), new Color32(255, 106, 0, 255)) + " Seconds left of grace period!"
                    });
                    Settings.CurrentGraceTimeReminder -= 1f;
                }
            }
            else
            {
                //Reminds players of remaining time every 30 seconds
                if (Settings.GraceTimer <= Settings.CurrentGraceTimeReminder)
                {
                    Chat.SendBroadcastChat(new Chat.SimpleChatMessage
                    {
                        baseToken = Util.GenerateColoredString(Settings.CurrentGraceTimeReminder.ToString(), new Color32(255, 216, 0, 255)) + " Seconds left of grace period!"
                    });
                    float nextTimeReminder = Settings.CurrentGraceTimeReminder - 30f;
                    if (nextTimeReminder <= 5f) Settings.CurrentGraceTimeReminder = 5f;
                    else Settings.CurrentGraceTimeReminder = nextTimeReminder;
                }
            }
        }

        static void FilterDCedPlayers(List<PlayerCharacterMasterController> listToFilter)
        {
            for (int i = listToFilter.Count - 1; i >= 0; i--)
            {
                if (!listToFilter[i].master.alive) listToFilter.RemoveAt(i);
            }
        }

        static void PMAboutAllies()
        {
            //PM ally list
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                if (PlayerCharacterMasterController.instances[i].master.alive && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
                {
                    string text = Util.GenerateColoredString("Your allies: ", new Color32(0, 255, 255, 255));
                    for (int j = 0; j < PlayerCharacterMasterController.instances.Count; j++)
                    {
                        if (PlayerCharacterMasterController.instances[j].master.alive && PlayerCharacterMasterController.instances[j].master.GetBody() != null)
                        {
                            if (PlayerCharacterMasterController.instances[i].master != PlayerCharacterMasterController.instances[j].master &&
                                PlayerCharacterMasterController.instances[i].master.GetBody().teamComponent.teamIndex == PlayerCharacterMasterController.instances[j].master.GetBody().teamComponent.teamIndex)
                            {
                                text += Util.GenerateColoredString(PlayerCharacterMasterController.instances[j].GetDisplayName(), new Color32(0, 255, 0, 255)) + ", ";
                            }
                        }
                    }
                    if (text == Util.GenerateColoredString("Your allies: ", new Color32(0, 255, 255, 255))) text += "None";
                    Hooks.SendPM(PlayerCharacterMasterController.instances[i].master.networkIdentity.clientAuthorityOwner, new Chat.SimpleChatMessage
                    {
                        baseToken = text
                    });
                }
            }
        }

        static void CashGrantTimer(uint cashAmount)
        {
            Settings.CashGrantTimer += Time.fixedDeltaTime;

            //Grant cash to each player
            if (Settings.CashGrantTimer >= Settings.CashDelay)
            {
                for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++) PlayerCharacterMasterController.instances[i].master.GiveMoney(cashAmount);
                Settings.CashGrantTimer -= Settings.CashDelay;
            }
        }

        static void ChangeTeams(TeamIndex teamIndex)
        {
            for (int i = 0; i < PlayerCharacterMasterController.instances.Count; i++)
            {
                PlayerCharacterMasterController.instances[i].master.teamIndex = teamIndex;
                if (PlayerCharacterMasterController.instances[i].master.alive && PlayerCharacterMasterController.instances[i].master.GetBody() != null)
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

                    if (companion != null && player != null) companion.teamComponent.teamIndex = player.teamComponent.teamIndex;
                }
            }
        }

        static void DestroyAllPurchaseables()
        {
            PurchaseInteraction[] array2 = GameObject.FindObjectsOfType<PurchaseInteraction>();
            for (int n = 0; n < array2.Length; n++)
            {
                NetworkServer.Destroy(array2[n].gameObject);
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
                if (PlayerCharacterMasterController.instances[i].master.alive && PlayerCharacterMasterController.instances[i].master.GetBody())
                {
                    if (survivingTeamIndex == TeamIndex.None) survivingTeamIndex = PlayerCharacterMasterController.instances[i].master.GetBody().teamComponent.teamIndex;
                    else
                    {
                        if (survivingTeamIndex != PlayerCharacterMasterController.instances[i].master.GetBody().teamComponent.teamIndex)
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
