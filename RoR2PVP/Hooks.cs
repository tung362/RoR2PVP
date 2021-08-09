using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using BepInEx;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Networking;
using R2API;
using R2API.Utils;
using RoR2GameModeAPI;
using RoR2GameModeAPI.Utils;
using RoR2PVP.UI;
using RoR2PVP.GameModes;

namespace RoR2PVP
{
    /// <summary>
    /// Handles all mod core hooks
    /// </summary>
    public static class Hooks
    {
        /// <summary>
        /// Core hook inits. Set core hooks
        /// </summary>
        public static void Init()
        {
            //GameModeAPI hooks
            GameModeAPI.OnPreGameStart += OnPreGameStart;
            GameModeAPI.OnPostGameStart += OnPostGameStart;
            GameModeAPI.OnGameEnd += OnGameEnd;
            GameModeAPI.OnPlayerConnect += OnPlayerConnect;
            GameModeAPI.OnPlayerDisconnect += OnPlayerDisconnect;

            //Extra hooks
            On.RoR2.UI.UIJuice.Awake += ApplyCustomUI;
        }

        #region GameModeAPI Hooks
        /// <summary>
        /// GameModeAPI hook for right before game session starts, used for applying the vote results to their respective game modes
        /// <para>Invoked on both client and server</para>
        /// </summary>
        static void OnPreGameStart()
        {
            //Ensures this only runs on the server side
            if (NetworkServer.active)
            {
                //Check if the active game mode is a pvp game mode
                if(GameModeAPI.ActiveGameMode is PVPGameMode)
                {
                    PVPGameMode pvpGameMode = (PVPGameMode)GameModeAPI.ActiveGameMode;

                    //Get the poll results belonging to this mod (The results of the votes while in the lobby)
                    VoteAPI.VoteResult pollResults = VoteAPI.VoteResults[Settings.VotePollName];

                    //Game mode setup
                    pvpGameMode.LoadDestinations();

                    //Apply configured banned items to the game mode
                    if(pollResults.Vote.HasVote(Settings.BanItems.Item2))
                    {
                        pvpGameMode.BannedItems = new HashSet<ItemIndex>(Settings.BannedItems);
                        pvpGameMode.BannedEquipments = new HashSet<EquipmentIndex>(Settings.BannedEquipments);
                    }
                    else
                    {
                        pvpGameMode.BannedItems.Clear();
                        pvpGameMode.BannedEquipments.Clear();
                    }

                    //Apply the vote results to the base game mode settings
                    pvpGameMode.AllowVanillaSpawnMobs = pollResults.Vote.HasVote(Settings.MobSpawn.Item2);
                    pvpGameMode.AllowVanillaInteractableSpawns = !pollResults.Vote.HasVote(Settings.CustomInteractablesSpawner.Item2);
                    /*pvpGameMode.AllowVanillaTeleport;
                    pvpGameMode.AllowVanillaGameOver;*/

                    //Apply vote results to the base pvp game mode settings
                    pvpGameMode.CompanionsShareItems = pollResults.Vote.HasVote(Settings.CompanionsShareItems.Item2);
                    pvpGameMode.CustomPlayableCharacters = pollResults.Vote.HasVote(Settings.CustomPlayableCharacters.Item2);
                    pvpGameMode.UseDeathPlaneFailsafe = pollResults.Vote.HasVote(Settings.UseDeathPlaneFailsafe.Item2);
                    pvpGameMode.WiderStageTransitions = pollResults.Vote.HasVote(Settings.WiderStageTransitions.Item2);

                    //Check if the pvp game mode is a team pvp game mode
                    if (pvpGameMode is TeamGameMode)
                    {
                        TeamGameMode teamGameMode = (TeamGameMode)pvpGameMode;

                        //Apply vote results to the team pvp game mode settings
                        teamGameMode.RandomTeams = pollResults.Vote.HasVote(Settings.RandomTeams.Item2);
                    }
                    /*else if (pvpGameMode is FreeForAllGameMode)
                    {
                        FreeForAllGameMode freeForAllGameMode = (FreeForAllGameMode)pvpGameMode;

                        //Apply vote results to the free for all pvp game mode settings
                    }*/
                }
            }
        }

        /// <summary>
        /// GameModeAPI hook for right after game session starts, used for applying team loadouts
        /// <para>Invoked on both client and server</para>
        /// </summary>
        static void OnPostGameStart()
        {
            //Ensures this only runs on the server side
            if (NetworkServer.active)
            {
                //Check if the active game mode is a team pvp game mode
                if (GameModeAPI.ActiveGameMode is TeamGameMode)
                {
                    TeamGameMode teamGameMode = (TeamGameMode)GameModeAPI.ActiveGameMode;

                    //Apply the team loadout specified during lobby scene
                    teamGameMode.LoadFixedTeams();
                }
            }
        }

        /// <summary>
        /// GameModeAPI hook for right after game session ends
        /// <para>Invoked on both client and server</para>
        /// </summary>
        static void OnGameEnd()
        {

        }

        /// <summary>
        /// GameModeAPI hook for right after a player connects, used for private messaging connecting players about the custom characters and to add them to the team picker
        /// <para>Invoked on server only</para>
        /// </summary>
        static void OnPlayerConnect(NetworkUser user, NetworkConnection conn, short playerControllerId)
        {
            //Send pm involving information about the custom playable characters to the connecting player
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
                        slotName = "Bandit";
                        break;
                    case 3:
                        slotName = "MUL-T";
                        break;
                    case 4:
                        slotName = "Engineer";
                        break;
                    case 5:
                        slotName = "Artificer";
                        break;
                    case 6:
                        slotName = "Mercenary";
                        break;
                    case 7:
                        slotName = "REX";
                        break;
                    case 8:
                        slotName = "Loader";
                        break;
                    case 9:
                        slotName = "Acrid";
                        break;
                    case 10:
                        slotName = "Captain";
                        break;
                }
                text += Util.GenerateColoredString(slotName, new Color32(255, 255, 0, 255)) + " = " + BodyCatalog.GetBodyName(BodyCatalog.FindBodyIndex(Settings.PlayableCharactersList[i])) + " ";

                //Switch to a new line every 3rd entry
                if (i + 1 % 3 == 0 || i + 1 == Settings.PlayableCharactersList.Count)
                {
                    RoR2PVPUtils.SendPM(conn, new Chat.SimpleChatMessage
                    {
                        baseToken = text
                    });
                    text = "";
                }
            }

            //Add the connecting player to the team picker menu
            if (user)
            {
                if (TeamPicker.instance) TeamPicker.instance.AddPlayer(user);
                else
                {
                    if (!TeamPicker.PlayerStates.ContainsKey(user)) TeamPicker.PlayerStates.Add(user, new TeamPicker.Slot(TeamPicker.StateType.Unassigned, -1));
                }
            }
        }

        /// <summary>
        /// GameModeAPI hook for right after a player disconnects, used for removing them from the team picker
        /// <para>Invoked on server only</para>
        /// </summary>
        static void OnPlayerDisconnect(NetworkUser user)
        {
            //Remove the disconnecting player from the team picker menu
            if (user)
            {
                if (TeamPicker.instance) TeamPicker.instance.RemovePlayer(user);
                else
                {
                    if (TeamPicker.PlayerStates.ContainsKey(user)) TeamPicker.PlayerStates.Remove(user);
                }
            }
        }
        #endregion

        #region Extra Hooks
        /// <summary>
        /// Hook for showing custom ui throughout each scene
        /// </summary>
        static void ApplyCustomUI(On.RoR2.UI.UIJuice.orig_Awake orig, RoR2.UI.UIJuice self)
        {
            //Show the pvp logo in the title scene
            if (self.name == "ImagePanel (JUICED)")
            {
                GameObject skull = GameObject.Instantiate((GameObject)Resources.Load("@TeamPVP:Assets/Resources/Prefabs/TeamPVPSkull.prefab"), self.transform);
                RectTransform skullTransform = skull.GetComponent<RectTransform>();
                skullTransform.anchoredPosition = new Vector2(90, -385);
            }

            //Ensures this only runs on the server side
            if (NetworkServer.active)
            {
                //Show the team picker, item banner, and character picker menus in the lobby scene
                if (self.name == "RightHandPanel")
                {
                    GameObject pvpMenu = GameObject.Instantiate((GameObject)Resources.Load("@TeamPVP:Assets/Resources/Prefabs/PVPMenu.prefab"), self.transform);
                    pvpMenu.AddComponent<TeamPicker>();
                    pvpMenu.AddComponent<ItemBanner>();
                    pvpMenu.AddComponent<CharacterPicker>();
                    RectTransform pvpMenuTransform = pvpMenu.GetComponent<RectTransform>();
                    pvpMenuTransform.anchoredPosition = new Vector2(-312, -64);
                }
            }
            orig(self);
        }
        #endregion
    }
}
