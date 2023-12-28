using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace AirJump.Structs
{
    public struct Jump
    {
        public Player Player;
        public Vector3 Position;
        public Quaternion Rotation;
        public int Material;
        public int Size;
        public bool IsLeftHand;
        public GameObject Object;
        public bool IsMine
        {
            get
            {
                return Player == PhotonNetwork.LocalPlayer;
            }
        }
    }
}
