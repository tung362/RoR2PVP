using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System;
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

namespace RoR2PVP.UI
{
    /// <summary>
    /// Lobby ui menu for picking custom playable characters
    /// </summary>
    public class CharacterPicker : MonoBehaviour
    {
        public static CharacterPicker instance { get; private set; }

        /*Enums*/
        public enum SlotType { Slot0, Slot1, Slot2, Slot3 }

        /*Callbacks*/
        public event Action OnLoad;

        /*Configuration*/
        public float offset = 40.0f;

        /*Binds*/
        public RectTransform Content;
        public RectTransform CharacterTemplate;
        public TMP_Dropdown LoadDropdown;
        public Button LoadButton;
        public TMP_Dropdown SaveDropdown;
        public Button SaveButton;

        /*Cache*/
        public SlotType LoadSlot = SlotType.Slot0;
        public SlotType SaveSlot = SlotType.Slot0;
        private List<RectTransform> Slots = new List<RectTransform>();

        /// <summary>
        /// Finds ui components and set listeners
        /// </summary>
        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.LogWarning("Warning! Multiple instances of \"CharacterPicker\" @RoR2PVP");

            //Bind
            Content = transform.Find("CustomPlayableCharactersMenu/ListSection/Template/Viewport/Content").GetComponent<RectTransform>();
            CharacterTemplate = transform.Find("CustomPlayableCharactersMenu/ListSection/Template/Viewport/Content/Character Template").GetComponent<RectTransform>();
            LoadDropdown = transform.Find("CustomPlayableCharactersMenu/BottomSection/Load/Load Dropdown").GetComponent<TMP_Dropdown>();
            LoadButton = transform.Find("CustomPlayableCharactersMenu/BottomSection/Load/Load Button").GetComponent<Button>();
            SaveDropdown = transform.Find("CustomPlayableCharactersMenu/BottomSection/Save/Save Dropdown").GetComponent<TMP_Dropdown>();
            SaveButton = transform.Find("CustomPlayableCharactersMenu/BottomSection/Save/Save Button").GetComponent<Button>();

            //Set listeners
            LoadDropdown.onValueChanged.AddListener(SetLoadSlot);
            LoadButton.onClick.AddListener(Load);
            SaveDropdown.onValueChanged.AddListener(SetSaveSlot);
            SaveButton.onClick.AddListener(Save);
        }

        /// <summary>
        /// Fills menu with character slots
        /// </summary>
        void Start()
        {
            for (int i = 0; i < Settings.PlayableCharactersList.Count; i++)
            {
                string slotName = "";
                string slotBodyName = "";
                switch (i)
                {
                    case 0:
                        slotName = "Commando Slot";
                        slotBodyName = "CommandoBody";
                        break;
                    case 1:
                        slotName = "Huntress Slot";
                        slotBodyName = "HuntressBody";
                        break;
                    case 2:
                        slotName = "Bandit Slot";
                        slotBodyName = "Bandit2Body";
                        break;
                    case 3:
                        slotName = "MUL-T Slot";
                        slotBodyName = "ToolbotBody";
                        break;
                    case 4:
                        slotName = "Engineer Slot";
                        slotBodyName = "EngiBody";
                        break;
                    case 5:
                        slotName = "Artificer Slot";
                        slotBodyName = "MageBody";
                        break;
                    case 6:
                        slotName = "Mercenary Slot";
                        slotBodyName = "MercBody";
                        break;
                    case 7:
                        slotName = "REX Slot";
                        slotBodyName = "TreebotBody";
                        break;
                    case 8:
                        slotName = "Loader Slot";
                        slotBodyName = "LoaderBody";
                        break;
                    case 9:
                        slotName = "Acrid Slot";
                        slotBodyName = "CrocoBody";
                        break;
                    case 10:
                        slotName = "Captain Slot";
                        slotBodyName = "CaptainBody";
                        break;
                    default:
                        slotName = "Dummy Slot";
                        slotBodyName = "BanditBody";
                        break;
                }

                CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndex(slotBodyName));
                RectTransform character = Instantiate(CharacterTemplate, Content.transform);
                character.gameObject.SetActive(true);
                character.anchoredPosition = new Vector2(0, -offset * Slots.Count);
                CharacterPickerSlot characterSlot = character.gameObject.AddComponent<CharacterPickerSlot>();
                characterSlot.Slot = i;
                characterSlot.Picker = this;
                characterSlot.CharacterSlotName = slotName;
                characterSlot.CharacterSlotTexture = body.portraitIcon;
                Slots.Add(character);
            }
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, offset * Slots.Count);
        }

        #region Listeners
        /// <summary>
        /// Sets the preset load slot
        /// </summary>
        /// <param name="num">Slot number</param>
        public void SetLoadSlot(int num)
        {
            LoadSlot = (SlotType)num;
        }

        /// <summary>
        /// Loads a character picker preset from the set load slot
        /// </summary>
        public void Load()
        {
            string path = $"{Settings.ConfigRootPath}PVPCharacterPicker{LoadSlot}.preset";

            //If preset exist
            if (File.Exists(path))
            {
                Settings.PlayableCharactersList.Clear();
                //Load config
                string[] presetLines = File.ReadAllLines(path);
                for (int i = 0; i < presetLines.Length; i++)
                {
                    //If not empty
                    if (presetLines[i].Length != 0)
                    {
                        //If not a comment
                        if (presetLines[i][0] != '#') Settings.PlayableCharactersList.Add(presetLines[i]);
                    }
                }
            }
            OnLoad?.Invoke();
        }

        /// <summary>
        /// Sets the preset save slot
        /// </summary>
        /// <param name="num">Slot number</param>
        public void SetSaveSlot(int num)
        {
            SaveSlot = (SlotType)num;
        }

        /// <summary>
        /// Saves a character picker preset from the set save slot
        /// </summary>
        public void Save()
        {
            List<string> config = new List<string>();

            //Add custom playable characters
            for(int i = 0; i < Settings.PlayableCharactersList.Count; i++)
            {
                config.Add(Settings.PlayableCharactersList[i].ToString());
            }

            //Save to preset file
            File.WriteAllLines($"{Settings.ConfigRootPath}PVPCharacterPicker{SaveSlot}.preset", config);
        }
        #endregion
    }
}
