using Behaviors;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace DI
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private WallSegment _wallCompositeTemplate;//substitute with addressable
        [SerializeField] private AssetReferenceGameObject _wallCompositeAssetRef;
        public override void InstallBindings()
        {
            Container.BindFactory<int, WallSegment, WallSegment.Factory>().FromComponentInNewPrefab(_wallCompositeTemplate).UnderTransform(transform);
        }
    }
}