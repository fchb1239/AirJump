using HarmonyLib;

namespace AirJump.Patches
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    class PlayerPatch
    {
        private static void Postfix(GorillaLocomotion.Player __instance)
        {
            __instance.gameObject.AddComponent<Behaviours.VersionVerifier>();
            __instance.gameObject.AddComponent<Behaviours.AirJump>();
        }
    }
}
