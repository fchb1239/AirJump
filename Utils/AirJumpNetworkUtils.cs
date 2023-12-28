using AirJump.Behaviours;
using AirJump.Structs;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace AirJump.Utils
{
    public static class AirJumpNetworkUtils
    {
        public static void Register()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
            Console.WriteLine("AirJump Successfully registered OnEvent");
        }

        public static void SendCreateJump(Jump jump)
        {
            object[] data = new object[]
            {
                jump.IsLeftHand,
                jump.Position,
                jump.Rotation,
                (byte)jump.Size,
                (byte)jump.Material
            };
            SendRaiseEvent(EventCodes.Create, data);
        }

        public static void SendDestroyJump(Jump jump)
        {
            object[] data = new object[]
            {
                jump.IsLeftHand
            };
            SendRaiseEvent(EventCodes.Destroy, data);
        }

        private static void SendRaiseEvent(EventCodes code, object[] data)
        {
            //Console.WriteLine($"Sending {code} with data {data}");
            RaiseEventOptions options = new RaiseEventOptions()
            {
                Receivers = ReceiverGroup.Others
            };

            PhotonNetwork.RaiseEvent((byte)code, data, options, SendOptions.SendReliable);
        }

        public static void OnEvent(EventData eventData)
        {
            try
            {
                byte eventCode = eventData.Code;
                object[] data = null;

                if (eventData.CustomData != null)
                    data = (object[])eventData.CustomData;

                switch (eventCode)
                {
                    case (byte)EventCodes.Create:
                        //Console.WriteLine("Create event");
                        bool isLeftHand = (bool)data[0];
                        Vector3 position = (Vector3)data[1];
                        Quaternion rotation = (Quaternion)data[2];
                        byte size = (byte)data[3];
                        byte material = (byte)data[4];
                        //Console.WriteLine("Spawning");
                        //Console.WriteLine($"Spawning for {eventData.Sender} AKA {PhotonNetwork.LocalPlayer.Get(eventData.Sender)}");
                        AirJumpController.CreateNetworkJump(isLeftHand, position, rotation, size, material, PhotonNetwork.LocalPlayer.Get(eventData.Sender));
                        break;
                    case (byte)EventCodes.Destroy:
                        //Console.WriteLine("Destroy event");
                        AirJumpController.DestroyNetworkJump((bool)data[0], PhotonNetwork.LocalPlayer.Get(eventData.Sender));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                // We do NOT want to cause an error in OnEvent, this is just in case
                // This is also so cheaters can't spam errors on other peoples clients by spamming the events (Which can cause lag)
                //Console.WriteLine(e);
            }
        }

        public enum EventCodes
        {
            Create = 80,
            Destroy = 81
        }
    }
}
