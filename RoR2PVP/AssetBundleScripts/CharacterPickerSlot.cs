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

        void Start()
        {
            //Bind
            CharacterSlotNameText = transform.Find("Character Slot Name Mask/Character Slot Name Text").GetComponent<TextMeshProUGUI>();
            CharacterSlotIcon = transform.Find("Character Slot Icon").GetComponent<Image>();
            CharacterDropdown = transform.Find("Character Dropdown").GetComponent<TMP_Dropdown>();
            CharacterIcon = transform.Find("Character Icon").GetComponent<Image>();

            //Load dropdown options
            for (int i = 0; i < BodyCatalog.bodyCount; i++) CharacterDropdown.options.Add(new TMP_Dropdown.OptionData(BodyCatalog.GetBodyPrefabBodyComponent(i).name));

            //Set listeners
            Picker.OnLoad += OnLoad;
            CharacterDropdown.onValueChanged.AddListener(SetCharacter);

            UpdateCharacterSlot();
            OnLoad();
        }

        #region Listeners
        void OnLoad()
        {
            CharacterDropdown.value = BodyCatalog.FindBodyIndex(Settings.PlayableCharactersList[Slot]);
        }

        void SetCharacter(int num)
        {
            CharacterBody body = BodyCatalog.GetBodyPrefabBodyComponent(num);
            Settings.PlayableCharactersList[Slot] = body.name;
            CharacterIcon.sprite = Sprite.Create((Texture2D)body.portraitIcon, new Rect(0.0f, 0.0f, body.portraitIcon.width, body.portraitIcon.height), new Vector2(0.5f, 0.5f), 100.0f);

        }
        #endregion

        #region Utils
        public void UpdateCharacterSlot()
        {
            CharacterSlotNameText.text = CharacterSlotName;
            CharacterSlotIcon.sprite = Sprite.Create((Texture2D)CharacterSlotTexture, new Rect(0.0f, 0.0f, CharacterSlotTexture.width, CharacterSlotTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
        #endregion
    }
}
