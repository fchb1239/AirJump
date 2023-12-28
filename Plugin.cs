using System;
using System.Reflection;
using System.ComponentModel;

using HarmonyLib;
using BepInEx;

using Utilla;

namespace AirJump
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.1")]
    [BepInDependency("com.kylethescientist.gorillatag.computerplusplus", "1.0.1")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        public static bool inModded;

        public void Awake()
        {
            new Harmony(PluginInfo.GUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        void OnDisable()
        {
            Console.WriteLine("Please enable/disable via AirJump screen");
            enabled = true;
        }

        [ModdedGamemodeJoin]
        private void RoomJoined(string gamemode)
        {
            inModded = true;
        }

        [ModdedGamemodeLeave]
        private void RoomLeft(string gamemode)
        {
            inModded = false;
        }
    }
}
