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

        /// Taken from https://www.youtube.com/watch?v=KPoeNZZ6H4s and modified to my liking
        /// --------------
        /// Thanks to @TheWhiteShadow#6615 for the detailed explanation
        /// --------------
        /// [Damping]
        /// z = 0 no damping -> vibrates forever
        /// 0 < z < 1 vibrates but settles at target
        /// z >= 1 settles at target slower the higher the value (over z amount of Hz(?))
        /// -------------
        /// [Initial Response]
        /// r < 0 "anticipates" the motion (moves in opposite direction for a bit before going to target)
        /// r = 0 takes time to accelerate
        /// 0 < r < 1 still takes time to accelerate but less the higher the value
        /// r = 1 takes no time to accelerate
        /// r > 1 takes no time to accelerate and overshoots target
        /// -------------
        public class SecondOrderDynamics
        {
            public Vector3 startVector;
            public Vector3 newVector, speed;
            public float f, z, r;

            public SecondOrderDynamics(float f, float z, float r)
            {
                SetConstants(f, z, r);
                startVector = newVector = speed = Vector3.zero;
            }


            /// <summary>
            /// Constants affect the behavior of the animation in 3 different ways: Frequency, Damping, Initial Response.
            /// </summary>
            /// <param name="f">Frequency</param>
            /// <param name="z">Damping</param>
            /// <param name="r">Initial Response</param>
            public void SetConstants(float f, float z, float r)
            {
                var PI = Mathf.PI;
                var PI2f = 2f * PI * f;

                this.f = z / (PI * f);
                this.z = 1 / Mathf.Pow(PI2f, 2);
                this.r = r * z / PI2f;
            }

            public void SetStartVector(Vector3 startPosition) => this.startVector = newVector = startPosition;


            public Vector3 GetNewVector(Vector3 destination, float deltaTime)
            {
                Vector3 estimatedVelocity = (destination - startVector) / deltaTime;
                startVector = destination;

                float z_stable = Mathf.Max(z, deltaTime * deltaTime / 2 + deltaTime * f / 2, deltaTime * f);

                newVector += deltaTime * speed;
                speed += deltaTime * (destination + r * estimatedVelocity - newVector - f * speed) / z_stable;
                return newVector;
            }
        }
    }
}
