using System;
using ComputerInterface.Interfaces;

namespace AirJump.ComputerInterface
{
    class AirJumpEntry : IComputerModEntry
    {
        public string EntryName => "AirJump";

        // This is the first view that is going to be shown if the user select you mod
        // The Computer Interface mod will instantiate your view 
        public Type EntryViewType => typeof(AirJumpView);
    }
}
