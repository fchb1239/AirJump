using UnityEngine;
using Photon.Pun;

namespace AirJump.Behaviours
{
	public class PlayerLeft : MonoBehaviour
	{
		private string userID;

		void Awake()
		{
			userID = GetComponent<PhotonView>()?.Owner?.UserId;
		}

		void OnDestroy()
		{
			AirJump.instance.PlayerLeftRoom(userID);
		}
	}
}
