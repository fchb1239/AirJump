using AirJump.Utils;
using HarmonyLib;

namespace AirJump.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    class PlayerPatch
    {
        private static void Postfix(GorillaLocomotion.Player __instance)
        {
            //__instance.gameObject.AddComponent<Behaviours.VersionVerifier>();
            __instance.gameObject.AddComponent<Behaviours.AirJumpController>();
            AirJumpNetworkUtils.Register(); // Could probably be done in Plugin Awake but I've had bad experiences with Plugin Awake I won't forgive
        }
    }
}
