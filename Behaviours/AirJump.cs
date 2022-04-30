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

namespace AirJump.Behaviours
{
    class AirJump : MonoBehaviour
    {
        public static AirJump instance;

        private string fileLocation = string.Format("{0}/SaveData", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        private SaveData saveData;

        public bool modEnabled = false;
        public bool isInModdedRoom = false;
        private bool isLeftPressed = false;
        private bool isRightPressed = false;

        private bool onceRight = false;
        private bool onceLeft = false;

        public Settings settings;

        private GorillaLocomotion.Player player = GorillaLocomotion.Player.Instance;

        private GameObject leftJump;
        private GameObject rightJump;

        private Dictionary<string, GameObject> leftJumpNetwork = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> rightJumpNetwork = new Dictionary<string, GameObject>();

        private XRNode leftHandNode = XRNode.LeftHand;
        private XRNode rightHandNode = XRNode.RightHand;

        private Vector3[] sizes = new Vector3[] { new Vector3(0.0125f, 0.28f, 0.3825f), new Vector3(0.0125f, 0.42f, 0.57375f), new Vector3(0.0125f, 0.56f, 0.765f) };
        private Material[] materials = new Material[] { null, Resources.Load<Material>("objects/treeroom/materials/darkfur"), null, null, Resources.Load<Material>("objects/character/materials/ice"), null };

        void Awake()
        {
            instance = this;

            //Not tseted but should work better
            AJLog.Log("Setting mats");
            //materials[2] = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[2];
            //materials[3] = GorillaTagger.Instance.offlineVRRig.materialsToChangeTo[1];

            
            foreach (VRRig rig in GameObject.FindObjectsOfType(typeof(VRRig)))
            {
                if (rig.isOfflineVRRig)
                {
                    materials[2] = rig.materialsToChangeTo[2];
                    materials[3] = rig.materialsToChangeTo[1];
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
                    saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(fileLocation));
                    modEnabled = saveData.enabled;
                    UpdateSize(saveData.sizeIndex);
                    UpdateMat(saveData.matIndex);
                    settings.otherCollisions = saveData.otherCollisions;
                }
                else
                {
                    saveData.enabled = modEnabled;
                    saveData.sizeIndex = settings.sizeIndex;
                    saveData.matIndex = settings.matIndex;
                    saveData.otherCollisions = settings.otherCollisions;
                    Plugin.instance.enabled = modEnabled;
                }
            }
            catch
            {
                modEnabled = saveData.enabled;
                UpdateSize(saveData.sizeIndex);
                UpdateMat(saveData.matIndex);
                settings.otherCollisions = saveData.otherCollisions;
                Plugin.instance.enabled = modEnabled;
            }

            PhotonNetwork.NetworkingClient.EventReceived += NetworkJump;
        }

        void Update()
        {
            if (modEnabled && isInModdedRoom && VersionVerifier.instance.validVersion)
            {
                InputDevices.GetDeviceAtXRNode(leftHandNode).TryGetFeatureValue(CommonUsages.gripButton, out isLeftPressed);
                InputDevices.GetDeviceAtXRNode(rightHandNode).TryGetFeatureValue(CommonUsages.gripButton, out isRightPressed);

                if (isLeftPressed)
                {
                    if (!onceLeft)
                    {
                        leftJump.transform.position = player.leftHandTransform.position;
                        //leftJump.transform.rotation = Quaternion.Euler(0, -45, 0) * player.leftHandTransform.rotation;
                        leftJump.transform.rotation = player.leftHandTransform.rotation;

                        object[] leftJumpData = new object[] { player.leftHandTransform.position, player.leftHandTransform.rotation, settings.sizeIndex, settings.matIndex };
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
                        rightJump.transform.position = new Vector3(0, (float)-0.0075, 0) + player.rightHandTransform.position;
                        //rightJump.transform.rotation = Quaternion.Euler(0, 45, 0) * player.rightHandTransform.rotation;
                        rightJump.transform.rotation = player.rightHandTransform.rotation;

                        object[] rightJumpData = new object[] { player.rightHandTransform.position, player.rightHandTransform.rotation, settings.sizeIndex, settings.matIndex };
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

        public void LeaveModded()
        {
            leftJump.transform.position = new Vector3(0, -999, 0);
            rightJump.transform.position = new Vector3(0, -999, 0);
            foreach (GameObject obj in leftJumpNetwork.Values)
                GameObject.Destroy(obj);
            foreach (GameObject obj in rightJumpNetwork.Values)
                GameObject.Destroy(obj);
        }

        public void UpdateEnabled()
        {
            modEnabled = !modEnabled;
            if (!modEnabled)
            {
                leftJump.transform.position = new Vector3(0, -999, 0);
                rightJump.transform.position = new Vector3(0, -999, 0);
            }

            Plugin.instance.enabled = modEnabled;

            saveData.enabled = modEnabled;
            File.WriteAllText(fileLocation, JsonUtility.ToJson(saveData));
        }

        public void UpdateCollisions()
        {
            settings.otherCollisions = !settings.otherCollisions;
            saveData.otherCollisions = settings.otherCollisions;
            File.WriteAllText(fileLocation, JsonUtility.ToJson(saveData));

            foreach (GameObject obj in leftJumpNetwork.Values)
                obj.GetComponent<BoxCollider>().enabled = true;
            foreach (GameObject obj in rightJumpNetwork.Values)
                obj.GetComponent<BoxCollider>().enabled = true;
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

            saveData.sizeIndex = index;
            File.WriteAllText(fileLocation, JsonUtility.ToJson(saveData));
        }

        public void UpdateMat(int index)
        {
            switch (index)
            {
                case 0:
                    leftJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    rightJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                    break;
                case 5:
                    foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
                    {
                        if (vrrig.isMyPlayer)
                        {
                            leftJump.GetComponent<Renderer>().material = vrrig.mainSkin.material;
                            rightJump.GetComponent<Renderer>().material = vrrig.mainSkin.material;
                        }
                    }
                    break;
                default:
                    leftJump.GetComponent<Renderer>().material = materials[index];
                    rightJump.GetComponent<Renderer>().material = materials[index];
                    break;
            }

            settings.matIndex = index;

            if (isRightPressed || isLeftPressed)
            {
                object[] matData = new object[] { true, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, matData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            saveData.matIndex = index;
            File.WriteAllText(fileLocation, JsonUtility.ToJson(saveData));
        }

        void NetworkJump(EventData eventData)
        {
            byte eventCode = eventData.Code;

            try
            {
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
                                        rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = GorillaGameManager.instance.FindVRRigForPlayer(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender)).GetComponent<VRRig>().mainSkin.material;
                                        break;
                                    default:
                                        rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
                                        break;
                                }
                            }
                            if (leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                            {
                                switch ((int)data[1])
                                {
                                    case 0:
                                        leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                        break;
                                    case 5:
                                        leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = GorillaGameManager.instance.FindVRRigForPlayer(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender)).GetComponent<VRRig>().mainSkin.material;
                                        break;
                                    default:
                                        leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
                                        break;
                                }
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
                        //just incase
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
            obj.transform.localScale = sizes[sizeIndex];
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
                    obj.GetComponent<Renderer>().material = GorillaGameManager.instance.FindVRRigForPlayer(player).GetComponent<VRRig>().mainSkin.material;
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
