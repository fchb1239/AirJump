using System;
using Utilla;
using ComputerInterface;
using ComputerInterface.ViewLib;

namespace AirJump.ComputerInterface
{
    class AirJumpView : ComputerView
    {
        public static AirJumpView instance;
        private readonly UISelectionHandler selectionHandler;
        const string highlightColour = "336BFF";
        public bool modEnabled;
        public int mat;
        public int size;

        string[] matNames = new string[] { "Normal", "Fur", "Lava", "Rock", "Ice" };
        string[] sizeNames = new string[] { "Normal", "Bigger", "Chonk" };

        public AirJumpView()
        {
            instance = this;

            selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);

            selectionHandler.MaxIdx = 2;

            selectionHandler.OnSelected += OnEntrySelected;

            selectionHandler.ConfigureSelectionIndicator($"<color=#{highlightColour}>></color> ", "", "  ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            // changing the Text property will fire an PropertyChanged event
            // which lets the computer know the text has changed and update it
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

                str.AppendLine(selectionHandler.GetIndicatedText(0, $"<color={(Behaviours.AirJump.instance.modEnabled ? string.Format("#{0}>[Enabled]", highlightColour) : "white>[Disabled]")}</color>"));
                str.AppendLine(selectionHandler.GetIndicatedText(1, $"Material: <color=#{highlightColour}>{matNames[Behaviours.AirJump.instance.currentMaterialIndex]}</color>"));
                str.AppendLine(selectionHandler.GetIndicatedText(2, $"Size: <color=#{highlightColour}>{sizeNames[Behaviours.AirJump.instance.currentSizeIndex]}</color>"));

                if (!Behaviours.AirJump.instance.isInModdedRoom)
                {
                    str.AppendLines(2);
                    str.AppendClr("Please join a modded room!", "A01515").EndColor().AppendLine();
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
                        Behaviours.AirJump.instance.UpdateEnabled();
                    UpdateScreen();
                    break;
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
        }

        private void OnEntryAdjusted(int index, bool increase)
        {
            try
            {
                int offset = increase ? 1 : -1;
                switch (index)
                {
                    case 1:
                        if (Behaviours.AirJump.instance.currentMaterialIndex == 4 && increase)
                            return;

                        Behaviours.AirJump.instance.UpdateMat(Behaviours.AirJump.instance.currentMaterialIndex + offset);
                        UpdateScreen();
                        break;
                    case 2:
                        if (Behaviours.AirJump.instance.currentSizeIndex == 2 && increase)
                            return;

                        Behaviours.AirJump.instance.UpdateSize(Behaviours.AirJump.instance.currentSizeIndex + offset);
                        UpdateScreen();
                        break;
                }
            }
            catch (Exception e) { Console.WriteLine(e); }
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
                UpdateScreen();
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
