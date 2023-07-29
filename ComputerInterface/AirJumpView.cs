using System;

using ComputerInterface;
using ComputerInterface.ViewLib;

using AirJump.Logging;

namespace AirJump.CI
{
    class AirJumpView : ComputerView
    {
        public static AirJumpView instance;
        private readonly UISelectionHandler selectionHandler;
        private const string highlightColour = "336BFF";
        private const int maxIndex = 3;
        private readonly string[] matNames = { "Normal", "Fur", "Lava", "Rock", "Ice" };
        private readonly string[] sizeNames = { "Normal", "Bigger", "Chonk" };

        public AirJumpView()
        {
            instance = this;

            selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            selectionHandler.MaxIdx = maxIndex;
            selectionHandler.OnSelected += OnEntrySelected;
            selectionHandler.ConfigureSelectionIndicator($"<color=#{highlightColour}>></color> ", "", "  ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            UpdateScreen();
        }

        public void UpdateScreen()
        {
            SetText(str =>
            {
                str.BeginCenter();
                str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
                str.AppendClr("AirJump", highlightColour).EndColor().AppendLine();
                str.AppendLine("By fchb1239");
                str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
                str.EndAlign().AppendLines(1);

                if (Behaviours.VersionVerifier.validVersion)
                {
                    str.AppendLine(selectionHandler.GetIndicatedText(0, $"<color={(Behaviours.AirJump.instance.settings.enabled ? string.Format("#{0}>[Enabled]", highlightColour) : "white>[Disabled]")}</color>"));
                    str.AppendLine(selectionHandler.GetIndicatedText(1, $"Material: <color=#{highlightColour}>{matNames[Behaviours.AirJump.instance.settings.matIndex]}</color>"));
                    str.AppendLine(selectionHandler.GetIndicatedText(2, $"Size: <color=#{highlightColour}>{sizeNames[Behaviours.AirJump.instance.settings.sizeIndex]}</color>"));
                    str.AppendLine(selectionHandler.GetIndicatedText(3, $"Other collisions: <color={(Behaviours.AirJump.instance.settings.otherCollisions ? string.Format("#{0}>[Enabled]", highlightColour) : "white>[Disabled]")}</color>"));

                    if (!Behaviours.AirJump.instance.isInModdedRoom)
                    {
                        str.AppendLines(2);
                        str.AppendClr("Please join a modded room!", "A01515").EndColor().AppendLine();
                    }
                }
                else
                {
                    str.AppendClr($"Old version detected! Please update!", "A01515").EndColor().AppendLine();
                    str.AppendClr($"Your version: {PluginInfo.Version}", "A01515").EndColor().AppendLine();
                    str.AppendClr($"Newest version: {Behaviours.VersionVerifier.newestVersion}", "A01515").EndColor().AppendLine();
                    str.AppendLine("");
                    str.AppendLine("");
                    str.AppendLine("");
                    str.AppendLine("");
                    str.AppendLine("The download link has been opened in your browser.");

                    AJLog.Log("Opening browser");
                    System.Diagnostics.Process.Start("https://github.com/fchb1239/AirJump/releases");
                }
            });
        }

        private void OnEntrySelected(int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Behaviours.AirJump.instance.UpdateEnabled(!Behaviours.AirJump.instance.settings.enabled);
                        break;
                    case 3:
                        Behaviours.AirJump.instance.UpdateCollisions();
                        break;
                }
                UpdateScreen();
            }
            catch (Exception e) { AJLog.Log(e.ToString()); }
        }

        private void OnEntryAdjusted(int index, bool increase)
        {
            try
            {
                int offset = increase ? 1 : -1;
                switch (index)
                {
                    case 1:
                        if (Behaviours.AirJump.instance.settings.matIndex == 5 && increase)
                            return;

                        Behaviours.AirJump.instance.UpdateMat(Behaviours.AirJump.instance.settings.matIndex + offset);
                        break;
                    case 2:
                        if (Behaviours.AirJump.instance.settings.sizeIndex == 2 && increase)
                            return;

                        Behaviours.AirJump.instance.UpdateSize(Behaviours.AirJump.instance.settings.sizeIndex + offset);
                        break;
                }
                UpdateScreen();
            }
            catch (Exception e) { AJLog.Log(e.ToString()); }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (selectionHandler.HandleKeypress(key))
            {
                UpdateScreen();
                return;
            }

            if (key == EKeyboardKey.Left || key == EKeyboardKey.Right)
            {
                OnEntryAdjusted(selectionHandler.CurrentSelectionIndex, key == EKeyboardKey.Right);
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Enter:
                    //ShowView<DetailsView>();
                    break;
            }
        }
    }
}
