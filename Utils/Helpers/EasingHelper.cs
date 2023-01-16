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

        //Taken from https://www.youtube.com/watch?v=KPoeNZZ6H4s and modified to my liking
        public class SecondOrderDynamics
        {
            public Vector2 startPosition;
            public Vector2 newPosition, speed;
            public float f, z, r;

            public SecondOrderDynamics(float f, float z, float r)
            {
                SetConstants(f, z, r);
                startPosition = newPosition = speed = Vector2.zero;
            }

            /// <summary>
            /// Constants affect the behavior of the animation in 3 different ways: Frequency, Damping, Initial Response.
            /// 
            /// </summary>
            /// <param name="f">f is frequency</param>
            /// <param name="z">z is damping</param>
            /// <param name="r">r is initial response</param>
            public void SetConstants(float f, float z, float r)
            {
                var PI = (float)Math.PI;
                var PI2f = 2f * PI * f;

                this.f = z / (PI * f);
                this.z = 1 / Mathf.Pow(PI2f, 2);
                this.r = r * z / PI2f;
            }

            public void SetStartPosition(Vector2 startPosition) => this.startPosition = newPosition = startPosition;


            public Vector2 GetNewPosition(Vector2 destination, float deltaTime)
            {
                Vector2 estimatedVelocity = (destination - startPosition) / deltaTime;
                startPosition = destination;

                float z_stable = Mathf.Max(z, deltaTime * deltaTime / 2 + deltaTime * f / 2, deltaTime * f);

                newPosition += deltaTime * speed;
                speed += deltaTime * (destination + r * estimatedVelocity - newPosition - f * speed) / z_stable;
                return newPosition;
            }
        }
    }
}
