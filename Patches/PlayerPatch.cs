using HarmonyLib;

namespace AirJump.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    class PlayerPatch
    {
        private static void Postfix(GorillaLocomotion.Player __instance)
        {
            /*
            if(Behaviours.AirJump.instance == null)
            {
                __instance.gameObject.AddComponent<Behaviours.AirJump>();
            }
            */
            __instance.gameObject.AddComponent<Behaviours.AirJump>();
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("Start", MethodType.Normal)]
    class RigPatch
	{
        private static void Postfix(VRRig __instance)
		{
            if (!__instance.isOfflineVRRig && !__instance.photonView.IsMine) {
                __instance.gameObject.AddComponent<Behaviours.PlayerLeft>();
			}
		}
	}
}
