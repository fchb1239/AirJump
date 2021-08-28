//WARNING: THIS CODE IS WORSE THAN YANDERE SIMULATOR CODE! DON'T READ THIS TO LEARN ANYTHING, IT'S BAD!
using System;
using System.Collections.Generic;
using HarmonyLib;
using BepInEx;
using UnityEngine;
using System.Reflection;
using UnityEngine.XR;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;

public enum PhotonEventCodes //Stores all the Photon codes, if you're making a mod do NOT use these photon values.
{
    left_jump_photoncode = 69,
    right_jump_photoncode = 70,
    left_jump_deletion = 71,
    right_jump_deletion = 72
}


namespace AirJump
{
    [BepInPlugin("org.fchb1239.gorillagame.AirJump", "AirJump", "0.6.9")]

    public class MonkePlugin : BaseUnityPlugin
    {
        static bool enabled = true; //It's for ComputerInterface.
        private void Awake()
        {
            new Harmony("com.fchb1239.gorillagame.AirJump").PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnEnable()
        {
            enabled = true;
        }

        private void OnDisable()
        {
            enabled = false;
        }

        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPatch("Update", MethodType.Normal)]
        private class AirJump_Patch
        {
            //Gotta make em' all static since the Postfix is forcing me to.
            static Vector3 scale = new Vector3(0.0125f, 0.25f, 0.335f);
            static bool gripDown_left;
            static bool gripDown_right;
            static bool once_left;
            static bool once_right;
            static bool once_left_false;
            static bool once_right_false;
            static bool once_networking;
            static int[] int_jump_left_network = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            static int[] int_jump_right_network = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            static GameObject[] jump_left_network = new GameObject[] { null, null, null, null, null, null, null, null, null };
            static GameObject[] jump_right_network = new GameObject[] { null, null, null, null, null, null, null, null, null };
            static GameObject jump_left_local = null;
            static GameObject jump_right_local = null;

            private static void Postfix(GorillaLocomotion.Player __instance) //Postfix is better than prefix.
            {
                bool roomVisible = PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.IsVisible;

                if (!roomVisible && enabled)
                {
                    if (!once_networking)
                    {
                        PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
                        once_networking = true;
                    }
                    List<InputDevice> list = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_left); 
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list);
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_right);

                    if (gripDown_right) //Right hand - I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
                    {
                        if (!once_right)
                        {
                            if (jump_right_local == null)
                            {
                                jump_right_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                jump_right_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                jump_right_local.transform.localScale = scale;
                                jump_right_local.transform.position = new Vector3(0, (float)-0.0075, 0) + __instance.rightHandTransform.position; //The reason for moving it down a little, is because on the right hand the cube would spawn ontop of the hand for some reason.
                                jump_right_local.transform.rotation = __instance.rightHandTransform.rotation;

                                object[] right_form_1 = new object[] { new Vector3(0, (float)-0.0075, 0) + __instance.rightHandTransform.position, __instance.rightHandTransform.rotation };

                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_photoncode, right_form_1, raiseEventOptions, SendOptions.SendReliable);

                                once_right = true;
                                once_right_false = false;
                            }
                        }
                    }
                    else
                    {
                        if (!once_right_false)
                        {
                            if (jump_right_local != null)
                            {
                                GameObject.Destroy(jump_right_local);
                                jump_right_local = null;

                                once_right = false;
                                once_right_false = true;

                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable);
                            }
                        }
                    }

                    if (gripDown_left) //Left hand - I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
                    {
                        if(!once_left)
                        {
                            if(jump_left_local == null)
                            {
                                jump_left_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                jump_left_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                jump_left_local.transform.localScale = scale;
                                jump_left_local.transform.position = __instance.leftHandTransform.position;
                                jump_left_local.transform.rotation = __instance.leftHandTransform.rotation;

                                object[] left_form_1 = new object[] { __instance.leftHandTransform.position, __instance.leftHandTransform.rotation };

                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_photoncode, left_form_1, raiseEventOptions, SendOptions.SendReliable);

                                once_left = true;
                                once_left_false = false;
                            }
                        }
                    }
                    else
                    {
                        if (!once_left_false)
                        {
                            if (jump_left_local != null)
                            {
                                GameObject.Destroy(jump_left_local);
                                jump_left_local = null;

                                once_left = false;
                                once_left_false = true;

                                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

                                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable);
                            }
                        }
                    }
                }

                if (!PhotonNetwork.InRoom) //Worst way of doing it, but I'm not even bothered lol.
                {
                    for (int i = 0; i < jump_right_network.Length; i++)
                    {
                        GameObject.Destroy(jump_right_network[i]);
                    }

                    for (int i = 0; i < jump_left_network.Length; i++)
                    {
                        GameObject.Destroy(jump_left_network[i]);
                    }
                }
            }

            static private void PlatformNetwork(EventData eventData)
            {
                byte eventCode = eventData.Code;
                if (eventCode == (byte)PhotonEventCodes.left_jump_photoncode)
                {
                    object[] data_left = (object[])eventData.CustomData; //Array
                    try
                    {
                        for (int i = 0; i < jump_left_network.Length; i++)
                        {
                            int_jump_left_network[i] = eventData.Sender;
                            jump_left_network[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            jump_left_network[i].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                            jump_left_network[i].transform.localScale = scale;
                            jump_left_network[i].transform.position = (Vector3)data_left[0]; //Sets position.
                            jump_left_network[i].transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                            i++;
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e); }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_photoncode)
                {
                    object[] data_right = (object[])eventData.CustomData; //Array
                    try
                    {
                        for (int i = 0; i < jump_right_network.Length; i++)
                        {
                            int_jump_right_network[i] = eventData.Sender;
                            jump_right_network[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            jump_right_network[i].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                            jump_right_network[i].transform.localScale = scale;
                            jump_right_network[i].transform.position = (Vector3)data_right[0]; //Sets position.
                            jump_right_network[i].transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                            i++;
                        }
                    }
                    catch (Exception e) { Console.WriteLine(e); }
                }
                else if (eventCode == (byte)PhotonEventCodes.left_jump_deletion)
                {
                    int i = 0;
                    foreach (int int_net in int_jump_left_network)
                    {
                        if (int_net == eventData.Sender)
                        {
                            GameObject.Destroy(jump_left_network[i]);
                            jump_left_network[i] = null;
                        }
                        i++;
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_deletion)
                {
                    int i = 0;
                    foreach (int int_net in int_jump_right_network)
                    {
                        if (int_net == eventData.Sender)
                        {
                            GameObject.Destroy(jump_right_network[i]);
                            jump_right_network[i] = null;
                        }
                        i++;
                    }
                }
            }
        }
    }
}

//Copyright (c) 2021 fchb1239
