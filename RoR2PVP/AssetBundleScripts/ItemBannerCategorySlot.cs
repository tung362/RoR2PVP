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
    public class ItemBannerCategorySlot : MonoBehaviour
    {
        public string CategoryName;
        public Color CategoryColor;

        /*Binds*/
        public TextMeshProUGUI CategoryNameText;

        void Start()
        {
            //Bind
            CategoryNameText = transform.Find("Category Name Text").GetComponent<TextMeshProUGUI>();

            UpdateCategory();
        }

        #region Utils
        public void UpdateCategory()
        {
            CategoryNameText.text = CategoryName;
            CategoryNameText.color = CategoryColor;
        }
        #endregion
    }
}
