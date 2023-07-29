using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using AirJump.Logging;
using AirJump.Data;

namespace AirJump.Behaviours
{
    class AirJump : MonoBehaviour
    {
        public static AirJump instance;
        private string fileLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SaveData");

        public bool isInModdedRoom = false;
        private bool onceRight = false, onceLeft = false;

        public Settings settings = new Settings();

        private GameObject leftJump, rightJump;

        private Dictionary<string, GameObject> leftJumpNetwork = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> rightJumpNetwork = new Dictionary<string, GameObject>();

        private XRNode leftHandNode = XRNode.LeftHand;
        private XRNode rightHandNode = XRNode.RightHand;

        private Vector3[] sizes = new Vector3[] { new Vector3(0.0125f, 0.28f, 0.3825f), new Vector3(0.0125f, 0.42f, 0.57375f), new Vector3(0.0125f, 0.56f, 0.765f) };
        private Material[] materials = new Material[6];

        void Awake()
        {
            instance = this;
            try
            {
                AJLog.Log("Setting mats");
                foreach (VRRig rig in FindObjectsOfType<VRRig>())
                {
                    if (rig.isOfflineVRRig)
                    {
                        for (int i = 1; i <= 4; i++)
                            materials[i] = rig.materialsToChangeTo[i-1];
                    }
                }

                AJLog.Log("Creating jumps");
                leftJump = CreateJump();
                rightJump = CreateJump();

                AJLog.Log("Loading settings");
                LoadSettings();
            }
            catch
            {
                settings.enabled = true;
            }
        }

        void Update()
        {
            if (settings.enabled && isInModdedRoom && VersionVerifier.validVersion)
            {
                ProcessJump(leftHandNode, ref onceLeft, leftJump);
                ProcessJump(rightHandNode, ref onceRight, rightJump);
            }
        }

        private VRRig FindPlayerRig()
        {
            VRRig vrRig = FindObjectOfType<VRRig>();

            foreach (VRRig rig in FindObjectsOfType<VRRig>())
            {
                if (rig.isMyPlayer)
                    vrRig = rig;
            }

            return vrRig;
        }

        private void ProcessJump(XRNode handNode, ref bool once, GameObject jump)
        {
            InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.gripButton, out bool isPressed);

            if (isPressed)
            {
                if (!once)
                {
                    VRRig vrRig = FindPlayerRig();

                    jump.transform.parent = vrRig.transform;
                    jump.transform.localScale = sizes[settings.sizeIndex];
                    jump.transform.parent = null;

                    jump.transform.position = vrRig.leftControllerTransform.position + (Vector3.down * 0.05f) * vrRig.scaleFactor;
                    jump.transform.rotation = vrRig.leftControllerTransform.rotation;

                    object[] jumpData = new object[] { jump.transform.position, jump.transform.rotation, settings.sizeIndex, settings.matIndex };
                    PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.LeftJump, jumpData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                    once = true;
                }
            }
            else
            {
                if (once)
                {
                    jump.transform.position = new Vector3(0, -999, 0);

                    PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.LeftJumpDeletion, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                    once = false;
                }
            }
        }

        public void JoinModded()
        {
            PhotonNetwork.NetworkingClient.EventReceived += NetworkJump;
        }

        public void LeaveModded()
        {
            PhotonNetwork.NetworkingClient.EventReceived -= NetworkJump;

            ResetJump(leftJump);
            ResetJump(rightJump);

            DestroyAll(leftJumpNetwork);
            DestroyAll(rightJumpNetwork);
        }

        private void ResetJump(GameObject jump)
        {
            jump.transform.position = new Vector3(0, -999, 0);
        }

        private void DestroyAll(Dictionary<string, GameObject> jumpNetwork)
        {
            foreach (GameObject obj in jumpNetwork.Values)
                Destroy(obj);
            jumpNetwork.Clear();
        }

        public void UpdateEnabled(bool enable)
        {
            settings.enabled = enable;
            if (!settings.enabled)
            {
                ResetJump(leftJump);
                ResetJump(rightJump);
            }
            Plugin.instance.enabled = enable;
            SaveSettings();
        }

        public void UpdateCollisions()
        {
            settings.otherCollisions = !settings.otherCollisions;
            SaveSettings();

            foreach (GameObject obj in leftJumpNetwork.Values)
                obj.GetComponent<BoxCollider>().enabled = settings.otherCollisions;
            foreach (GameObject obj in rightJumpNetwork.Values)
                obj.GetComponent<BoxCollider>().enabled = settings.otherCollisions;
        }

        public void UpdateSize(int index)
        {
            leftJump.transform.localScale = sizes[index];
            rightJump.transform.localScale = sizes[index];

            settings.sizeIndex = index;

            if (onceRight || onceLeft)
            {
                object[] sizeData = new object[] { false, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, sizeData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            settings.sizeIndex = index;
            SaveSettings();
        }

        public void UpdateMat(int index)
        {
            ApplyMaterial(leftJump, index);
            ApplyMaterial(rightJump, index);

            settings.matIndex = index;

            if (onceRight || onceLeft)
            {
                object[] matData = new object[] { true, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, matData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            settings.matIndex = index;
            SaveSettings();
        }

        private void ApplyMaterial(GameObject jump, int index)
        {
            switch (index)
            {
                case 0:
                    jump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    break;
                case 5:
                    //jump.GetComponent<Renderer>().material = customMaterial;
                    break;
                default:
                    jump.GetComponent<Renderer>().material = materials[index];
                    break;
            }

            AssignSurfaceOverride(jump, index);
        }

        public void AssignSurfaceOverride(GameObject jump, int index)
        {
            if (jump.GetComponent<GorillaSurfaceOverride>() == null)
                jump.AddComponent<GorillaSurfaceOverride>();

            GorillaSurfaceOverride surface = jump.GetComponent<GorillaSurfaceOverride>();

            switch (index)
            {
                case 1:
                    surface.overrideIndex = 4;
                    break;
                case 2:
                case 3:
                    surface.overrideIndex = 114;
                    break;
                case 4:
                    surface.overrideIndex = 42;
                    break;
                default:
                    surface.overrideIndex = 81;
                    break;
            }
        }

        public void SaveSettings()
        {
            File.WriteAllText(fileLocation, JsonUtility.ToJson(settings));
        }

        public void LoadSettings()
        {
            if (File.Exists(fileLocation))
            {
                settings = JsonUtility.FromJson<Settings>(File.ReadAllText(fileLocation));
                UpdateSize(settings.sizeIndex);
                UpdateMat(settings.matIndex);
            }
            else
                settings.enabled = true;
        }

        void NetworkJump(EventData eventData)
        {
            byte eventCode = eventData.Code;

            try
            {
                // Prevent cheating
                if (!isInModdedRoom)
                    return;

                object[] data = null;

                if (eventData.CustomData != null)
                    data = (object[])eventData.CustomData;

                switch (eventCode)
                {
                    case (byte)PhotonEventCodes.LeftJump:
                        if (!leftJumpNetwork.ContainsKey(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId))
                            leftJumpNetwork.Add(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId, CreateJumpNetwork((Vector3)data[0], (Quaternion)data[1], (int)data[2], (int)data[3], PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender), settings.otherCollisions));
                        break;
                    case (byte)PhotonEventCodes.RightJump:
                        if (!rightJumpNetwork.ContainsKey(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId))
                            rightJumpNetwork.Add(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId, CreateJumpNetwork((Vector3)data[0], (Quaternion)data[1], (int)data[2], (int)data[3], PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender), settings.otherCollisions));
                        break;
                    case (byte)PhotonEventCodes.LeftJumpDeletion:
                        GameObject.Destroy(leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId]);
                        leftJumpNetwork.Remove(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId);
                        break;
                    case (byte)PhotonEventCodes.RightJumpDeletion:
                        GameObject.Destroy(rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId]);
                        rightJumpNetwork.Remove(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId);
                        break;
                    case (byte)PhotonEventCodes.UpdateJump:
                        if ((bool)data[0])
                        {
                            if (rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                            {
                                switch ((int)data[1])
                                {
                                    case 0:
                                        rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                        break;
                                    case 5:
                                        //rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = customMaterial;
                                        break;
                                    default:
                                        rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
                                        break;
                                }

                                AssignSurfaceOverride(rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId], (int)data[1]);
                            }
                            if (leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                            {
                                switch ((int)data[1])
                                {
                                    case 0:
                                        leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                        break;
                                    case 5:
                                        //leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = customMaterial;
                                        break;
                                    default:
                                        leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
                                        break;
                                }

                                AssignSurfaceOverride(leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId], (int)data[1]);
                            }
                        }
                        else
                        {

                            if (rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                                rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].transform.localScale = sizes[(int)data[1]];
                            if (leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                                leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].transform.localScale = sizes[(int)data[1]];
                        }
                        break;
                    default:
                        //just in case
                        break;
                }
            }
            catch { }
        }

        GameObject CreateJump()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            obj.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
            return obj;
        }

        GameObject CreateJumpNetwork(Vector3 position, Quaternion rotation, int sizeIndex, int matIndex, Photon.Realtime.Player player, bool otherCol)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.parent = GorillaGameManager.instance.FindVRRigForPlayer(player).transform;
            obj.transform.localScale = sizes[sizeIndex];
            obj.transform.parent = null;
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            //Highly untested
            if (!otherCol)
                obj.GetComponent<BoxCollider>().enabled = false;

            ApplyMaterial(obj, matIndex);

            return obj;
        }
    }

    public enum PhotonEventCodes
    {
        LeftJump = 80,
        RightJump = 81,
        LeftJumpDeletion = 82,
        RightJumpDeletion = 83,
        UpdateJump = 84
    }
}
