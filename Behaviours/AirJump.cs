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

namespace AirJump.Behaviours
{
    class AirJump : MonoBehaviour
    {
        public static AirJump instance;

        string fileLocation = string.Format("{0}/SaveData", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        string[] fileArray = new string[3];

        public bool modEnabled;
        public bool isInModdedRoom;
        bool isLeftPressed;
        bool isRightPressed;

        bool onceRight;
        bool onceLeft;

        public int currentSizeIndex = 0;
        public int currentMaterialIndex = 0;

        GorillaLocomotion.Player player = GorillaLocomotion.Player.Instance;

        GameObject leftJump;
        GameObject rightJump;

        Dictionary<string, GameObject> leftJumpNetwork = new Dictionary<string, GameObject>();
        Dictionary<string, GameObject> rightJumpNetwork = new Dictionary<string, GameObject>();

        XRNode leftHandNode = XRNode.LeftHand;
        XRNode rightHandNode = XRNode.RightHand;

        Vector3[] sizes = new Vector3[] { new Vector3(0.0125f, 0.28f, 0.3825f), new Vector3(0.0125f, 0.42f, 0.57375f), new Vector3(0.0125f, 0.56f, 0.765f) };
        Material[] materials = new Material[] { null, Resources.Load<Material>("objects/treeroom/materials/darkfur"), null, null, Resources.Load<Material>("objects/character/materials/ice") };

        void Awake()
        {
            instance = this;

            foreach (VRRig rig in GameObject.FindObjectsOfType(typeof(VRRig)))
            {
                if (rig.isOfflineVRRig)
                {
                    materials[2] = rig.materialsToChangeTo[2];
                    materials[3] = rig.materialsToChangeTo[1];
                }
            }

            leftJump = CreateJump();
            rightJump = CreateJump();

            if (File.Exists(fileLocation))
            {
                fileArray = File.ReadAllText(fileLocation).Split(',');
                modEnabled = bool.Parse(fileArray[0]);
                UpdateSize(int.Parse(fileArray[1]));
                UpdateMat(int.Parse(fileArray[2]));
            }
            else
            {
                modEnabled = false;
                currentSizeIndex = 0;
                currentMaterialIndex = 0;
                fileArray[0] = modEnabled.ToString();
                fileArray[1] = currentSizeIndex.ToString();
                fileArray[2] = currentMaterialIndex.ToString();
            }

            PhotonNetwork.NetworkingClient.EventReceived += NetworkJump;
        }

        void Update()
        {
            if (modEnabled && isInModdedRoom)
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

                        object[] leftJumpData = new object[] { player.leftHandTransform.position, player.leftHandTransform.rotation, currentSizeIndex, currentMaterialIndex };
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

                        object[] rightJumpData = new object[] { player.rightHandTransform.position, player.rightHandTransform.rotation, currentSizeIndex, currentMaterialIndex };
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

            fileArray[0] = modEnabled.ToString();
            File.WriteAllText(fileLocation, string.Join(",", fileArray));
        }

        public void UpdateSize(int index)
        {
            leftJump.transform.localScale = sizes[index];
            rightJump.transform.localScale = sizes[index];

            currentSizeIndex = index;

            if(isRightPressed || isLeftPressed)
            {
                object[] sizeData = new object[] { false, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, sizeData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            fileArray[1] = index.ToString();
            File.WriteAllText(fileLocation, string.Join(",", fileArray));
        }

        public void UpdateMat(int index)
        {
            if (index == 0)
            {
                leftJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                rightJump.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            }
            else
            {
                leftJump.GetComponent<Renderer>().material = materials[index];
                rightJump.GetComponent<Renderer>().material = materials[index];
            }

            currentMaterialIndex = index;

            if (isRightPressed || isLeftPressed)
            {
                object[] matData = new object[] { true, index };
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.UpdateJump, matData, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
            }

            fileArray[2] = index.ToString();
            File.WriteAllText(fileLocation, string.Join(",", fileArray));
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
                        leftJumpNetwork.Add(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId, CreateJumpNetwork((Vector3)data[0], (Quaternion)data[1], (int)data[2], (int)data[3]));
                        break;
                    case (byte)PhotonEventCodes.RightJump:
                        rightJumpNetwork.Add(PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId, CreateJumpNetwork((Vector3)data[0], (Quaternion)data[1], (int)data[2], (int)data[3]));
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
                            if(rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                            {
                                if((int)data[1] == 0)
                                    rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                else
                                    rightJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
                            }
                            if (leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId] != null)
                            {
                                if ((int)data[1] == 0)
                                    leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material.SetColor("_Color", Color.black);
                                else
                                    leftJumpNetwork[PhotonNetwork.CurrentRoom.GetPlayer(eventData.Sender).UserId].GetComponent<Renderer>().material = materials[(int)data[1]];
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
            } catch { }
        }

        GameObject CreateJump()
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            obj.transform.localScale = new Vector3(0.0125f, 0.28f, 0.3825f);
            return obj;
        }

        GameObject CreateJumpNetwork(Vector3 position, Quaternion rotation, int sizeIndex, int matIndex)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.transform.localScale = sizes[sizeIndex];
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            if (matIndex == 0)
                obj.GetComponent<Renderer>().material.SetColor("_Color", Color.black);
            else
                obj.GetComponent<Renderer>().material = materials[matIndex];

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
