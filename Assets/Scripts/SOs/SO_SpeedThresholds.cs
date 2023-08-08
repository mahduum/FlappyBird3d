using System.Collections.Generic;
using UnityEngine;

namespace SOs
{
    [CreateAssetMenu(menuName = "Data/Speed Thresholds")]
    public class SO_SpeedThresholds : ScriptableObject
    {
        [System.Serializable]
        public struct DictionaryEntry
        {
            public int Threshold;
            public float Speed;
        }

        public List<DictionaryEntry> ThresholdSpeeds = new();
    }
}
