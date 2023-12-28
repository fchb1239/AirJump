using UnityEngine;

namespace AirJump
{
    public static class Constants
    {
        public static Vector3[] Sizes = new Vector3[]
        {
            new Vector3(0.0125f, 0.28f, 0.3825f),
            new Vector3(0.0125f, 0.42f, 0.57375f),
            new Vector3(0.0125f, 0.56f, 0.765f)
        };

        public static Vector3 leftHandOffset = Vector3.down * 0.05f;
        public static Vector3 rightHandOffset = Vector3.down * 0.0425f;
    }
}
