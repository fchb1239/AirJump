using System.Collections.Generic;
using UnityEngine;

namespace AirJump.Views.Options
{
    public class AirJumpIntOption : AirJumpOption<int>
    {
        public int Max;
        public int CurrentNumber;
        public string[] HumanText;

        public override void OnChange(int i)
        {
            CurrentNumber = i;
            if (CurrentNumber > Max)
                CurrentNumber = Max;
            if (CurrentNumber < 0)
                CurrentNumber = Max;
            PlayerPrefs.SetInt($"AirJump{OptionName}", CurrentNumber);
        }

        public override string Status()
        {
            string text = HumanText[0];

            if (CurrentNumber < HumanText.Length)
                text = HumanText[CurrentNumber];

            return $"<color=blue>{text}</color>";
        }

        public override int Value()
        {
            return CurrentNumber;
        }

        public AirJumpIntOption(string name, int max, params string[] names)
        {
            OptionName = name;
            Max = max;
            //CurrentNumber = PlayerPrefs.GetInt($"AirJump{name}");
            // Doing OnChance to sanitise inputs
            OnChange(PlayerPrefs.GetInt($"AirJump{name}"));
            HumanText = names;
            OptionType = "int";
        }
    }
}
