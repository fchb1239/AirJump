using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR;

namespace AirJump.Utils
{
    public class AirJumpInput
    {
        static List<InputType> pressedLeft = new List<InputType>();
        static List<InputType> pressedRight = new List<InputType>();

        public static float PullInputFloat(string name, bool isLeftHand)
        {
            return (float)Traverse.Create(ControllerInputPoller.instance).Field($"{(isLeftHand ? "leftController" : "rightController")}{name}").GetValue();
        }

        public static bool PullInputBool(string name, bool isLeftHand)
        {
            return PullInputFloat(name, isLeftHand) > 0.4f;
        }

        /// <summary>
        /// Returns true for the frame that the designated input has been pressed
        /// </summary>
        /// <param name="IsLeftHand">If it's left hand or not</param>
        /// <param name="inputType">What input type it is</param>
        /// <returns>bool</returns>
        public static bool GetInputDown(bool IsLeftHand, InputType inputType)
        {
            bool output = false;

            switch (inputType)
            {
                case InputType.Trigger:
                    if (PullInputFloat("IndexFloat", IsLeftHand) > 0.8f)
                        output = true;
                    break;
                case InputType.Grip:
                    output = PullInputBool("GripFloat", IsLeftHand);
                    break;
                case InputType.Primary:
                    output = PullInputBool("PrimaryButton", IsLeftHand);
                    break;
                case InputType.Secondary:
                    output = PullInputBool("SecondaryButton", IsLeftHand);
                    break;
                default:
                    Debug.Log("Input called is not supported - this should not happen.");
                    break;
            }

            // One frame checker
            if (output)
            {
                bool contains = false;

                if (IsLeftHand)
                {
                    if (!pressedLeft.Contains(inputType))
                        pressedLeft.Add(inputType);
                    else
                        contains = true;
                }
                else
                {
                    if (!pressedRight.Contains(inputType))
                        pressedRight.Add(inputType);
                    else
                        contains = true;
                }

                if (contains)
                    output = false;
            }
            else
            {
                if (IsLeftHand)
                {
                    if (pressedLeft.Contains(inputType))
                        pressedLeft.Remove(inputType);
                }
                else
                {
                    if (pressedRight.Contains(inputType))
                        pressedRight.Remove(inputType);
                }
            }

            return output;
        }

        /// <summary>
        /// Returns true if input is being held down
        /// </summary>
        /// <param name="IsLeftHand">If it's left hand or not</param>
        /// <param name="inputType">What input type it is</param>
        /// <returns>bool</returns>
        public static bool GetInput(bool IsLeftHand, InputType inputType)
        {
            bool output = false;

            switch (inputType)
            {
                case InputType.Trigger:
                    if (PullInputFloat("IndexFloat", IsLeftHand) > 0.8f)
                        output = true;
                    break;
                case InputType.Grip:
                    output = PullInputBool("GripFloat", IsLeftHand);
                    break;
                case InputType.Primary:
                    output = PullInputBool("PrimaryButton", IsLeftHand);
                    break;
                case InputType.Secondary:
                    output = PullInputBool("SecondaryButton", IsLeftHand);
                    break;
                default:
                    Debug.Log("Input called is not supported - this should not happen.");
                    break;
            }

            return output;
        }
    }

    public enum InputType
    {
        Trigger,
        Grip,
        Primary,
        Secondary
    }
}
