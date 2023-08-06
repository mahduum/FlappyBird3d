using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Behaviors
{
    public class Obstacle : MonoBehaviour
    {
        /*elements: (shaft, top), reward mesh*/
        
        [SerializeField] private AssetReference _rewardTypeA;

        private int _index;
        private Bounds combinedBounds;

        void Start()
        {
            combinedBounds = CalculateCombinedBounds(transform);
        }

        private Bounds CalculateCombinedBounds(Transform currentTransform)
        {
            MeshRenderer renderer = currentTransform.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                return renderer.bounds;
            }

            // If the current object doesn't have a MeshRenderer, we check its children.
            Bounds bounds = new Bounds(currentTransform.position, Vector3.zero);

            foreach (Transform child in currentTransform)
            {
                Bounds childBounds = CalculateCombinedBounds(child);
                bounds.Encapsulate(childBounds);
            }

            return bounds;
        }

        // Access the combined bounds from other scripts if needed
        public Bounds GetCombinedBounds() => combinedBounds;
    }
}