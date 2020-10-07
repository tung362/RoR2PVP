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
    public class TeamPicker : MonoBehaviour
    {
        #region Format
        public class Slot
        {
            public StateType State;
            public int Index;

            public Slot(StateType state, int index)
            {
                State = state;
                Index = index;
            }
        }
        #endregion

        public static TeamPicker instance { get; private set; }

        /*Enums*/
        public enum StateType { Team1, Unassigned, Team2 }
        public enum UnassignType { LeastMembers, Team1, Team2 }

        /*Configuration*/
        public float offset = 30.0f;

        /*Binds*/
        public TMP_Dropdown UnassignDropdown;
        public RectTransform Content;
        public RectTransform PlayerTemplate;

        /*Cache*/
        public static UnassignType UnassignAction = UnassignType.LeastMembers;
        public static Dictionary<NetworkUser, Slot> PlayerStates = new Dictionary<NetworkUser, Slot>();
        private List<TeamPickerSlot> Slots = new List<TeamPickerSlot>();

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.LogWarning("Warning! Multiple instances of \"TeamPicker\" @RoR2PVP");

            //Bind
            UnassignDropdown = transform.Find("TeamPickerMenu/BottomSection/Unassign/Unassign Dropdown").GetComponent<TMP_Dropdown>();
            Content = transform.Find("TeamPickerMenu/TeamSection/Template/Viewport/Content").GetComponent<RectTransform>();
            PlayerTemplate = transform.Find("TeamPickerMenu/TeamSection/Template/Viewport/Content/Player Template").GetComponent<RectTransform>();

            //Set listeners
            UnassignDropdown.onValueChanged.AddListener(SetUnassignAction);

            //Reload
            List<NetworkUser> playerCache = new List<NetworkUser>();
            foreach(NetworkUser user in PlayerStates.Keys)
            {
                if (user) playerCache.Add(user);
            }
            UnassignAction = UnassignType.LeastMembers;
            PlayerStates.Clear();
            for (int i = 0; i < playerCache.Count; i++) AddPlayer(playerCache[i]);
        }

        #region Listeners
        public void SetUnassignAction(int num)
        {
            UnassignAction = (UnassignType)num;
        }
        #endregion

        #region Utils
        public void AddPlayer(NetworkUser user)
        {
            if(!PlayerStates.ContainsKey(user))
            {
                RectTransform playerSlot = Instantiate(PlayerTemplate, Content.transform);
                playerSlot.gameObject.SetActive(true);
                playerSlot.anchoredPosition = new Vector2(0, -offset * Slots.Count);
                Content.sizeDelta = new Vector2(Content.sizeDelta.x, offset * (Slots.Count + 1));
                TeamPickerSlot slot = playerSlot.gameObject.AddComponent<TeamPickerSlot>();
                slot.Slot = user;
                Slots.Add(slot);
                PlayerStates.Add(slot.Slot, new Slot(StateType.Unassigned, Slots.Count - 1));
            }
        }

        public void RemovePlayer(NetworkUser user)
        {
            if (PlayerStates.ContainsKey(user))
            {
                int startIndex = PlayerStates[user].Index;

                Destroy(Slots[startIndex].gameObject);
                Slots.RemoveAt(startIndex);
                PlayerStates.Remove(user);

                for (int i = startIndex; i < Slots.Count; i++)
                {
                    RectTransform slotTransform = Slots[i].GetComponent<RectTransform>();
                    slotTransform.anchoredPosition = new Vector2(slotTransform.anchoredPosition.x, slotTransform.anchoredPosition.y + offset);
                    PlayerStates[Slots[i].Slot].Index = i;
                }
                Content.sizeDelta = new Vector2(Content.sizeDelta.x, Content.sizeDelta.y - offset);
            }
        }
        #endregion
    }
}
