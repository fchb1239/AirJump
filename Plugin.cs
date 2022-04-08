using HarmonyLib;
using BepInEx;
using Bepinject;
using System.Reflection;
using Utilla;
using System.ComponentModel;

namespace AirJump
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    [Description("HauntedModMenu")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            new Harmony(PluginInfo.GUID).PatchAll(Assembly.GetExecutingAssembly());
            Zenjector.Install<ComputerInterface.MainInstaller>().OnProject();
        }


        void OnEnable()
        {
            try
            { 
                Behaviours.AirJump.instance.UpdateEnabled();
                ComputerInterface.AirJumpView.instance.UpdateScreen();
            }
            catch { }
        }

        void OnDisable()
        {
            try
            {
                Behaviours.AirJump.instance.UpdateEnabled();
                ComputerInterface.AirJumpView.instance.UpdateScreen();
            }
            catch { }
        }

        [ModdedGamemodeJoin]
        void JoinModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = true;

            if (ComputerInterface.AirJumpView.instance != null)
                ComputerInterface.AirJumpView.instance.UpdateScreen();
        }

        [ModdedGamemodeLeave]
        void LeaveModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = false;
            Behaviours.AirJump.instance.LeaveModded();

            if (ComputerInterface.AirJumpView.instance != null)
                ComputerInterface.AirJumpView.instance.UpdateScreen();
        }
    }
}
