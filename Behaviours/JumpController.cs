using AirJump.Structs;
using AirJump.Utils;
using UnityEngine;

namespace AirJump.Behaviours
{
    public class JumpController
    {
        public Jump Jump;

        public JumpController(Jump jump)
        {
            Jump = AirJumpUtils.CreateJump(jump);

            if (jump.IsMine)
                AirJumpNetworkUtils.SendCreateJump(jump);
        }

        public void Destroy()
        {
            if (Jump.IsMine)
                AirJumpNetworkUtils.SendDestroyJump(Jump);

            GameObject.Destroy(Jump.Object);
        }
    }
}
