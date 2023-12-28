namespace AirJump.Views
{
    public abstract class AirJumpOption<T>
    {
        public string OptionName;

        public string OptionType;

        public abstract void OnChange(int i);

        public abstract string Status();

        public abstract T Value();
    }
}
