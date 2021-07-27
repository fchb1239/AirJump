/*
MIT License
Copyright (c) 2021 fchb1239
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
In other words any modification or redistributing is herby granted, however the original creator must be credited.

I have commented everything so you can get a better understanding of what's going on.
Do NOT modify this code to work in public lobbies, if you do so you have a high chance of getting banned from the game.
Credits: stackoverflow.com
*/
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
            static bool gripDown_left;
            static bool gripDown_right;
            static bool once_left = false;
            static bool once_right = false;
            static bool once_left_false = false;
            static bool once_right_false = false;
            static bool once_networking = false;
            static GameObject jump_left = null;
            static GameObject jump_right = null;
            static GameObject jump_left_network1 = null;
            static GameObject jump_right_network1 = null;
            static GameObject jump_left_network2 = null;
            static GameObject jump_right_network2 = null;
            static GameObject jump_left_network3 = null;
            static GameObject jump_right_network3 = null;
            static GameObject jump_left_network4 = null;
            static GameObject jump_right_network4 = null;
            static GameObject jump_left_network5 = null;
            static GameObject jump_right_network5 = null;
            static GameObject jump_left_network6 = null;
            static GameObject jump_right_network6 = null;
            static GameObject jump_left_network7 = null;
            static GameObject jump_right_network7 = null;
            static GameObject jump_left_network8 = null;
            static GameObject jump_right_network8 = null;
            static GameObject jump_left_network9 = null;
            static GameObject jump_right_network9 = null;



            private static void Postfix(GorillaLocomotion.Player __instance) //Postfix is better than prefix.
            {
                bool roomVisible = PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.IsVisible;

                if (!roomVisible || !PhotonNetwork.InRoom)
                {
                    if(!once_networking)
                    {
                        PhotonNetwork.NetworkingClient.EventReceived += PlatformNetwork;
                        once_networking = true;
                    }
                    List<InputDevice> list = new List<InputDevice>();
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller, list); //Getting left controller.
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_left); //Makes it into an if statement.
                    InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller, list); //Getting right controller.
                    list[0].TryGetFeatureValue(CommonUsages.gripButton, out gripDown_right); //Makes it into an if statement.

                    if (gripDown_left && jump_left == null) //Checks if the left grip is down and the left platform hasn't spawnned yet.
                    {
                        DrawLeftHand(); //Calls the void to create the platform.
                    }
                    else if (!gripDown_left && jump_left != null) //Checks if you let go of the left grip button and the platform exists.
                    {
                        GameObject.Destroy(jump_left); //Destroys left platform.
                        jump_left = null; //Sets it back to null.
                    }

                    if (gripDown_right && jump_right == null) //Checks if the right grip is down and the right platform hasn't spawnned yet.
                    {
                        DrawRightHand(); //Calls the void to create the platform.
                    }
                    else if (!gripDown_right && jump_right != null) //Checks if you let go of the right grip button and the platform exists.
                    {
                        GameObject.Destroy(jump_right); //Destroys right platform.
                        jump_right = null; //Sets it back to null.
                    }

                    if (gripDown_left && jump_left != null)
                    {
                        if (!once_left) //Does it once so the platform doesn't follow your hand.
                        {
                            jump_left.transform.position = __instance.leftHandTransform.position; //Sets the left platforms position to your hand position.
                            jump_left.transform.rotation = __instance.leftHandTransform.rotation; //Sets the left platforms rotation to your hand rotation.
                            once_left = true; //Sets once to true so it does it once.
                            once_left_false = false; //I made this for networking so it doesn't spam it and deleted platforms every second.
                        }
                    }
                    else
                    {
                        if(!once_left_false) //Deletion of left networked cube
                        {
                            once_left = false; //Sets once back so you can spawn in platforms again.
                            once_left_false = true; //I made this for networking so it doesn't spam it and deleted platforms every second.

                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                        }
                    }

                    if (gripDown_right && jump_right != null)
                    {
                        if (!once_right) //Does it once so the platform doesn't follow your hand.
                        {
                            jump_right.transform.position = new Vector3(0, (float)-0.075, 0) + __instance.rightHandTransform.position; //Sets the right platforms position to your hand position, the reason there is a -0.1 in the Vector3 is because the spawning position is a little off.
                            jump_right.transform.rotation = __instance.rightHandTransform.rotation; //Sets the right platforms rotation to your hand rotation.
                            once_right = true; //Sets once to true so it does it once.
                            once_right_false = false; //I made this for networking so it doesn't spam it and deleted platforms every second.
                        }
                    }
                    else
                    {
                        if(!once_right_false) //Deletion of right networked cube
                        {
                            once_right = false; //Sets once back so you can spawn in platforms again.
                            once_right_false = true; //I made this for networking so it doesn't spam it and deleted platforms every second.

                            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                            PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_deletion, null, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                        }
                    }

                    void DrawLeftHand() //The void to spawn in the left platform.
                    {
                        jump_left = GameObject.CreatePrimitive(PrimitiveType.Cube); //Makes the GameObject into a cube.
                        jump_left.GetComponent<Renderer>().material.SetColor("_Color", Color.black); //Sets colour to black... or it would burn your eyes lol.
                        jump_left.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f); //Sets scale.

                        object[] left_form_1 = new object[] { __instance.leftHandTransform.position, __instance.leftHandTransform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_photoncode, left_form_1, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                    }

                    void DrawRightHand() //The void to spawn in the right platform.
                    {
                        jump_right = GameObject.CreatePrimitive(PrimitiveType.Cube); //Makes the GameObject into a cube.
                        jump_right.GetComponent<Renderer>().material.SetColor("_Color", Color.black); //Sets colour to black... or it would burn your eyes lol.
                        jump_right.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f); //Sets scale.

                        object[] right_form_1 = new object[] { new Vector3(0, (float)-0.075, 0) + __instance.rightHandTransform.position, __instance.rightHandTransform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_photoncode, right_form_1, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                    }
                }
            }

            static private void PlatformNetwork(EventData eventData)
            {
                byte eventCode = eventData.Code;
                if(eventCode == (byte)PhotonEventCodes.left_jump_photoncode)
                {
                    object[] data_left = (object[])eventData.CustomData; //Array
                    if(jump_left_network1 == null)
                    {
                        jump_left_network1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network1.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network1.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network1.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network1.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if(jump_left_network2 == null)
                    {
                        jump_left_network2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network2.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network2.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network2.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network2.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network3 == null)
                    {
                        jump_left_network3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network3.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network3.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network3.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network3.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network4 == null)
                    {
                        jump_left_network4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network4.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network4.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network4.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network4.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network5 == null)
                    {
                        jump_left_network5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network5.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network5.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network5.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network5.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network6 == null)
                    {
                        jump_left_network6 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network6.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network6.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network6.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network6.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network7 == null)
                    {
                        jump_left_network7 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network7.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network7.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network7.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network7.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network8 == null)
                    {
                        jump_left_network8 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network8.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network8.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network8.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network8.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else if (jump_left_network9 == null)
                    {
                        jump_left_network9 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_left_network9.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_left_network9.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_left_network9.transform.position = (Vector3)data_left[0]; //Sets position.
                        jump_left_network9.transform.rotation = (Quaternion)data_left[1]; //Sets rotation.
                    }
                    else
                    {
                        Console.WriteLine("An error occurred: Not enough game objects are currently available for the left hand (AirJump)");
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_photoncode)
                {
                    object[] data_right = (object[])eventData.CustomData; //Array
                    if(jump_right_network1 == null)
                    {
                        jump_right_network1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network1.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network1.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network1.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network1.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if(jump_right_network2 == null)
                    {
                        jump_right_network2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network2.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network2.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network2.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network2.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network3 == null)
                    {
                        jump_right_network3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network3.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network3.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network3.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network3.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network4 == null)
                    {
                        jump_right_network4 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network4.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network4.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network4.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network4.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network5 == null)
                    {
                        jump_right_network5 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network5.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network5.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network5.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network5.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network6 == null)
                    {
                        jump_right_network6 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network6.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network6.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network6.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network6.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network7 == null)
                    {
                        jump_right_network7 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network7.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network7.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network7.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network7.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network8 == null)
                    {
                        jump_right_network8 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network8.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network8.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network8.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network8.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else if (jump_right_network9 == null)
                    {
                        jump_right_network9 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        jump_right_network9.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                        jump_right_network9.transform.localScale = new Vector3(0.01f, 0.3f, 0.4f);
                        jump_right_network9.transform.position = (Vector3)data_right[0]; //Sets position.
                        jump_right_network9.transform.rotation = (Quaternion)data_right[1]; //Sets rotation.
                    }
                    else
                    {
                        Console.WriteLine("An error occurred: Not enough game objects are currently available for the right hand (AirJump)");
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.left_jump_deletion)
                {
                    GameObject.Destroy(jump_left_network1);
                    GameObject.Destroy(jump_left_network2);
                    GameObject.Destroy(jump_left_network3);
                    GameObject.Destroy(jump_left_network4);
                    GameObject.Destroy(jump_left_network5);
                    GameObject.Destroy(jump_left_network6);
                    GameObject.Destroy(jump_left_network7);
                    GameObject.Destroy(jump_left_network8);
                    GameObject.Destroy(jump_left_network9);
                    jump_left_network1 = null;
                    jump_left_network2 = null;
                    jump_left_network3 = null;
                    jump_left_network4 = null;
                    jump_left_network5 = null;
                    jump_left_network6 = null;
                    jump_left_network7 = null;
                    jump_left_network8 = null;
                    jump_left_network9 = null;

                    if (jump_left != null) //Oh nooo, we deleted all cubes and now there is no cubes left... HA! Just replace them :)
                    {
                        object[] left_form_2 = new object[] { jump_left.transform.position, jump_right.transform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.left_jump_photoncode, left_form_2, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                    }
                }
                else if (eventCode == (byte)PhotonEventCodes.right_jump_deletion)
                {
                    GameObject.Destroy(jump_right_network1);
                    GameObject.Destroy(jump_right_network2);
                    GameObject.Destroy(jump_right_network3);
                    GameObject.Destroy(jump_right_network4);
                    GameObject.Destroy(jump_right_network5);
                    GameObject.Destroy(jump_right_network6);
                    GameObject.Destroy(jump_right_network7);
                    GameObject.Destroy(jump_right_network8);
                    GameObject.Destroy(jump_right_network9);
                    jump_right_network1 = null;
                    jump_right_network2 = null;
                    jump_right_network3 = null;
                    jump_right_network4 = null;
                    jump_right_network5 = null;
                    jump_right_network6 = null;
                    jump_right_network7 = null;
                    jump_right_network8 = null;
                    jump_right_network9 = null;

                    if (jump_right != null) //Oh nooo, we deleted all cubes and now there is no cubes left... HA! Just replace them :)
                    {
                        object[] right_form_2 = new object[] { jump_right.transform.position, jump_right.transform.rotation };

                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //OoOoOoOoO ReceiverGroup.Others, sends signal to other people with the mod :O

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.right_jump_photoncode, right_form_2, raiseEventOptions, SendOptions.SendReliable); //Networking, if anyone abuses this in a seprate mod, it will be removed.
                    }
                }
            }
        }
    }
}