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
    /// <summary>
    /// Useful utilities methods for handling pvp functionalities
    /// </summary>
    public static class RoR2PVPUtils
    {
        /// <summary>
        /// Sends a private message to the targeted player
        /// </summary>
        /// <param name="conn">Targeted user's network connection</param>
        /// <param name="message">The message to send</param>
        public static void SendPM(NetworkConnection conn, ChatMessageBase message)
        {
            NetworkWriter networkWriter = new NetworkWriter();
            networkWriter.StartMessage(59);
            networkWriter.Write(message.GetTypeIndex());
            networkWriter.Write(message);
            networkWriter.FinishMessage();
            conn.SendWriter(networkWriter, QosChannelIndex.chat.intVal);
        }

        /// <summary>
        /// Finds the local client
        /// </summary>
        /// <returns></returns>
        public static NetworkUser FindNetworkUser()
        {
            LocalUser localUser = ((MPEventSystem)EventSystem.current).localUser;
            if (localUser == null) return null;
            return localUser.currentNetworkUser;
        }

        /// <summary>
        /// AI search filtering for minion groups extension
        /// </summary>
        /// <param name="search">Current search instance</param>
        /// <param name="netId">Entity's net id</param>
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

        /// <summary>
        /// Shuffles a generic list
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="list">List to shuffle</param>
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
