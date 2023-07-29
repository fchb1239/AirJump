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
        public static Plugin instance;
        
        public void Awake()
        {
            instance = this;
            new Harmony(PluginInfo.GUID).PatchAll(Assembly.GetExecutingAssembly());
            Zenjector.Install<ComputerInterface.MainInstaller>().OnProject();
        }

        void OnEnable()
        {
            UpdateFeatureAndScreen(true);
        }

        void OnDisable()
        {
            UpdateFeatureAndScreen(false);
        }

        private void UpdateFeatureAndScreen(bool enabled)
        {
            try
            { 
                Behaviours.AirJump.instance.UpdateEnabled(enabled);
                ComputerInterface.AirJumpView.instance.UpdateScreen();
            }
            catch { }
        }

        [ModdedGamemodeJoin]
        void JoinModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = true;
            UpdateScreen();
        }

        [ModdedGamemodeLeave]
        void LeaveModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = false;
            Behaviours.AirJump.instance.LeaveModded();
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            if (ComputerInterface.AirJumpView.instance != null)
                ComputerInterface.AirJumpView.instance.UpdateScreen();
        }
    }
}
