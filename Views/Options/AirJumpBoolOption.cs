using UnityEngine;

namespace AirJump.Views.Options
{
    public class AirJumpBoolOption : AirJumpOption<int> // Couldn't have an list with both ints and bools
    {
        public bool On;

        public override void OnChange(int i)
        {
            On = !On;
            PlayerPrefs.SetInt($"AirJump{OptionName}", On ? 1 : 0);
        }

        public override string Status()
        {
            return $"<color={(On ? "green" : "blue")}>{On}</color>";
        }

        public override int Value()
        {
            return On ? 1 : 0;
        }

        public AirJumpBoolOption(string name, bool defaultOn)
        {
            OptionName = name;

            On = PlayerPrefs.GetInt($"AirJump{OptionName}", defaultOn ? 1 : 0) > 0;

            OptionType = "bool";
        }
    }
}
