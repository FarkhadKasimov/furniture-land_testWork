using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    [System.Serializable]
    public class SimpleMath
    {
        /// <summary>
        /// Float normalize function for simple math calculations
        /// </summary>
        /// <param name="value">Value of the float that will be normalized</param>
        /// <param name="min">Minimum of the value that you can get</param>
        /// <param name="max">Maximum of the value that you can get</param>
        /// <returns></returns>
        public static float NormalizeFloat(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        /// <summary>
        /// Function to return sinusoid and cosinusoid with gived frequency and amplitude
        /// </summary>
        public static float Sin(float frequency = 0, float amplitude = 0)
        {
            return Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        }

        public static float Cos(float frequency = 0, float amplitude = 0)
        {
            return Mathf.Cos(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        }
        
        public static void ReAssignItems<T>(List<T> list)
        {
            var aux = list[0];
            list.Remove(list[0]);
            list.Add(aux);
        }
    }
}

