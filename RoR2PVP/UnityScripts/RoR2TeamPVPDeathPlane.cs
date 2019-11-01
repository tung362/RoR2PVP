using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.CharacterAI;
using RoR2.UI;
using EntityStates;
using Facepunch.Steamworks;

namespace RoR2PVP.UnityScripts
{
    public class RoR2TeamPVPDeathPlane : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (NetworkServer.active)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body) body.healthComponent.Suicide();
            }
        }
    }
}
