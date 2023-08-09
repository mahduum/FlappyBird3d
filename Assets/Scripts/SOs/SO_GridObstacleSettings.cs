using System.Collections.Generic;
using UnityEngine;
using Utilities.Grid;

namespace SOs
{
    [CreateAssetMenu(menuName = "Data/Grid Obstacle Settings")]
    public class SO_GridObstacleSettings : ScriptableObject
    {
        [Tooltip("List index represents the level and value is this level's range counted in segment indices less or equal to.")]
        [SerializeField]
        private List<int> LevelThresholdsBySegmentIndex = new();
        [SerializeField]
        private List<int> MaxObstaclesPerSide = new();
        [Tooltip("Percentage of distance between bottom and top wall.")]
        [Range(0,1)]
        [SerializeField]
        private List<float> ObstacleMinHeightLimits = new ();
        [Tooltip("Percentage of distance between bottom and top wall.")]
        [Range(0,1)]
        [SerializeField]
        private List<float> ObstacleMaxHeightLimits = new ();
        [Tooltip("Percentage of distance between bottom and top wall.")]
        [Range(0,1)]
        [SerializeField]
        private List<float> GapSizes = new();
        [Tooltip("Percent probability of spawning an obstacle inside grid cell.")]
        [Range(0,1)]
        [SerializeField]
        private List<float> ObstacleProbability = new();

        private bool IsValid()
        {
            var levels = LevelThresholdsBySegmentIndex.Count;
            if (levels == MaxObstaclesPerSide.Count &&
                levels == ObstacleProbability.Count &&
                levels == ObstacleMinHeightLimits.Count &&
                levels == MaxObstaclesPerSide.Count &&
                levels == GapSizes.Count &&
                levels == ObstacleMaxHeightLimits.Count) return true;
            Debug.LogError("All lists should have the same number of elements!");
            return false;
        }
        
        public GridElementsSetting CreateGridElementsSettingForLevel(int level)
        {
            if (level < 0 || IsValid() == false)
            {
                return default;
            }

            return new GridElementsSetting(
                (MaxObstaclesPerSide[level], MaxObstaclesPerSide[level]),//todo split this to a separate setting struct
                ObstacleMinHeightLimits[level],
                ObstacleMaxHeightLimits[level],
                GapSizes[level],
                ObstacleProbability[level]
            );
        }
        
        public int GetSettingsLevelFromSegmentIndex(int index)
        {
            int level = LevelThresholdsBySegmentIndex.FindIndex(item => index <= item);
            if (level < 0 && index > 0)
            {
                return LevelThresholdsBySegmentIndex.Count - 1;
            }

            return Mathf.Max(level, 0);
        }
    }
}
