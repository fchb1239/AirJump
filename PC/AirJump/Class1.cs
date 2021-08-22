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
    [BepInPlugin("org.fchb1239.gorillagame.AirJump", "AirJump", "0.0.6.9")] //He he he, funny number.
    [BepInProcess("Gorilla Tag.exe")] //Not needed, just because it's nicer.

    public class MonkePlugin : BaseUnityPlugin
    {
        private void Awake() //Function gets called when the mod is started.
        {
            new Harmony("com.fchb1239.gorillagame.AirJump").PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(GorillaLocomotion.Player))] //Pathes the player.
        [HarmonyPatch("Update", MethodType.Normal)] //Patching as an update method.
        private class AirJump_Patch
        {
            //Gotta make em' all static since the Postfix is forcing me to.
            static Vector3 scale = new Vector3(0.01f, 0.3f, 0.4f);
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

                if (!roomVisible || !PhotonNetwork.InRoom)
                {
                    if (!once_networking)
                    {
                        PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
                        once_networking = true;
                    }
                    List<InputDevice> list = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list); //Getting left controller.
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_left); //Makes it into an if statement.
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list); //Getting right controller.
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_right); //Makes it into an if statement.

                    if (gripDown_right) //I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
                    {
                        if (!once_right)
                        {
                            if (jump_right_local == null)
                            {
                                jump_right_local = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                jump_right_local.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                jump_right_local.transform.localScale = scale;
                                jump_right_local.transform.position = new Vector3(0, (float)-0.075, 0) + __instance.rightHandTransform.position;
                                jump_right_local.transform.rotation = __instance.rightHandTransform.rotation;

                                object[] right_form_1 = new object[] { new Vector3(0, (float)-0.075, 0) + __instance.rightHandTransform.position, __instance.rightHandTransform.rotation };

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

                    if (gripDown_left) //I'm sorry for putting if statements within if statements... OR ELSE IT WOULN'T WORK :<
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
            }

            static private void PlatformNetwork(EventData eventData)
            {
                byte eventCode = eventData.Code;
                if (eventCode == (byte)PhotonEventCodes.left_jump_photoncode)
                {
                    object[] data_left = (object[])eventData.CustomData; //Array
                    if (jump_left_network[0] == null)
                    {
                        int_jump_left_network[0] = eventData.Sender;
                        jump_left_network[0] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[0].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[0].transform.localScale = scale;
                        jump_left_network[0].transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network[0].transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network[1] == null)
                    {
                        int_jump_left_network[1] = eventData.Sender;
                        jump_left_network[1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[1].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[1].transform.localScale = scale;
                        jump_left_network[1].transform.position = (Vector3)data_left[0];
                        jump_left_network[1].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[2] == null)
                    {
                        int_jump_left_network[2] = eventData.Sender;
                        jump_left_network[2] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[2].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[2].transform.localScale = scale;
                        jump_left_network[2].transform.position = (Vector3)data_left[0];
                        jump_left_network[2].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[3] == null)
                    {
                        int_jump_left_network[3] = eventData.Sender;
                        jump_left_network[3] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[3].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[3].transform.localScale = scale;
                        jump_left_network[3].transform.position = (Vector3)data_left[0];
                        jump_left_network[3].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[4] == null)
                    {
                        int_jump_left_network[4] = eventData.Sender;
                        jump_left_network[4] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[4].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[4].transform.localScale = scale;
                        jump_left_network[4].transform.position = (Vector3)data_left[0];
                        jump_left_network[4].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[5] == null)
                    {
                        int_jump_left_network[5] = eventData.Sender;
                        jump_left_network[5] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[5].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[5].transform.localScale = scale;
                        jump_left_network[5].transform.position = (Vector3)data_left[0];
                        jump_left_network[5].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[6] == null)
                    {
                        int_jump_left_network[6] = eventData.Sender;
                        jump_left_network[6] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[6].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[6].transform.localScale = scale;
                        jump_left_network[6].transform.position = (Vector3)data_left[0];
                        jump_left_network[6].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[7] == null)
                    {
                        int_jump_left_network[7] = eventData.Sender;
                        jump_left_network[7] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[7].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[7].transform.localScale = scale;
                        jump_left_network[7].transform.position = (Vector3)data_left[0];
                        jump_left_network[7].transform.rotation = (Quaternion)data_left[1];
                    }
                    else if (jump_left_network[8] == null)
                    {
                        int_jump_left_network[8] = eventData.Sender;
                        jump_left_network[8] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network[8].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network[8].transform.localScale = scale;
                        jump_left_network[8].transform.position = (Vector3)data_left[0];
                        jump_left_network[8].transform.rotation = (Quaternion)data_left[1];
                    }
                    else
                    {
                        Console.WriteLine("An error occurred: Not enough game objects are currently available for the left hand (AirJump)"); //This should never happen.
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_photoncode)
                {
                    object[] data_right = (object[])eventData.CustomData; //Array
                    if (jump_right_network[0] == null)
                    {
                        int_jump_right_network[0] = eventData.Sender;
                        jump_right_network[0] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[0].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[0].transform.localScale = scale;
                        jump_right_network[0].transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network[0].transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network[1] == null)
                    {
                        int_jump_right_network[1] = eventData.Sender;
                        jump_right_network[1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[1].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[1].transform.localScale = scale;
                        jump_right_network[1].transform.position = (Vector3)data_right[0];
                        jump_right_network[1].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[2] == null)
                    {
                        int_jump_right_network[2] = eventData.Sender;
                        jump_right_network[2] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[2].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[2].transform.localScale = scale;
                        jump_right_network[2].transform.position = (Vector3)data_right[0];
                        jump_right_network[2].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[3] == null)
                    {
                        int_jump_right_network[3] = eventData.Sender;
                        jump_right_network[3] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[3].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[3].transform.localScale = scale;
                        jump_right_network[3].transform.position = (Vector3)data_right[0];
                        jump_right_network[3].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[4] == null)
                    {
                        int_jump_right_network[4] = eventData.Sender;
                        jump_right_network[4] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[4].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[4].transform.localScale = scale;
                        jump_right_network[4].transform.position = (Vector3)data_right[0];
                        jump_right_network[4].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[5] == null)
                    {
                        int_jump_right_network[5] = eventData.Sender;
                        jump_right_network[5] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[5].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[5].transform.localScale = scale;
                        jump_right_network[5].transform.position = (Vector3)data_right[0];
                        jump_right_network[5].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[6] == null)
                    {
                        int_jump_right_network[6] = eventData.Sender;
                        jump_right_network[6] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[6].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[6].transform.localScale = scale;
                        jump_right_network[6].transform.position = (Vector3)data_right[0];
                        jump_right_network[6].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[7] == null)
                    {
                        int_jump_right_network[7] = eventData.Sender;
                        jump_right_network[7] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[7].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[7].transform.localScale = scale;
                        jump_right_network[7].transform.position = (Vector3)data_right[0];
                        jump_right_network[7].transform.rotation = (Quaternion)data_right[1];
                    }
                    else if (jump_right_network[8] == null)
                    {
                        int_jump_right_network[8] = eventData.Sender;
                        jump_right_network[8] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network[8].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network[8].transform.localScale = scale;
                        jump_right_network[8].transform.position = (Vector3)data_right[0];
                        jump_right_network[8].transform.rotation = (Quaternion)data_right[1];
                    }
                    else
                    {
                        Console.WriteLine("An error occurred: Not enough game objects are currently available for the right hand (AirJump)"); //This should never happen.
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.left_jump_deletion)
                {
                    if (int_jump_left_network[0] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[0]);
                        jump_left_network[0] = null;
                    }
                    else if (int_jump_left_network[1] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[1]);
                        jump_left_network[1] = null;
                    }
                    else if (int_jump_left_network[2] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[2]);
                        jump_left_network[2] = null;
                    }
                    else if (int_jump_left_network[3] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[3]);
                        jump_left_network[3] = null;
                    }
                    else if (int_jump_left_network[4] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[4]);
                        jump_left_network[4] = null;
                    }
                    else if (int_jump_left_network[5] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[5]);
                        jump_left_network[5] = null;
                    }
                    else if (int_jump_left_network[6] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[6]);
                        jump_left_network[6] = null;
                    }
                    else if (int_jump_left_network[7] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[7]);
                        jump_left_network[7] = null;
                    }
                    else if (int_jump_left_network[8] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_left_network[8]);
                        jump_left_network[8] = null;
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_deletion)
                {
                    if (int_jump_right_network[0] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[0]);
                        jump_right_network[0] = null;
                    }
                    else if (int_jump_right_network[1] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[1]);
                        jump_right_network[1] = null;
                    }
                    else if (int_jump_right_network[2] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[2]);
                        jump_right_network[2] = null;
                    }
                    else if (int_jump_right_network[3] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[3]);
                        jump_right_network[3] = null;
                    }
                    else if (int_jump_right_network[4] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[4]);
                        jump_right_network[4] = null;
                    }
                    else if (int_jump_right_network[5] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[5]);
                        jump_right_network[5] = null;
                    }
                    else if (int_jump_right_network[6] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[6]);
                        jump_right_network[6] = null;
                    }
                    else if (int_jump_right_network[7] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[7]);
                        jump_right_network[7] = null;
                    }
                    else if (int_jump_right_network[8] == eventData.Sender)
                    {
                        GameObject.Destroy(jump_right_network[8]);
                        jump_right_network[8] = null;
                    }
                }
            }
        }
    }
}

//Copyright (c) 2021 fchb1239
