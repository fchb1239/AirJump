using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

using AirJump.Logging;
using AirJump.Data;
//using AirJump.Helpers;

namespace AirJump.Behaviours
{
    class AirJump : MonoBehaviour
    {
        public static AirJump instance;

        private string fileLocation = string.Format("{0}\\SaveData", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        private string folderLocation = string.Format("{0}\\", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

        //private Material customMaterial = new Material(Shader.Find("Standard"));

        public bool isInModdedRoom = false;
        private bool isLeftPressed = false;
        private bool isRightPressed = false;

        private bool onceRight = false;
        private bool onceLeft = false;

        public Settings settings = new Settings();

        private GorillaLocomotion.Player player = GorillaLocomotion.Player.Instance;

        private GameObject leftJump;
        private GameObject rightJump;

        private Dictionary<string, GameObject> leftJumpNetwork = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> rightJumpNetwork = new Dictionary<string, GameObject>();

        private XRNode leftHandNode = XRNode.LeftHand;
        private XRNode rightHandNode = XRNode.RightHand;

        private Vector3[] sizes = new Vector3[] { new Vector3(0.0125f, 0.28f, 0.3825f), new Vector3(0.0125f, 0.42f, 0.57375f), new Vector3(0.0125f, 0.56f, 0.765f) };
        private Material[] materials = new Material[] { null, null, null, null, null, null };

        void Awake()
        {
            instance = this;

            try
            {
                //customMaterial.mainTexture = AirJumpImageLoader.LoadImage($"{folderLocation}\\Custom.png", FilterMode.Point, 465, 1260);
            }
            catch (Exception e)
            {
                AJLog.Log("Failed to load custom texture: " + e.ToString());
            }

            //Not tseted but should work better
            AJLog.Log("Setting mats");
            //materials[2] = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[2];
            //materials[3] = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[1];

            
            foreach (VRRig rig in FindObjectsOfType<VRRig>())
            {
                if (rig.isOfflineVRRig)
                {
                    materials[1] = rig.materialsToChangeTo[0];
                    materials[2] = rig.materialsToChangeTo[2];
                    materials[3] = rig.materialsToChangeTo[1];
                    materials[4] = rig.materialsToChangeTo[3];
                }
            }
            

            AJLog.Log("Creating jumps");
            leftJump = CreateJump();
            rightJump = CreateJump();

            AJLog.Log("Doing file thing");
            try
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
            catch
            {
                settings.enabled = true;
            }
        }

        void Update()
        {
            if (settings.enabled && isInModdedRoom && VersionVerifier.validVersion)
            {
                InputDevices.GetDeviceAtXRNode(leftHandNode).TryGetFeatureValue(CommonUsages.gripButton, out isLeftPressed);
                InputDevices.GetDeviceAtXRNode(rightHandNode).TryGetFeatureValue(CommonUsages.gripButton, out isRightPressed);

                if (isLeftPressed)
                {
                    if (!onceLeft)
                    {
                        VRRig vrRig = FindObjectOfType<VRRig>();

                        foreach (VRRig rig in FindObjectsOfType<VRRig>())
                        {
                            if (rig.isMyPlayer)
                                vrRig = rig;
                        }

                        leftJump.transform.parent = vrRig.transform;
                        leftJump.transform.localScale = sizes[settings.sizeIndex];
                        leftJump.transform.parent = null;

                        leftJump.transform.position = player.leftControllerTransform.position + (Vector3.down * 0.05f) * vrRig.scaleFactor;
                        //leftJump.transform.rotation = Quaternion.Euler(0, -45, 0) * player.leftHandTransform.rotation;
                        leftJump.transform.rotation = player.leftControllerTransform.rotation;

                        object[] leftJumpData = new object[] { leftJump.transform.position, leftJump.transform.rotation, settings.sizeIndex, settings.matIndex };
                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.LeftJump, leftJumpData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                        onceLeft = true;
                    }
                }
                else
                {
                    if (onceLeft)
                    {
                        leftJump.transform.position = new Vector3(0, -999, 0);

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.LeftJumpDeletion, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                        onceLeft = false;
                    }
                }

                if (isRightPressed)
                {
                    if (!onceRight)
                    {
                        VRRig vrRig = FindObjectOfType<VRRig>();

                        foreach (VRRig rig in FindObjectsOfType<VRRig>())
                        {
                            if (rig.isMyPlayer)
                                vrRig = rig;
                        }

                        rightJump.transform.parent = vrRig.transform;
                        rightJump.transform.localScale = sizes[settings.sizeIndex];
                        rightJump.transform.parent = null;

                        //rightJump.transform.position = new Vector3(0, (float)-0.0075, 0) + player.rightHandTransform.position;
                        rightJump.transform.position = player.rightControllerTransform.position + (new Vector3(0, (float)-0.0075, 0) + Vector3.down * 0.05f) * vrRig.scaleFactor;
                        //rightJump.transform.rotation = Quaternion.Euler(0, 45, 0) * player.rightHandTransform.rotation;
                        rightJump.transform.rotation = player.rightControllerTransform.rotation;

                        object[] rightJumpData = new object[] { rightJump.transform.position, rightJump.transform.rotation, settings.sizeIndex, settings.matIndex };
                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RightJump, rightJumpData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                        onceRight = true;
                    }
                }
                else
                {
                    if (onceRight)
                    {
                        rightJump.transform.position = new Vector3(0, -999, 0);

                        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RightJumpDeletion, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);

                        onceRight = false;
                    }
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

            leftJump.transform.position = new Vector3(0, -999, 0);
            rightJump.transform.position = new Vector3(0, -999, 0);
            foreach (GameObject obj in leftJumpNetwork.Values)
                GameObject.Destroy(obj);
            foreach (GameObject obj in rightJumpNetwork.Values)
                GameObject.Destroy(obj);
        }

        public void UpdateEnabled(bool enable)
        {
            settings.enabled = enable;
            if (!settings.enabled)
            {
                leftJump.transform.position = new Vector3(0, -999, 0);
                rightJump.transform.position = new Vector3(0, -999, 0);
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

            if (isRightPressed || isLeftPressed)
            {
                object[] sizeData = new object[] { false, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, sizeData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            settings.sizeIndex = index;
            SaveSettings();
        }

        public void UpdateMat(int index)
        {
            switch (index)
            {
                case 0:
                    // Color!! Americans, am I right? xD
                    leftJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    rightJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    break;
                case 5:
                    //leftJump.GetComponent<Renderer>().material = customMaterial;
                    //rightJump.GetComponent<Renderer>().material = customMaterial;
                    break;
                default:
                    leftJump.GetComponent<Renderer>().material = materials[index];
                    rightJump.GetComponent<Renderer>().material = materials[index];
                    break;
            }

            settings.matIndex = index;

            AssignSurfaceOverride(leftJump, index);
            AssignSurfaceOverride(rightJump, index);

            if (isRightPressed || isLeftPressed)
            {
                object[] matData = new object[] { true, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, matData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            settings.matIndex = index;

            SaveSettings();
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

            switch (matIndex)
            {
                case 0:
                    obj.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    break;
                case 5:
                    //obj.GetComponent<Renderer>().material = GorillaGameManager.instance.FindVRRigForPlayer(player).GetComponent<VRRig>().mainSkin.material;
                    //obj.GetComponent<Renderer>().material = customMaterial;
                    break;
                default:
                    obj.GetComponent<Renderer>().material = materials[matIndex];
                    break;
            }

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
