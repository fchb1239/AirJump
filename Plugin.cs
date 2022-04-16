using HarmonyLib;
using BepInEx;
using Bepinject;
using System;
using System.Reflection;
using System.ComponentModel;
using Utilla;
using UnityEngine;

namespace AirJump
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [ModdedGamemode]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            new Harmony(PluginInfo.GUID).PatchAll(Assembly.GetExecutingAssembly());
            try {
                Zenjector.Install<ComputerInterface.MainInstaller>().OnProject();
            
            } catch (Exception e) {
                Debug.LogError(e.ToString());
			}
        }

        void OnEnable()
		{
            if (Behaviours.AirJump.instance != null && Behaviours.AirJump.instance.modEnabled == false) {
                Behaviours.AirJump.instance.UpdateEnabled(true);

                try {
                    UpdateScreen();

                } catch (Exception e) {
                    Debug.LogWarning(e.ToString());
                }
            }
		}

        void OnDisable()
		{
            if (Behaviours.AirJump.instance != null && Behaviours.AirJump.instance.modEnabled == true) {
                Behaviours.AirJump.instance.UpdateEnabled(false);

                try {
                    UpdateScreen();

                } catch (Exception e) {
                    Debug.LogWarning(e.ToString());
                }
            }
		}

        [ModdedGamemodeJoin]
        void JoinModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = true;

            try {
                UpdateScreen();
            
            } catch (Exception e) {
                Debug.LogWarning(e.ToString());
			}
        }

        [ModdedGamemodeLeave]
        void LeaveModded()
        {
            Behaviours.AirJump.instance.isInModdedRoom = false;
            Behaviours.AirJump.instance.LeaveModded();

            try {
                UpdateScreen();

            } catch (Exception e) {
                Debug.LogWarning(e.ToString());
            }
        }

        void UpdateScreen()
		{
            if (ComputerInterface.AirJumpView.instance != null)
                ComputerInterface.AirJumpView.instance.UpdateScreen();
        }
    }
}
