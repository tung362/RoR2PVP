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
using APIExtension.VoteAPI;

namespace RoR2PVP.UI
{
    public class ItemBanner : MonoBehaviour
    {
        #region Format
        public class ItemSorter : IComparer<ItemIndex>
        {
            public int Compare(ItemIndex x, ItemIndex y)
            {
                ItemDef itemDefx = ItemCatalog.GetItemDef(x);
                ItemDef itemDefy = ItemCatalog.GetItemDef(y);
                int result = itemDefx.tier.CompareTo(itemDefy.tier);
                if (result == 0) result = Language.GetString(itemDefx.nameToken).CompareTo(Language.GetString(itemDefy.nameToken));
                return result;
            }
        }
        public class EquipmentSorter : IComparer<EquipmentIndex>
        {
            public int Compare(EquipmentIndex x, EquipmentIndex y)
            {
                EquipmentDef equipmentDefx = EquipmentCatalog.GetEquipmentDef(x);
                EquipmentDef equipmentDefy = EquipmentCatalog.GetEquipmentDef(y);
                int result = equipmentDefx.colorIndex.CompareTo(equipmentDefy.colorIndex);
                if (result == 0) result = Language.GetString(equipmentDefx.nameToken).CompareTo(Language.GetString(equipmentDefy.nameToken));
                return result;
            }
        }
        #endregion

        public static ItemBanner instance { get; private set; }

        /*Enums*/
        public enum SlotType { Slot0, Slot1, Slot2, Slot3 }

        /*Callbacks*/
        public event Action OnLoad;

        /*Configuration*/
        public float offsetX = 346.0f;
        public float offsetY = 40.0f;

        /*Binds*/
        public RectTransform Content;
        public RectTransform CategoryTemplate;
        public RectTransform ItemTemplate;
        public TMP_Dropdown LoadDropdown;
        public Button LoadButton;
        public TMP_Dropdown SaveDropdown;
        public Button SaveButton;
        public Button ClearAllButton;

        /*Cache*/
        public SlotType LoadSlot = SlotType.Slot0;
        public SlotType SaveSlot = SlotType.Slot0;
        private List<RectTransform> Slots = new List<RectTransform>();

        void OnEnable()
        {
            if (!instance) instance = this;
            else Debug.LogWarning("Warning! Multiple instances of \"ItemBanner\" @RoR2PVP");

            //Bind
            Content = transform.Find("BanItemsMenu/ListSection/Template/Viewport/Content").GetComponent<RectTransform>();
            CategoryTemplate = transform.Find("BanItemsMenu/ListSection/Template/Viewport/Content/Category Template").GetComponent<RectTransform>();
            ItemTemplate = transform.Find("BanItemsMenu/ListSection/Template/Viewport/Content/Item Template").GetComponent<RectTransform>();
            LoadDropdown = transform.Find("BanItemsMenu/BottomSection/Load/Load Dropdown").GetComponent<TMP_Dropdown>();
            LoadButton = transform.Find("BanItemsMenu/BottomSection/Load/Load Button").GetComponent<Button>();
            SaveDropdown = transform.Find("BanItemsMenu/BottomSection/Save/Save Dropdown").GetComponent<TMP_Dropdown>();
            SaveButton = transform.Find("BanItemsMenu/BottomSection/Save/Save Button").GetComponent<Button>();
            ClearAllButton = transform.Find("BanItemsMenu/BottomSection/Clear All Button").GetComponent<Button>();

            //Set listeners
            LoadDropdown.onValueChanged.AddListener(SetLoadSlot);
            LoadButton.onClick.AddListener(Load);
            SaveDropdown.onValueChanged.AddListener(SetSaveSlot);
            SaveButton.onClick.AddListener(Save);
            ClearAllButton.onClick.AddListener(ClearAll);
        }

        void Start()
        {
            float splitY = 0;
            //Items category
            RectTransform itemCategory = Instantiate(CategoryTemplate, Content.transform);
            itemCategory.gameObject.SetActive(true);
            itemCategory.anchoredPosition = new Vector2(0, -offsetY * Mathf.FloorToInt(splitY / 2));
            ItemBannerCategorySlot itemCategorySlot = itemCategory.gameObject.AddComponent<ItemBannerCategorySlot>();
            itemCategorySlot.CategoryName = "Items";
            itemCategorySlot.CategoryColor = new Color(0.4481132f, 1.0f, 0.9442427f, 1.0f);
            Slots.Add(itemCategory);
            splitY += 2;

            //Load items
            List<ItemIndex> items = ItemCatalog.allItems.ToList();
            items.Sort(new ItemSorter());
            for (int i = 0; i < items.Count; i++)
            {
                float splitX = offsetX;
                if (i % 2 == 0) splitX = 0;
                ItemDef itemDef = ItemCatalog.GetItemDef(items[i]);
                RectTransform item = Instantiate(ItemTemplate, Content.transform);
                item.gameObject.SetActive(true);
                item.anchoredPosition = new Vector2(splitX, -offsetY * Mathf.FloorToInt(splitY / 2));
                ItemBannerItemSlot itemSlot = item.gameObject.AddComponent<ItemBannerItemSlot>();
                itemSlot.ItemSlot = items[i];
                itemSlot.Banner = this;
                itemSlot.ItemNameToken = itemDef.nameToken;
                itemSlot.ItemColor = ColorCatalog.GetColor(itemDef.colorIndex);
                itemSlot.ItemSprite = itemDef.pickupIconSprite;
                Slots.Add(item);
                splitY++;
                if(i >= items.Count - 1)
                {
                    if (splitY % 2 != 0) splitY++;
                }
            }

            //Equipments category
            RectTransform equipmentCategory = Instantiate(CategoryTemplate, Content.transform);
            equipmentCategory.gameObject.SetActive(true);
            equipmentCategory.anchoredPosition = new Vector2(0, -offsetY * Mathf.FloorToInt(splitY / 2));
            ItemBannerCategorySlot equipmentCategorySlot = equipmentCategory.gameObject.AddComponent<ItemBannerCategorySlot>();
            equipmentCategorySlot.CategoryName = "Equipment";
            equipmentCategorySlot.CategoryColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Equipment);
            Slots.Add(equipmentCategory);
            splitY += 2;

            //Load equipments
            List<EquipmentIndex> equipments = EquipmentCatalog.allEquipment.ToList();
            equipments.Sort(new EquipmentSorter());
            for (int i = 0; i < equipments.Count; i++)
            {
                float splitX = offsetX;
                if (i % 2 == 0) splitX = 0;
                EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef(equipments[i]);
                RectTransform item = Instantiate(ItemTemplate, Content.transform);
                item.gameObject.SetActive(true);
                item.anchoredPosition = new Vector2(splitX, -offsetY * Mathf.FloorToInt(splitY / 2));
                ItemBannerItemSlot itemSlot = item.gameObject.AddComponent<ItemBannerItemSlot>();
                itemSlot.EquipmentSlot = equipments[i];
                itemSlot.Banner = this;
                itemSlot.ItemNameToken = equipmentDef.nameToken;
                itemSlot.ItemColor = ColorCatalog.GetColor(equipmentDef.colorIndex);
                itemSlot.ItemSprite = equipmentDef.pickupIconSprite;
                Slots.Add(item);
                splitY++;
                if (i >= equipments.Count - 1)
                {
                    if (splitY % 2 != 0) splitY++;
                }
            }
            Content.sizeDelta = new Vector2(Content.sizeDelta.x, offsetY * Mathf.FloorToInt(splitY / 2));
        }

        #region Listeners
        public void SetLoadSlot(int num)
        {
            LoadSlot = (SlotType)num;
        }

        public void Load()
        {
            string path = Setup.ConfigRootPath + "PVPBanItem" + LoadSlot + ".preset";
            Settings.BannedItems.Clear();
            Settings.BannedEquipments.Clear();

            //If preset exist
            if (File.Exists(path))
            {
                //Load config
                string[] presetLines = File.ReadAllLines(path);
                for (int i = 0; i < presetLines.Length; i++)
                {
                    //If not empty
                    if (presetLines[i].Length != 0)
                    {
                        //If not a comment
                        if (presetLines[i][0] != '#')
                        {
                            if (Enum.TryParse(presetLines[i], out ItemIndex itemIndex))
                            {
                                if(!Settings.BannedItems.ContainsKey(itemIndex)) Settings.BannedItems.Add(itemIndex, itemIndex);
                            }
                            if (Enum.TryParse(presetLines[i], out EquipmentIndex equipmentIndex))
                            {
                                if (!Settings.BannedEquipments.ContainsKey(equipmentIndex)) Settings.BannedEquipments.Add(equipmentIndex, equipmentIndex);
                            }
                        }
                    }
                }
            }
            OnLoad?.Invoke();
        }

        public void SetSaveSlot(int num)
        {
            SaveSlot = (SlotType)num;
        }

        public void Save()
        {
            List<string> config = new List<string>();

            //Add banned items
            foreach (ItemIndex item in Settings.BannedItems.Values)
            {
                config.Add(item.ToString());
            }

            //Add banned equipment
            foreach (EquipmentIndex equipment in Settings.BannedEquipments.Values)
            {
                config.Add(equipment.ToString());
            }

            //Save to preset file
            File.WriteAllLines(Setup.ConfigRootPath + "PVPBanItem" + SaveSlot + ".preset", config);
        }

        public void ClearAll()
        {
            Settings.BannedItems.Clear();
            Settings.BannedEquipments.Clear();
            OnLoad?.Invoke();
        }
        #endregion
    }
}
