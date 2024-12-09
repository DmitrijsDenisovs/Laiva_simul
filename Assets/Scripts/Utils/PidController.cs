using System;
using UnityEngine;

namespace Utils
{
    public class PidController
    { 
        public float OutputMax { get; set; }
        public float OutputMin { get; set; }

        public float ProportionalGain { get; set; }
        public float DifferentialGain { get; set; }
        public float IntegralGain { get; set; }
        public float IntegralSaturation { get; set; }

        public float Error { get; set; }
        public float Target { get; set; }

        public float MeasurementTime { get; set; }
        public float IntegralStored = 0;
 

        public float GetValue(float measurement, float time)
        {
            float prevError = Error;
            Error = Target - measurement;

            float prevTime = MeasurementTime;
            MeasurementTime = time;

            float proportional = Error * ProportionalGain;

            float differential = (Error - prevError) / (MeasurementTime - prevTime) * DifferentialGain;

            IntegralStored += Mathf.Clamp(IntegralStored + Error * (MeasurementTime - prevTime), -IntegralSaturation, IntegralSaturation);
            float integral = IntegralStored * IntegralGain;

            return Mathf.Clamp(proportional + differential + integral, OutputMin, OutputMax);
        }

        public void WindupIntegral()
        {
            IntegralStored = 0;
        }
    }

}