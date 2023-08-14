using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SOs;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using Utilities.Grid;

namespace Behaviors
{
    public class WallsManager : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private int _visibleUnitsNumber;
        [SerializeField] private GameObject _wallCompositeTemplate;
        [SerializeField] private AssetReferenceGameObject _wallCompositeAssetRef;
        [SerializeField] private SO_StateChannel _stateChannel;
        [FormerlySerializedAs("_gridObstacleSettings")] [SerializeField] private SO_GridObstacleSettings _SO_GridObstacleSettings;

        private List<AsyncOperationHandle<GameObject>> _wallCompositeHandles = new();
        //todo add side walls, so obstacles but heating up the player or can have sudden wind gusts
        private Vector3 SpawningDirection => _playerController.transform.forward;
        private float DepthBound
        {
            get
            {
                if (WallSegment)
                {
                    return WallSegment.UnitRenderer.bounds.size.z;
                }

                return 100.0f;
            }
        }

        private readonly List<WallSegment> _wallCompositePool = new();
        private readonly List<GridElementsSetting> _wallSegmentGridSettings = new();

        private WallSegment WallSegment => _wallCompositePool.Count > 0 ? _wallCompositePool[0] : null;

        private void Awake()
        {
            _stateChannel.OnReset += ResetSegmentsPosition;
        }

        private void OnDestroy()
        {
            _stateChannel.OnReset -= ResetSegmentsPosition;
        }

        void Start()
        {
            //todo set spacing between upper and lower wall on prefab to spawn based on settings
            InstantiateSegments();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, SpawningDirection * DepthBound);
        }

        public void UpdateSegmentsPosition()
        {
            var first = _wallCompositePool[0];
            var last = _wallCompositePool[^1];
            first.transform.position = OffsetPosition(last.transform.position);
            _wallCompositePool.RemoveAt(0);
            _wallCompositePool.Add(first);
            first.Set(last.Index.GetValueOrDefault() + 1);
        }

        private async void InstantiateSegments()
        {
            if (_wallCompositePool.Count == _visibleUnitsNumber)
            {
                return;
            }

            int segmentsToSpawn = _visibleUnitsNumber - _wallCompositePool.Count;
            var position = _wallCompositePool.Count > 0
                ? _wallCompositePool[^1].transform.position
                : transform.position;

            var wallCompositeHandle = _wallCompositeAssetRef.LoadAssetAsync<GameObject>();
            _wallCompositeHandles.Add(wallCompositeHandle);
            await wallCompositeHandle.ToUniTask();

            for (int i = 0; i < segmentsToSpawn; i++)
            {
                if (wallCompositeHandle.Status != AsyncOperationStatus.Succeeded || wallCompositeHandle.Result == null)
                {
                    break;
                }
                
                var spawned =
                    Instantiate(_wallCompositeTemplate, _wallCompositePool.Count > 0 ? OffsetPosition(position) : position, Quaternion.identity,
                        transform);
                position = spawned.transform.position;
                if (spawned.GetComponent<WallSegment>() is { } segment)
                {
                    _wallCompositePool.Add(segment);
                    segment.Set(i);
                }
                spawned.SetActive(true);
            }
            
            Addressables.Release(wallCompositeHandle);
        
            if (_wallCompositePool.Count > 1)//sort when is done
            {
                _wallCompositePool.Sort((a, b) => (int)((a.transform.position.z - b.transform.position.z) * 1000.0f));//todo change to queue then the sorting won't be needed
            }
        }

        private void ResetSegmentsPosition()
        {
            var position = transform.position;
            for (int i = 0; i < _wallCompositePool.Count; i++)
            {
                if (i > 0)
                {
                    position = OffsetPosition(position);
                }
                
                var segment = _wallCompositePool[i];
                segment.transform.position = position;
                segment.GetComponent<WallSegment>()?.Set(i);
            }
            
            InstantiateSegments();
        }

        private Vector3 OffsetPosition(Vector3 position)
        {
            return position + new Vector3(0, 0, DepthBound);
        }

        public GridElementsSetting GetOrCreateSegmentSettings(int index)
        {
            int level = _SO_GridObstacleSettings.GetSettingsLevelFromSegmentIndex(index);
            if (level >= 0 &&
                level < _wallSegmentGridSettings.Count &&
                _wallSegmentGridSettings[level] is { } cachedSetting)
            {
                return cachedSetting;
            }

            var setting = _SO_GridObstacleSettings.CreateGridElementsSettingForLevel(level);
            _wallSegmentGridSettings.Add(setting);
            return setting;
        }
    }
}
