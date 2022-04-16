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
        string[] fileArray = new string[4];

        public bool modEnabled;
        public bool isInModdedRoom;
        public bool otherCollisions;
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
                otherCollisions = bool.Parse(fileArray[3]);
                UpdateSize(int.Parse(fileArray[1]));
                UpdateMat(int.Parse(fileArray[2]));
            }
            else
            {
                modEnabled = false;
                otherCollisions = true;
                currentSizeIndex = 0;
                currentMaterialIndex = 0;
                fileArray[0] = modEnabled.ToString();
                fileArray[1] = currentSizeIndex.ToString();
                fileArray[2] = currentMaterialIndex.ToString();
            }

            PhotonNetwork.AddCallbackTarget(this);
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
                        rightJump.transform.position = new Vector3(0, -0.0075f, 0) + player.rightHandTransform.position;
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

        public void UpdateEnabled(bool? toEnable = null)
        {
            modEnabled = (toEnable.HasValue ? toEnable.Value : !modEnabled);

            if (!modEnabled) {
                leftJump.transform.position = new Vector3(0, -999, 0);
                rightJump.transform.position = new Vector3(0, -999, 0);
                UpdateCollisions(false);

            } else {
                UpdateCollisions(modEnabled && isInModdedRoom && otherCollisions);
            }

            fileArray[0] = modEnabled.ToString();
            File.WriteAllText(fileLocation, string.Join(",", fileArray));
        }

        public  void ToggleCollisions(bool? toEnable = null)
		{
            otherCollisions = toEnable ?? !otherCollisions;
            UpdateCollisions(otherCollisions);

            fileArray[3] = otherCollisions.ToString();
            File.WriteAllText(fileLocation, string.Join(",", fileArray));
        }

        public void UpdateCollisions(bool toEnable)
		{
            try {
                foreach (var platform in leftJumpNetwork.Values) {
                    platform.GetComponent<Collider>().enabled = toEnable;
                }

                foreach (var platform in rightJumpNetwork.Values) {
                    platform.GetComponent<Collider>().enabled = toEnable;
				}

            } catch { }  
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

                if (eventData.CustomData != null) {
                    data = (object[])eventData.CustomData;
                
                } 

                switch (eventCode)
                {
                    case (byte)PhotonEventCodes.LeftJump: {
                        UpdateNetworkedPlatform(data, eventData.Sender, leftJumpNetwork);
                        break;
                    }

                    case (byte)PhotonEventCodes.RightJump: {
                        UpdateNetworkedPlatform(data, eventData.Sender, rightJumpNetwork);
                        break;
                    }

                    case (byte)PhotonEventCodes.LeftJumpDeletion: {
                        if (leftJumpNetwork.TryGetValue(PhotonNetwork.CurrentRoom?.GetPlayer(eventData.Sender)?.UserId, out var platform)) {
                            platform?.SetActive(false);
                        }
                        break;
                    }

                    case (byte)PhotonEventCodes.RightJumpDeletion: {
                        if (rightJumpNetwork.TryGetValue(PhotonNetwork.CurrentRoom?.GetPlayer(eventData.Sender)?.UserId, out var platform)) {
                            platform?.SetActive(false);
						}
                        break;
					}                        

                    case (byte)PhotonEventCodes.UpdateJump: {
                        string userKey = PhotonNetwork.CurrentRoom?.GetPlayer(eventData.Sender)?.UserId;

                        if (data[1] is int index && leftJumpNetwork.TryGetValue(userKey, out var leftPlatform) && rightJumpNetwork.TryGetValue(userKey, out var rightPlatform)) {
                            if ((bool)data[0]) {
                                    SetPlatformMaterial(ref rightPlatform, index);
                                    SetPlatformMaterial(ref leftPlatform, index);

                            } else {
                                rightPlatform.transform.localScale = sizes[index];
                                leftPlatform.transform.localScale = sizes[index];
                            }
                        }
                        break;
                    }

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
            GameObject platformObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformObject.GetComponent<Collider>().enabled = otherCollisions && isInModdedRoom && modEnabled;
            SetJumpNetwork(ref platformObject, position, rotation, sizeIndex, matIndex);

            return platformObject;
        }

        void SetJumpNetwork(ref GameObject platformObject, Vector3 position, Quaternion rotation, int sizeIndex, int matIndex)
        {
            //GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platformObject.transform.localScale = sizes[sizeIndex];
            platformObject.transform.position = position;
            platformObject.transform.rotation = rotation;

            SetPlatformMaterial(ref platformObject, matIndex);
        }

        void SetPlatformMaterial(ref GameObject platformObject, in int index)
		{
            var renderer = platformObject?.GetComponent<Renderer>();
            if (index == 0) {
                renderer.material.SetColor("_Color", Color.black);

            } else {
                renderer.material = materials[index];
			}
		}

        private void UpdateNetworkedPlatform(in object[] platformData, in int senderID, Dictionary<string, GameObject> platformMap)
		{
            try {
                if (platformData?.Length < 4) {
                    Debug.Log("Platform information being sent is missing data");
				}

                string userKey = PhotonNetwork.CurrentRoom?.GetPlayer(senderID)?.UserId;
                
                if (platformData[0] is Vector3 pos && platformData[1] is Quaternion rot && platformData[2] is int platSize && platformData[3] is int platMat) {
                    if (platformMap.TryGetValue(userKey, out var platform)) {
                        SetJumpNetwork(ref platform, pos, rot, platSize, platMat);
                        if(!platform.activeSelf) {
                            platform.SetActive(true);
						}

					} else {
                        platformMap.Add(userKey, CreateJumpNetwork(pos, rot, platSize, platMat));
					}
				}
            
            } catch (Exception e) {
                Debug.Log(e.ToString());
			}
		}
        public void PlayerLeftRoom(string userID)
		{
            if (rightJumpNetwork.TryGetValue(userID, out var rightPlatform)) {
                GameObject.Destroy(rightPlatform);
                rightJumpNetwork.Remove(userID);
            }

            if (leftJumpNetwork.TryGetValue(userID, out var leftplatform)) {
                GameObject.Destroy(leftplatform);
                leftJumpNetwork.Remove(userID);
            }
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
