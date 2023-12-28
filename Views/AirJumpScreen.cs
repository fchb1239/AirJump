using ComputerPlusPlus;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using AirJump.Views.Options;
using System.Text;

namespace AirJump.Views
{
    public class AirJumpScreen : IScreen
    {
        public static int currentOption;
        public static List<AirJumpOption<int>> options = new List<AirJumpOption<int>>
        {
            new AirJumpBoolOption("Enabled", true),
            new AirJumpIntOption("Size", Constants.Sizes.Length - 1, "Normal", "Medium", "Large"),
            new AirJumpIntOption("Material", 5, "Normal", "Fur", "Lava", "Rock", "Ice"),
            //new AirJumpBoolOption("Collisions", false)
        };

        public string Title => "AirJump";

        public string Description => "Made by <color=blue>fchb1239</color>";

        public string GetContent()
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < options.Count; i++)
            {
                string infront = "";
                if (i == currentOption)
                    infront = ">";
                builder.AppendLine($"{infront}{options[i].OptionName}: {options[i].Status()}");
            }

            if (!Plugin.inModded)
            {
                builder.AppendLine();
                builder.AppendLine("<color=red>Not in a modded room</color>");
            }
            return builder.ToString();
        }

        public void OnKeyPressed(GorillaKeyboardButton button)
        {
            if (button.characterString.StartsWith("option"))
            {
                currentOption = int.Parse(button.characterString.Replace("option", "")) - 1;
                if (currentOption >= options.Count)
                    currentOption = options.Count - 1;
                return;
            }

            if (int.TryParse(button.characterString, out int result))
            {
                if (options[currentOption].OptionType == "int")
                {
                    options[currentOption].OnChange(result - 1);
                    return;
                }
            }

            if (button.characterString == "enter")
            {
                if (options[currentOption].OptionType == "bool")
                {
                    options[currentOption].OnChange(0);
                    return;
                }
            }
        }

        public void Start()
        {
            Console.WriteLine("AirJump screen registered");
        }
    }
}
