using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KittyHelpYouOut
{
    public class DeltaCalculator
    {
        private float lastValue = 0;
        private Vector3 lastVector = default;
        public float DeltaFloat(float crtValue)
        {
            if (lastValue == 0)
            {
                lastValue = crtValue;
            }
            var delta = crtValue - lastValue;
            lastValue = crtValue;
            return delta;
        }

        public float DeltaVectorSignedAngle(Vector3 crtVector, Vector3 axis)
        {
            if (lastVector == default)
            {
                lastVector = crtVector;
            }
            var delta = Vector3.SignedAngle(crtVector, lastVector, axis);
            lastVector = crtVector;
            return delta;
        }
    }
}


