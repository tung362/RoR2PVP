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
    /// UI slot for items used by the item banner menu
    /// </summary>
    public class ItemBannerItemSlot : MonoBehaviour
    {
        public ItemIndex ItemSlot = ItemIndex.None;
        public EquipmentIndex EquipmentSlot = EquipmentIndex.None;
        public string ItemNameToken;
        public Color ItemColor;
        public Sprite ItemSprite;

        /*Binds*/
        public ItemBanner Banner;
        public TextMeshProUGUI ItemNameText;
        public Image ItemIcon;
        public Toggle ItemToggle;
        public Image ItemToggleHandle;

        /*Cache*/
        public static Sprite OnSprite = Resources.Load<Sprite>("@TeamPVP:Assets/Resources/UI/Menu/Icons/ToggleHandleOn.png");
        public static Sprite OffSprite = Resources.Load<Sprite>("@TeamPVP:Assets/Resources/UI/Menu/Icons/ToggleHandleOff.png");
        public static Color OnColor = new Color(1.0f, 0.4156863f, 0.0f, 1.0f);
        public static Color OffColor = Color.white;
        public static Color InteractableOffColor = new Color(0.4056604f, 0.4056604f, 0.4056604f, 1.0f);

        /// <summary>
        /// Finds ui components, set listeners, update slot
        /// </summary>
        void Start()
        {
            //Bind
            ItemNameText = transform.Find("Item Name Mask/Item Name Text").GetComponent<TextMeshProUGUI>();
            ItemIcon = transform.Find("Item Icon").GetComponent<Image>();
            ItemToggle = transform.Find("Item Toggle Mask/Item Toggle").GetComponent<Toggle>();
            ItemToggleHandle = transform.Find("Item Toggle Mask/Item Toggle/Item Handle").GetComponent<Image>();

            //Set listeners
            Banner.OnLoad += OnLoad;
            ItemToggle.onValueChanged.AddListener(ToggleBan);

            UpdateItem();
            OnLoad();

            //Invalid check
            //if (ItemNameToken == Language.GetString(ItemNameToken)) InteractableToggle(false);
        }

        #region Listeners
        /// <summary>
        /// Item banner load preset slot listener
        /// </summary>
        void OnLoad()
        {
            if (ItemSlot != ItemIndex.None)
            {
                if (Settings.BannedItems.Contains(ItemSlot))
                {
                    ItemToggle.isOn = true;
                    return;
                }
            }

            if (EquipmentSlot != EquipmentIndex.None)
            {
                if (Settings.BannedEquipments.Contains(EquipmentSlot))
                {
                    ItemToggle.isOn = true;
                    return;
                }
            }
            ItemToggle.isOn = false;
        }

        /// <summary>
        /// Ban/unban the item this slot represents
        /// </summary>
        /// <param name="toggle">Toggle ban</param>
        public void ToggleBan(bool toggle)
        {
            ItemToggleHandle.sprite = toggle ? OnSprite : OffSprite;
            ItemToggleHandle.color = toggle ? OnColor : OffColor;

            if(!ItemToggle.interactable) ItemToggleHandle.color = InteractableOffColor;

            //Item
            if (ItemSlot != ItemIndex.None)
            {
                //Ban
                if (toggle)
                {
                    if (!Settings.BannedItems.Contains(ItemSlot)) Settings.BannedItems.Add(ItemSlot);
                }
                //Unban
                else
                {
                    if (Settings.BannedItems.Contains(ItemSlot)) Settings.BannedItems.Remove(ItemSlot);
                }
            }

            //Equipment
            if (EquipmentSlot != EquipmentIndex.None)
            {
                //Ban
                if (toggle)
                {
                    if (!Settings.BannedEquipments.Contains(EquipmentSlot)) Settings.BannedEquipments.Add(EquipmentSlot);
                }
                //Unban
                else
                {
                    if (Settings.BannedEquipments.Contains(EquipmentSlot)) Settings.BannedEquipments.Remove(EquipmentSlot);
                }
            }
        }
        #endregion

        #region Utils
        /// <summary>
        /// Update slot's sprite and color to match the item it's representing
        /// </summary>
        public void UpdateItem()
        {
            ItemNameText.text = Language.GetString(ItemNameToken);
            ItemNameText.color = ItemColor;
            ItemIcon.sprite = ItemSprite;
        }

        /// <summary>
        /// Enable/disable interaction with the slot and grey it out if can't be interacted with
        /// </summary>
        /// <param name="toggle">Toggle interaction</param>
        public void InteractableToggle(bool toggle)
        {
            ItemToggle.interactable = toggle;
            if (toggle) ItemToggleHandle.color = ItemToggle.isOn ? OnColor : OffColor;
            else ItemToggleHandle.color = InteractableOffColor;
        }
        #endregion
    }
}
