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
using RoR2.CharacterAI;

namespace RoR2PVP
{
    public static class Tools
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

        public static void SendPM(NetworkConnection conn, ChatMessageBase message)
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

        public static ItemIndex TryGetRandomItem(List<PickupIndex> list, Xoroshiro128Plus rng)
        {
            if(list.Count != 0)
            {
                int tries = 0;
                while (tries < 10)
                {
                    ItemIndex index = PickupCatalog.GetPickupDef(list[rng.RangeInt(0, list.Count)]).itemIndex;
                    if (index != ItemIndex.None) return index;
                    else tries++;
                }
            }
            return ItemIndex.None;
        }

        public static EquipmentIndex TryGetRandomEquipment(List<PickupIndex> list, Xoroshiro128Plus rng)
        {
            if(list.Count != 0)
            {
                int tries = 0;
                while (tries < 10)
                {
                    EquipmentIndex index = PickupCatalog.GetPickupDef(list[rng.RangeInt(0, list.Count)]).equipmentIndex;
                    if (index != EquipmentIndex.None) return index;
                    else tries++;
                }
            }
            return EquipmentIndex.None;
        }

        public static void FilterOutMinionGroup(this BullseyeSearch search, NetworkInstanceId netId)
        {
            List<MinionOwnership.MinionGroup> minionGroups = (List<MinionOwnership.MinionGroup>)typeof(MinionOwnership.MinionGroup).GetField("instancesList", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            for (int i = 0; i < minionGroups.Count; i++)
            {
                if (netId == minionGroups[i].ownerId)
                {
                    MinionOwnership[] members = minionGroups[i].GetFieldValue<MinionOwnership[]>("members");
                    for (int j = 0; j < minionGroups[i].memberCount; j++)
                    {
                        CharacterBody minionBody = members[j].GetComponent<CharacterMaster>().GetBody();
                        if (minionBody) search.FilterOutGameObject(minionBody.gameObject);
                    }
                    break;
                }
            }
        }

        //Shuffles a list
        public static void Shuffle<T>(List<T> list)
        {
            System.Random random = new System.Random();
            int i = list.Count;
            while (i > 1)
            {
                int index = random.Next(i);
                i--;
                T value = list[index];
                list[index] = list[i];
                list[i] = value;
            }
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
