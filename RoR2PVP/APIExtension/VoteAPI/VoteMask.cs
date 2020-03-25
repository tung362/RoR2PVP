using System;
using UnityEngine;

namespace APIExtension.VoteAPI
{
    [Serializable]
    public struct VoteMask
    {
        public bool HasVote(int voteIndex)
        {
            return voteIndex >= 0 && ((int)a & 1 << (int)voteIndex) != 0;
        }

        public void AddVote(int voteIndex)
        {
            if (voteIndex < 0) return;
            a |= (ushort)(1 << (int)voteIndex);
        }

        public void ToggleVote(int voteIndex)
        {
            if (voteIndex < 0) return;
            a ^= (ushort)(1 << (int)voteIndex);
        }

        public void RemoveVote(int voteIndex)
        {
            if (voteIndex < 0) return;
            a &= (ushort)(~(ushort)(1 << (int)voteIndex));
        }

        public static VoteMask operator &(VoteMask mask1, VoteMask mask2)
        {
            return new VoteMask
            {
                a = (ushort)(mask1.a & mask2.a)
            };
        }

        [SerializeField]
        public ushort a;

        public static readonly VoteMask none;

        public static readonly VoteMask all = default(VoteMask);
    }
}
