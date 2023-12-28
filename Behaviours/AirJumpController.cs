using AirJump.Utils;
using AirJump.Views;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AirJump.Behaviours
{
    public class AirJumpController : MonoBehaviour
    {
        bool leftOnce;
        bool rightOnce;

        Transform leftHandTransform => GorillaLocomotion.Player.Instance.leftControllerTransform;
        Transform rightHandTransform => GorillaLocomotion.Player.Instance.rightControllerTransform;

        static Dictionary<Player, JumpController> cachedLeftJumps = new Dictionary<Player, JumpController>();
        static Dictionary<Player, JumpController> cachedRightJumps = new Dictionary<Player, JumpController>();

        public void Update()
        {
            if (Plugin.inModded && AirJumpScreen.options[0].Value() > 0)
            {
                CheckInput();
            }
            else
            {
                foreach (KeyValuePair<Player, JumpController> kvL in cachedLeftJumps)
                {
                    DestroyNetworkJump(kvL.Value.Jump.IsLeftHand, kvL.Key);
                }
                foreach (KeyValuePair<Player, JumpController> kvR in cachedRightJumps)
                {
                    DestroyNetworkJump(kvR.Value.Jump.IsLeftHand, kvR.Key);
                }

                leftOnce = false;
                rightOnce = false;
            }
        }
        
        void CheckInput()
        {
            if (AirJumpInput.GetInput(true, InputType.Grip))
            {
                if (!leftOnce)
                {
                    CreateJump(true, leftHandTransform, leftHandTransform.position + Constants.rightHandOffset);
                    leftOnce = true;
                }
            }
            else
            {
                if (leftOnce)
                {
                    DestroyJump(true);
                    leftOnce = false;
                }
            }

            if (AirJumpInput.GetInput(false, InputType.Grip))
            {
                if (!rightOnce)
                {
                    CreateJump(false, rightHandTransform, rightHandTransform.position + Constants.rightHandOffset);
                    rightOnce = true;
                }
            }
            else
            {
                if (rightOnce)
                {
                    DestroyJump(false);
                    rightOnce = false;
                }
            }
        }

        void CreateJump(bool isLeftHand, Transform hand, Vector3 position)
        {
            byte Size = (byte)AirJumpScreen.options[1].Value();
            byte Material = (byte)AirJumpScreen.options[2].Value();
            CreateNetworkJump(isLeftHand, position, hand.rotation, Size, Material, PhotonNetwork.LocalPlayer);
        }

        void DestroyJump(bool isLeftHand)
        {
            DestroyNetworkJump(isLeftHand, PhotonNetwork.LocalPlayer);
        }

        public static void CreateNetworkJump(bool isLeftHand, Vector3 position, Quaternion rotation, byte size, byte material, Player player)
        {
            try
            {
                //Console.WriteLine($"Creating | {isLeftHand} for {player}");

                Dictionary<Player, JumpController> cachedJumps = isLeftHand ? cachedLeftJumps : cachedRightJumps;

                foreach (KeyValuePair<Player, JumpController> kv in cachedJumps)
                {
                    //Console.WriteLine(kv.Key + " owns " + kv.Value.Jump.Player);
                }

                if (cachedJumps.ContainsKey(player))
                {
                    //Console.WriteLine($"Already contains {player}");
                    return;
                }

                JumpController controller = new JumpController(new Structs.Jump()
                {
                    Player = player,
                    Size = size,
                    Material = material,
                    Position = position,
                    Rotation = rotation,
                    IsLeftHand = isLeftHand
                });

                //cachedJumps[player] = controller;
                cachedJumps.Add(player, controller);
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
        }

        public static void DestroyNetworkJump(bool isLeftHand, Player player)
        {
            try
            {
                //Console.WriteLine($"Destroying | {isLeftHand}");
                Dictionary<Player, JumpController> cachedJumps = isLeftHand ? cachedLeftJumps : cachedRightJumps;
                foreach (KeyValuePair<Player, JumpController> kv in cachedJumps)
                {
                    //Console.WriteLine($"{kv.Value.Jump.Player} == {player} ({kv.Value.Jump.Player == player})\n{kv.Value.Jump.IsLeftHand} == {isLeftHand} ({kv.Value.Jump.IsLeftHand == isLeftHand})");
                    //if (kv.Value.Jump.IsLeftHand == isLeftHand)
                    if (kv.Key == player && kv.Value.Jump.IsLeftHand == isLeftHand)
                    {
                        kv.Value.Destroy();
                        cachedJumps.Remove(kv.Key);
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
        }
    }
}
