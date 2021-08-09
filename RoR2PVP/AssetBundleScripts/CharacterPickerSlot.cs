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

namespace RoR2PVP.UI
{
    /// <summary>
    /// UI slot used by the character picker menu
    /// </summary>
    public class CharacterPickerSlot : MonoBehaviour
    {
        public int Slot;
        public string CharacterSlotName;
        public Texture CharacterSlotTexture;

        /*Binds*/
        public CharacterPicker Picker;
        public TextMeshProUGUI CharacterSlotNameText;
        public Image CharacterSlotIcon;
        public TMP_Dropdown CharacterDropdown;
        public Image CharacterIcon;

        /*Cache*/
        Dictionary<BodyIndex, int> DropDownMap = new Dictionary<BodyIndex, int>();

        /// <summary>
        /// Finds ui components, set listeners, loads drop down options, and updates slot
        /// </summary>
        void Start()
        {
            //Bind
            CharacterSlotNameText = transform.Find("Character Slot Name Mask/Character Slot Name Text").GetComponent<TextMeshProUGUI>();
            CharacterSlotIcon = transform.Find("Character Slot Icon").GetComponent<Image>();
            CharacterDropdown = transform.Find("Character Dropdown").GetComponent<TMP_Dropdown>();
            CharacterIcon = transform.Find("Character Icon").GetComponent<Image>();

            //Load dropdown options
            List<CharacterBody> bodies = BodyCatalog.allBodyPrefabBodyBodyComponents.ToList();
            for (int i = 0; i < bodies.Count; i++)
            {
                CharacterDropdown.options.Add(new TMP_Dropdown.OptionData(bodies[i].name));

                if (!DropDownMap.ContainsKey(bodies[i].bodyIndex)) DropDownMap.Add(bodies[i].bodyIndex, i);
                else Debug.LogWarning($"Warning! Duplicate custom character dropdown body index \"{bodies[i].bodyIndex}\" @RoR2PVP");
            }

            //Set listeners
            Picker.OnLoad += OnLoad;
            CharacterDropdown.onValueChanged.AddListener(SetCharacter);

            UpdateCharacterSlot();
            OnLoad();
        }

        #region Listeners
        /// <summary>
        /// Character picker load preset slot listener
        /// </summary>
        void OnLoad()
        {
            BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(Settings.PlayableCharactersList[Slot]);

            if (bodyIndex != BodyIndex.None) CharacterDropdown.value = DropDownMap[bodyIndex];
            else
            {
                SetCharacter(0);
                Debug.LogWarning($"Warning! Custom character load could not find index for body of \"{Settings.PlayableCharactersList[Slot]}\", using default @RoR2PVP");
            }
        }

        /// <summary>
        /// Picks the custom character this character slot should spawn as, and updates the slot to represent the picked custom character
        /// </summary>
        /// <param name="num">Dropdown option value</param>
        void SetCharacter(int num)
        {
            CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndex(CharacterDropdown.options[num].text));
            Settings.PlayableCharactersList[Slot] = body.name;
            CharacterIcon.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0.0f, 0.0f, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
        #endregion

        #region Utils
        /// <summary>
        /// Update slot's text and sprite to match the custom character it's representing
        /// </summary>
        public void UpdateCharacterSlot()
        {
            CharacterSlotNameText.text = CharacterSlotName;
            CharacterSlotIcon.sprite = Sprite.Create((Texture2D)CharacterSlotTexture, new Rect(0.0f, 0.0f, CharacterSlotTexture.width, CharacterSlotTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        #endregion
    }
}
