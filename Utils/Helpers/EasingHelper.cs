using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace TootTally.Utils.Helpers
{
    public static class EasingHelper
    {
        public static float Lerp(float firstFloat, float secondFloat, float by) //Linear easing
        {
            return firstFloat + (secondFloat - firstFloat) * by;
        }

        public static Vector2 Lerp(Vector2 firstVector, Vector2 secondVector, float by)
        {
            return new Vector2(Lerp(firstVector.x, secondVector.x, by), Lerp(firstVector.y, secondVector.y, by));
        }

        public static float EaseIn(float by) => by * by;
        public static float EaseOut(float by) => 1 - EaseIn(1 - by);
        public static float EaseInAndOut(float by)
        {
            if (by < 0.5) return EaseIn(by * 2) / 2;
            return 1 - EaseIn((1 - by) * 2) / 2;
        }
    }
}
