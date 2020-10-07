using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Networking;
using EntityStates;
using APIExtension.VoteAPI;

namespace RoR2PVP.UI
{
    public class TeamPickerSlot : MonoBehaviour
    {
        public NetworkUser Slot;

        /*Binds*/
        public TextMeshProUGUI PlayerNameText;
        public Button LeftButton;
        public Button RightButton;
        public RectTransform UITransform;

        /*Cache*/
        private TeamPicker.StateType State = TeamPicker.StateType.Unassigned;

        void Start()
        {
            //Bind
            PlayerNameText = transform.Find("Player Name Mask/Player Name Text").GetComponent<TextMeshProUGUI>();
            LeftButton = transform.Find("Left Button").GetComponent<Button>();
            RightButton = transform.Find("Right Button").GetComponent<Button>();
            UITransform = GetComponent<RectTransform>();

            if (Slot)
            {
                PlayerNameText.text = Slot.GetNetworkPlayerName().GetResolvedName();

                //Set listeners
                LeftButton.onClick.AddListener(MoveLeft);
                RightButton.onClick.AddListener(MoveRight);
            }
            else Debug.LogWarning("Warning! Player slot was created but slot is null @RoR2PVP");
        }

        #region Listeners
        public void MoveLeft()
        {
            switch(State)
            {
                case TeamPicker.StateType.Unassigned:
                    UITransform.anchoredPosition = new Vector2(-175.0f, UITransform.anchoredPosition.y);
                    State = TeamPicker.StateType.Team1;
                    LeftButton.gameObject.SetActive(false);
                    RightButton.gameObject.SetActive(true);
                    break;
                case TeamPicker.StateType.Team2:
                    UITransform.anchoredPosition = new Vector2(0.0f, UITransform.anchoredPosition.y);
                    State = TeamPicker.StateType.Unassigned;
                    LeftButton.gameObject.SetActive(true);
                    RightButton.gameObject.SetActive(true);
                    break;
            }
            TeamPicker.PlayerStates[Slot].State = State;
            AnnounceMove(Slot, TeamPicker.PlayerStates[Slot].State);
        }

        public void MoveRight()
        {
            switch (State)
            {
                case TeamPicker.StateType.Team1:
                    UITransform.anchoredPosition = new Vector2(0.0f, UITransform.anchoredPosition.y);
                    State = TeamPicker.StateType.Unassigned;
                    LeftButton.gameObject.SetActive(true);
                    RightButton.gameObject.SetActive(true);
                    break;
                case TeamPicker.StateType.Unassigned:
                    UITransform.anchoredPosition = new Vector2(175.0f, UITransform.anchoredPosition.y);
                    State = TeamPicker.StateType.Team2;
                    RightButton.gameObject.SetActive(false);
                    LeftButton.gameObject.SetActive(true);
                    break;
            }
            TeamPicker.PlayerStates[Slot].State = State;
            AnnounceMove(Slot, TeamPicker.PlayerStates[Slot].State);
        }
        #endregion

        #region Utils
        void AnnounceMove(NetworkUser user, TeamPicker.StateType state)
        {
            string teamText = "Unassigned";
            Color32 teamColor = new Color32(209, 209, 209, 255);
            switch(state)
            {
                case TeamPicker.StateType.Team1:
                    teamText = "Team 1";
                    teamColor = new Color32(57, 255, 58, 255);
                    break;
                case TeamPicker.StateType.Team2:
                    teamText = "Team 2";
                    teamColor = new Color32(255, 75, 57, 255);
                    break;
                default:
                    break;
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage
            {
                baseToken = Util.GenerateColoredString(user.GetNetworkPlayerName().GetResolvedName(), new Color32(146, 222, 255, 255)) + " is moved to " + Util.GenerateColoredString(teamText, teamColor)
            });
        }
        #endregion
    }
}
