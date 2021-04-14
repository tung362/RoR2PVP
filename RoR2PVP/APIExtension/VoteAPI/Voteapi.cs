using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using R2API.Utils;
using RoR2;

namespace APIExtension.VoteAPI
{
    /// <summary>
    /// Unofficial api implementing a voting system to multiplayer and singleplayer lobbies created for RoR2PVP
    /// Works regardless of other players having vanilla or modded clients over the network with no desync errors
    /// Send me a message if you'd like a public version for your mod on https://github.com/tung362
    /// Does not yet fully support other mods
    /// </summary>
    public static class VoteAPI
    {
        /*Result*/
        public static VoteMask VoteResults;

        /*Pending vote elements to be registered*/
        public static readonly List<RuleCategoryDef> VoteHeadersTop = new List<RuleCategoryDef>();
        public static readonly List<RuleCategoryDef> VoteHeadersBottom = new List<RuleCategoryDef>();
        public static readonly List<RuleDef> VoteSelections = new List<RuleDef>();

        /*Registered vote elements*/
        private static readonly List<RuleCategoryDef> RegisteredVoteHeaders = new List<RuleCategoryDef>();
        private static readonly List<RuleDef> RegisteredVoteSelections = new List<RuleDef>();
        private static readonly List<RuleChoiceDef> RegisteredVoteChoices = new List<RuleChoiceDef>();

        /*Network*/
        private static List<byte> RuleBookExtraBytes = new List<byte>();
        private static List<byte> RuleMaskExtraBytes = new List<byte>();
        private static List<byte> RuleChoiceMaskExtraBytes = new List<byte>();
        private static RuleBook TargetedRuleBook;
        private static RuleMask TargetedRuleMask;
        private static RuleChoiceMask TargetedRuleChoiceMask;

        public static void SetHook()
        {
            On.RoR2.RuleCatalog.Init += RegisterVotes;
            On.RoR2.Run.SetRuleBook += ApplyVotes;
            On.RoR2.VoteController.OnSerialize += VoteControllerSerialize;
            On.RoR2.VoteController.OnDeserialize += VoteControllerDeserialize;
            On.RoR2.PreGameRuleVoteController.WriteVotes += PreGameRuleVoteControllerSerialize;
            On.RoR2.PreGameRuleVoteController.ReadVotes += PreGameRuleVoteControllerDeserialize;
            On.RoR2.NetworkExtensions.Write_NetworkWriter_RuleBook += NetworkRuleBookSerialize;
            On.RoR2.NetworkExtensions.ReadRuleBook += NetworkRuleBookDeserialize;
            On.RoR2.RuleMask.Serialize += RuleMaskSerialize;
            On.RoR2.RuleMask.Deserialize += RuleMaskDeserialize;
            On.RoR2.RuleChoiceMask.Serialize += RuleChoiceMaskSerialize;
            On.RoR2.RuleChoiceMask.Deserialize += RuleChoiceMaskDeserialize;
        }

        public static void UnsetHook()
        {
            On.RoR2.RuleCatalog.Init -= RegisterVotes;
            On.RoR2.Run.SetRuleBook -= ApplyVotes;
            On.RoR2.VoteController.OnSerialize -= VoteControllerSerialize;
            On.RoR2.VoteController.OnDeserialize -= VoteControllerDeserialize;
            On.RoR2.PreGameRuleVoteController.WriteVotes -= PreGameRuleVoteControllerSerialize;
            On.RoR2.PreGameRuleVoteController.ReadVotes -= PreGameRuleVoteControllerDeserialize;
            On.RoR2.NetworkExtensions.Write_NetworkWriter_RuleBook -= NetworkRuleBookSerialize;
            On.RoR2.NetworkExtensions.ReadRuleBook -= NetworkRuleBookDeserialize;
            On.RoR2.RuleBook.Serialize -= RuleBookSerialize;
            On.RoR2.RuleBook.Deserialize -= RuleBookDeserialize;
            On.RoR2.RuleMask.Serialize -= RuleMaskSerialize;
            On.RoR2.RuleMask.Deserialize -= RuleMaskDeserialize;
            On.RoR2.RuleChoiceMask.Serialize -= RuleChoiceMaskSerialize;
            On.RoR2.RuleChoiceMask.Deserialize -= RuleChoiceMaskDeserialize;
        }

        #region Hooks
        static void RegisterVotes(On.RoR2.RuleCatalog.orig_Init orig)
        {
            foreach (RuleCategoryDef header in VoteHeadersTop) RegisterVoteHeader(header);
            orig();
            foreach(RuleCategoryDef header in VoteHeadersBottom) RegisterVoteHeader(header);
            foreach (RuleDef selection in VoteSelections) RegisterVoteSelection(selection);

            //Filler due to last 2 indexes never showing up
            //RuleCategoryDef dummyHeader = VoteAPI.AddVoteHeader("Dummy", new Color(0.0f, 0.0f, 0.0f, 1.0f), true);
            //RegisterVoteHeader(dummyHeader);

            //RuleDef dummySelection1 = VoteAPI.AddVoteSelection(dummyHeader, "Dummy1", new ChoiceMenu("Dummy1 On", new Color(0.0f, 0.0f, 0.0f, 0.4f), "", Color.black, "Textures/ArtifactIcons/texCommandSmallSelected", "artifact_dummy", -1));
            //VoteAPI.AddVoteChoice(dummySelection1, new ChoiceMenu("Dummy1 Off", new Color(0.0f, 0.0f, 0.0f, 0.4f), "", Color.black, "Textures/ArtifactIcons/texCommandSmallDeselected", "artifact_dummy", -1));
            //dummySelection1.defaultChoiceIndex = 0;
            //RegisterVoteSelection(dummySelection1);

            //RuleDef dummySelection2 = VoteAPI.AddVoteSelection(dummyHeader, "Dummy2", new ChoiceMenu("Dummy2 On", new Color(0.0f, 0.0f, 0.0f, 0.4f), "", Color.black, "Textures/ArtifactIcons/texCommandSmallSelected", "artifact_dummy", -1));
            //VoteAPI.AddVoteChoice(dummySelection2, new ChoiceMenu("Dummy2 Off", new Color(0.0f, 0.0f, 0.0f, 0.4f), "", Color.black, "Textures/ArtifactIcons/texCommandSmallDeselected", "artifact_dummy", -1));
            //dummySelection2.defaultChoiceIndex = 0;
            //RegisterVoteSelection(dummySelection2);
        }

        static void ApplyVotes(On.RoR2.Run.orig_SetRuleBook orig, Run self, RuleBook newRuleBook)
        {
            VoteResults = VoteMask.none;
            for (int i = 0; i < RegisteredVoteSelections.Count; i++)
            {
                VoteChoiceDef voteChoice = (VoteChoiceDef)newRuleBook.GetRuleChoice(RegisteredVoteSelections[i]);
                if (voteChoice.VoteIndex > 0) VoteResults.AddVote(voteChoice.VoteIndex);
            }
            orig(self, newRuleBook);
        }

        static bool VoteControllerSerialize(On.RoR2.VoteController.orig_OnSerialize orig, VoteController self, NetworkWriter writer, bool forceAll)
        {
            bool result = orig(self, writer, forceAll);
            for (int i = 0; i < RuleBookExtraBytes.Count; i++) writer.Write(RuleBookExtraBytes[i]);
            for (int i = 0; i < RuleChoiceMaskExtraBytes.Count; i++) writer.Write(RuleChoiceMaskExtraBytes[i]);
            RuleBookExtraBytes.Clear();
            RuleChoiceMaskExtraBytes.Clear();
            return result;
        }

        static void VoteControllerDeserialize(On.RoR2.VoteController.orig_OnDeserialize orig, VoteController self, NetworkReader reader, bool initialState)
        {
            orig(self, reader, initialState);
            if (reader.Position < reader.Length && !Run.instance)
            {
                if (TargetedRuleBook != null && TargetedRuleChoiceMask != null)
                {
                    byte[] ruleBookBytes = TargetedRuleBook.GetFieldValue<byte[]>("ruleValues");
                    byte[] ruleChoiceMaskBytes = (byte[])TargetedRuleChoiceMask.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TargetedRuleChoiceMask);

                    for (int i = ruleBookBytes.Length - RegisteredVoteSelections.Count; i < ruleBookBytes.Length; i++) ruleBookBytes[i] = reader.ReadByte();

                    int difference = ruleChoiceMaskBytes.Length - ((RuleCatalog.choiceCount - RegisteredVoteChoices.Count) + 7 >> 3);
                    for (int i = ruleChoiceMaskBytes.Length - difference; i < ruleChoiceMaskBytes.Length; i++) ruleChoiceMaskBytes[i] = reader.ReadByte();

                    TargetedRuleBook = null;
                    TargetedRuleChoiceMask = null;
                }
            }
        }

        static void PreGameRuleVoteControllerSerialize(On.RoR2.PreGameRuleVoteController.orig_WriteVotes orig, PreGameRuleVoteController self, NetworkWriter writer)
        {
            Array votes = self.GetType().GetField("votes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) as Array;
            RuleMask ruleMaskBuffer = self.GetFieldValue<RuleMask>("ruleMaskBuffer");

            for (int i = 0; i < RuleCatalog.ruleCount; i++) ruleMaskBuffer[i] = votes.GetValue(i).GetPropertyValue<bool>("hasVoted");
            writer.Write(ruleMaskBuffer);
            for (int i = 0; i < RuleCatalog.ruleCount - RegisteredVoteSelections.Count; i++)
            {
                if (votes.GetValue(i).GetPropertyValue<bool>("hasVoted")) votes.GetValue(i).GetType().InvokeMethod("Serialize", writer, votes.GetValue(i));
            }

            for (int i = 0; i < RuleMaskExtraBytes.Count; i++) writer.Write(RuleMaskExtraBytes[i]);
            for (int i = RuleCatalog.ruleCount - RegisteredVoteSelections.Count; i < RuleCatalog.ruleCount; i++)
            {
                if (votes.GetValue(i).GetPropertyValue<bool>("hasVoted")) votes.GetValue(i).GetType().InvokeMethod("Serialize", writer, votes.GetValue(i));
            }
            RuleMaskExtraBytes.Clear();
        }

        static void PreGameRuleVoteControllerDeserialize(On.RoR2.PreGameRuleVoteController.orig_ReadVotes orig, PreGameRuleVoteController self, NetworkReader reader)
        {
            Array votes = self.GetType().GetField("votes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) as Array;
            RuleMask ruleMaskBuffer = self.GetFieldValue<RuleMask>("ruleMaskBuffer");

            reader.ReadRuleMask(ruleMaskBuffer);
            bool flag = !self.networkUserNetworkIdentity || !self.networkUserNetworkIdentity.isLocalPlayer;
            for (int i = 0; i < RuleCatalog.ruleCount - RegisteredVoteSelections.Count; i++)
            {
                object vote;
                if (ruleMaskBuffer[i]) vote = votes.GetValue(i).GetType().InvokeMethod<object>("Deserialize", reader);
                else vote = default(object);
                if (flag) votes.SetValue(vote, i);
            }

            if (TargetedRuleMask != null)
            {
                if(reader.Position < reader.Length)
                {
                    byte[] bytes = (byte[])TargetedRuleMask.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(TargetedRuleMask);
                    int difference = bytes.Length - ((RuleCatalog.ruleCount - RegisteredVoteSelections.Count) + 7 >> 3);
                    for (int i = bytes.Length - difference; i < bytes.Length; i++) bytes[i] = reader.ReadByte();

                    for (int i = RuleCatalog.ruleCount - RegisteredVoteSelections.Count; i < RuleCatalog.ruleCount; i++)
                    {
                        object vote;
                        if (ruleMaskBuffer[i]) vote = votes.GetValue(i).GetType().InvokeMethod<object>("Deserialize", reader);
                        else vote = default(object);
                        if (flag) votes.SetValue(vote, i);
                    }
                }
                TargetedRuleMask = null;
            }

            self.GetType().GetField("shouldUpdateGameVotes", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, ((bool)self.GetType().GetField("shouldUpdateGameVotes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) || flag));
            if (NetworkServer.active) self.SetDirtyBit(2u);
        }

        static void NetworkRuleBookSerialize(On.RoR2.NetworkExtensions.orig_Write_NetworkWriter_RuleBook orig, NetworkWriter writer, RuleBook src)
        {
            On.RoR2.RuleBook.Serialize += RuleBookSerialize;
            orig(writer, src);
        }

        static void NetworkRuleBookDeserialize(On.RoR2.NetworkExtensions.orig_ReadRuleBook orig, NetworkReader reader, RuleBook dest)
        {
            On.RoR2.RuleBook.Deserialize += RuleBookDeserialize;
            orig(reader, dest);
        }

        static void RuleBookSerialize(On.RoR2.RuleBook.orig_Serialize orig, RuleBook self, NetworkWriter writer)
        {
            byte[] ruleValues = self.GetFieldValue<byte[]>("ruleValues");
            for (int i = 0; i < ruleValues.Length - RegisteredVoteSelections.Count; i++) writer.Write(ruleValues[i]);
            for (int i = ruleValues.Length - RegisteredVoteSelections.Count; i < ruleValues.Length; i++) RuleBookExtraBytes.Add(ruleValues[i]);
        }

        static void RuleBookDeserialize(On.RoR2.RuleBook.orig_Deserialize orig, RuleBook self, NetworkReader reader)
        {
            byte[] ruleValues = self.GetFieldValue<byte[]>("ruleValues");
            for (int i = 0; i < ruleValues.Length - RegisteredVoteSelections.Count; i++) ruleValues[i] = reader.ReadByte();
            TargetedRuleBook = self;
        }

        static void RuleMaskSerialize(On.RoR2.RuleMask.orig_Serialize orig, RuleMask self, NetworkWriter writer)
        {
            byte[] bytes = (byte[])self.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            int difference = bytes.Length - ((RuleCatalog.ruleCount - RegisteredVoteSelections.Count) + 7 >> 3);
            for (int i = 0; i < bytes.Length - difference; i++) writer.Write(bytes[i]);
            for (int i = bytes.Length - difference; i < bytes.Length; i++) RuleMaskExtraBytes.Add(bytes[i]);
        }

        static void RuleMaskDeserialize(On.RoR2.RuleMask.orig_Deserialize orig, RuleMask self, NetworkReader reader)
        {
            byte[] bytes = (byte[])self.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            int difference = bytes.Length - ((RuleCatalog.ruleCount - RegisteredVoteSelections.Count) + 7 >> 3);
            for (int i = 0; i < bytes.Length - difference; i++) bytes[i] = reader.ReadByte();
            TargetedRuleMask = self;
        }

        static void RuleChoiceMaskSerialize(On.RoR2.RuleChoiceMask.orig_Serialize orig, RuleChoiceMask self, NetworkWriter writer)
        {
            byte[] bytes = (byte[])self.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            int difference = bytes.Length - ((RuleCatalog.choiceCount - RegisteredVoteChoices.Count) + 7 >> 3);
            for (int i = 0; i < bytes.Length - difference; i++) writer.Write(bytes[i]);
            for (int i = bytes.Length - difference; i < bytes.Length; i++) RuleChoiceMaskExtraBytes.Add(bytes[i]);
        }

        static void RuleChoiceMaskDeserialize(On.RoR2.RuleChoiceMask.orig_Deserialize orig, RuleChoiceMask self, NetworkReader reader)
        {
            byte[] bytes = (byte[])self.GetType().BaseType.GetField("bytes", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            int difference = bytes.Length - ((RuleCatalog.choiceCount - RegisteredVoteChoices.Count) + 7 >> 3);
            for (int i = 0; i < bytes.Length - difference; i++) bytes[i] = reader.ReadByte();
            TargetedRuleChoiceMask = self;
        }
        #endregion

        #region Helpers
        public static RuleCategoryDef AddVoteHeader(string categoryToken, Color color, bool addAfterVanillaHeaders = false)
        {
            RuleCategoryDef header = new RuleCategoryDef
            {
                position = -1,
                displayToken = categoryToken,
                color = color,
                emptyTipToken = null,
                hiddenTest = new Func<bool>(HiddenTestFalse)
            };
            if(addAfterVanillaHeaders) VoteHeadersBottom.Add(header);
            else VoteHeadersTop.Add(header);
            return header;
        }

        public static RuleDef AddVoteSelection(RuleCategoryDef header, string selectionName, ChoiceMenu choiceMenu)
        {
            RuleDef selection = new RuleDef("Votes." + selectionName, selectionName);
            selection.category = header;
            AddVoteChoice(selection, choiceMenu);
            header.children.Add(selection);
            VoteSelections.Add(selection);
            return selection;
        }

        public static void AddVoteChoice(RuleDef selection, ChoiceMenu choiceMenu)
        {
            VoteChoiceDef choice = CreateChoice(ref selection, choiceMenu.TooltipName, null, false);
            choice.tooltipNameToken = choiceMenu.TooltipName;
            choice.tooltipNameColor = choiceMenu.TooltipNameColor;
            choice.tooltipBodyToken = choiceMenu.TooltipBody;
            choice.tooltipBodyColor = choiceMenu.TooltipBodyColor;
            choice.spritePath = choiceMenu.IconPath;
            //choice.unlockableName = choiceMenu.UnlockableName;
            choice.VoteIndex = choiceMenu.ChoiceIndex;
        }
        #endregion

        #region Functions
        static void RegisterVoteHeader(RuleCategoryDef header)
        {
            (typeof(RuleCatalog)).GetFieldValue<List<RuleCategoryDef>>("allCategoryDefs").Add(header);
            RegisteredVoteHeaders.Add(header);
        }

        static void RegisterVoteSelection(RuleDef selection)
        {
            List<RuleDef> allRuleDefs = (typeof(RuleCatalog)).GetFieldValue<List<RuleDef>>("allRuleDefs");
            List<RuleChoiceDef> allChoicesDefs = (typeof(RuleCatalog)).GetFieldValue<List<RuleChoiceDef>>("allChoicesDefs");

            selection.globalIndex = allRuleDefs.Count;
            if (selection.category.position == 0) selection.category.position = selection.globalIndex;
            for (int i = 0; i < selection.choices.Count; i++)
            {
                RuleChoiceDef choice = selection.choices[i];
                choice.globalIndex = allChoicesDefs.Count;
                choice.localIndex = i;
                allChoicesDefs.Add(choice);
                RegisteredVoteChoices.Add(choice);
            }

            allRuleDefs.Add(selection);
            if (RuleCatalog.highestLocalChoiceCount < selection.choices.Count) (typeof(RuleCatalog)).SetFieldValue<int>("highestLocalChoiceCount", selection.choices.Count);
            (typeof(RuleCatalog)).GetFieldValue<Dictionary<string, RuleDef>>("ruleDefsByGlobalName").Add(selection.globalName, selection);
            foreach (RuleChoiceDef choice in selection.choices) (typeof(RuleCatalog)).GetFieldValue<Dictionary<string, RuleChoiceDef>>("ruleChoiceDefsByGlobalName").Add(choice.globalName, choice);
            RegisteredVoteSelections.Add(selection);
        }

        static VoteChoiceDef CreateChoice(ref RuleDef selection, string choiceName, object extraData = null, bool excludeByDefault = false)
        {
            RuleChoiceDef choice = new VoteChoiceDef();
            choice.ruleDef = selection;
            choice.localName = choiceName;
            choice.globalName = selection.globalName + "." + choiceName;
            choice.localIndex = selection.choices.Count;
            choice.extraData = extraData;
            choice.excludeByDefault = excludeByDefault;
            selection.GetFieldValue<List<RuleChoiceDef>>("choices").Add(choice);
            return (VoteChoiceDef)choice;
        }

        static bool HiddenTestFalse()
        {
            return false;
        }
        #endregion
    }
}
