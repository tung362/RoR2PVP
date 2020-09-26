using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Networking;
using RoR2.UI;

namespace RoR2PVP
{
    class Tools
    {
        public static void CustomGenerate(string prefabPath, int amountToTrySpawn, int Price, Xoroshiro128Plus rng)
        {
            for (int i = 0; i < amountToTrySpawn; i++)
            {
                //Amount of attempts to try spawning this prefab before moving on
                int tries = 0;
                while (tries < 10)
                {
                    DirectorPlacementRule placementRule = new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.Random
                    };
                    //Spawn
                    GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest((InteractableSpawnCard)Resources.Load(prefabPath), placementRule, rng));

                    if (spawnedObject)
                    {
                        //Find PurchaseInteraction
                        PurchaseInteraction purchaseInteraction = spawnedObject.GetComponent<PurchaseInteraction>();
                        if (purchaseInteraction)
                        {
                            if (purchaseInteraction.costType == CostTypeIndex.Money)
                            {
                                //Apply unscaled cost
                                purchaseInteraction.Networkcost = Price == -1 ? purchaseInteraction.cost : Price;
                                break;
                            }
                        }

                        break;
                    }
                    else tries++;
                }
            }
        }

        public static GameObject CustomGenerate(string prefabPath, int Price, Xoroshiro128Plus rng)
        {
            //Amount of attempts to try spawning this prefab before moving on
            int tries = 0;
            while (tries < 10)
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Random
                };
                //Spawn
                GameObject spawnedObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest((InteractableSpawnCard)Resources.Load(prefabPath), placementRule, rng));

                if (spawnedObject)
                {
                    //Find PurchaseInteraction
                    PurchaseInteraction purchaseInteraction = spawnedObject.GetComponent<PurchaseInteraction>();
                    if (purchaseInteraction)
                    {
                        if (purchaseInteraction.costType == CostTypeIndex.Money)
                        {
                            //Apply unscaled cost
                            purchaseInteraction.Networkcost = Price == -1 ? purchaseInteraction.cost : Price;
                            break;
                        }
                    }

                    return spawnedObject;
                }
                else tries++;
            }
            return null;
        }

        public static void SendPM(NetworkConnection conn, Chat.ChatMessageBase message)
        {
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.StartMessage(59);
            networkWriter.Write(message.GetTypeIndex());
            networkWriter.Write(message);
            networkWriter.FinishMessage();
            conn.SendWriter(networkWriter, QosChannelIndex.chat.intVal);
        }

        public static bool TryGetStage(string stageName, out SceneDef stage)
        {
            stage = SceneCatalog.GetSceneDefFromSceneName(stageName);
            if (!stage)
            {
                Debug.LogWarning("Warning! Stage name: \"" + stageName + "\" does not exist, TeamPVP mod might be outdated!");
                return false;
            }
            return true;
        }

        public static void TryAddStage(string stageName, List<SceneDef> stages)
        {
            SceneDef stage = SceneCatalog.GetSceneDefFromSceneName(stageName);
            if (!stage)
            {
                Debug.LogWarning("Warning! Stage name: \"" + stageName + "\" does not exist, TeamPVP mod might be outdated!");
                return;
            }
            stages.Add(stage);
        }

        public static NetworkUser FindNetworkUser()
        {
            LocalUser localUser = ((MPEventSystem)EventSystem.current).localUser;
            if (localUser == null) return null;
            return localUser.currentNetworkUser;
        }

        //public static void SendCustomPing(NetworkConnection conn, NetworkIdentity networkIdentity, GameObject self, PingerController.PingInfo incomingPing)
        //{
        //    NetworkWriter networkWriter = new NetworkWriter();
        //    networkWriter.Write(0);
        //    networkWriter.Write((short)((ushort)5));
        //    networkWriter.WritePackedUInt32((uint)1170265357);
        //    networkWriter.Write(networkIdentity.netId);
        //    networkWriter.Write(incomingPing.active);
        //    networkWriter.Write(incomingPing.origin);
        //    networkWriter.Write(incomingPing.normal);
        //    networkWriter.Write(incomingPing.targetNetworkIdentity);
        //    networkWriter.FinishMessage();
        //    conn.SendWriter(networkWriter, QosChannelIndex.ping.intVal);
        //}
    }
}
