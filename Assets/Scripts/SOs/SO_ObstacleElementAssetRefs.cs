using System.Collections.Generic;
using Behaviors;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Utilities;

namespace SOs
{
    [CreateAssetMenu(menuName = "Data/Obstacle Element Asset References")]
    public class SO_ObstacleElementAssetRefs : ScriptableObject
    {
        [FormerlySerializedAs("Dictionary")] public List<SerializableDictionary<WallsManager.ObstacleElementType, AssetReferenceGameObject>.SerializableDictionaryObject> Entries = new ();
    }
}
