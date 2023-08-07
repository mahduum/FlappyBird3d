using System.Collections.Generic;
using SOs;
using UnityEditor;
using UnityEngine;

namespace Behaviors
{
    public class WallsManager : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerController;//todo naive solution: follow the position every frame, refactor to recalculate only on events
        [SerializeField] private int _visibleUnitsNumber;
        [SerializeField] private Renderer _unitRenderer;
        [SerializeField] private GameObject _wallCompositeTemplate;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
    
        //todo add side walls, so obstacles but heating up the player or can have sudden wind gusts

        private Vector3 SpawningDirection => _playerController.transform.forward;
        private float DepthBound => _unitRenderer.bounds.size.z;
        private readonly List<GameObject> _wallCompositePool = new List<GameObject>();//todo order it by z

        private void Awake()
        {
            _segmentUpdateChannel.OnSegmentExit += UpdateWallsPosition;
        }

        private void OnDestroy()
        {
            _segmentUpdateChannel.OnSegmentExit -= UpdateWallsPosition;
        }

        void Start()
        {
            InstantiateWalls();
        }

        private void OnValidate()
        {
            //InstantiateWalls();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, SpawningDirection * DepthBound);
        }

        private void UpdateWallsPosition()
        {
            //todo first take on update, take last and append it at the front, and reinitialize obstacles (LATER)
            var first = _wallCompositePool[0];
            var last = _wallCompositePool[^1];
            first.transform.position = OffsetPosition(last.transform.position);
            _wallCompositePool.RemoveAt(0);
            _wallCompositePool.Add(first);//or sort, but useless to go through all of it
        }

        private void InstantiateWalls()
        {
            if (_wallCompositePool.Count == _visibleUnitsNumber)
            {
                return;
            }

            if (_wallCompositePool.Count > _visibleUnitsNumber)
            {
                while (_wallCompositePool.Count > _visibleUnitsNumber)
                {
                    int indexToRemove = _wallCompositePool.Count - 1;
                    var toDestroy = _wallCompositePool[indexToRemove];
                    _wallCompositePool.RemoveAt(indexToRemove);
#if UNITY_EDITOR
                    // Schedule the object for destruction during the Editor update phase
                    EditorApplication.delayCall += () => DestroyImmediate(toDestroy);
#else
                Destroy(toDestroy);
#endif
                }

                return;
            }

            int segmentsToSpawn = _visibleUnitsNumber - _wallCompositePool.Count;
            var position = _wallCompositePool.Count > 0
                ? _wallCompositePool[^1].transform.position
                : transform.position;

            for (int i = 0; i < segmentsToSpawn; i++)
            {
                var spawned =
                    Instantiate(_wallCompositeTemplate, _wallCompositePool.Count > 0 ? OffsetPosition(position) : position, Quaternion.identity,
                        transform); //todo remember to use addressables with obstacles, as addressable refference
                _wallCompositePool.Add(spawned);
                position = spawned.transform.position;
                spawned.SetActive(true);
            }
        
            if (_wallCompositePool.Count > 1)
            {
                _wallCompositePool.Sort((a, b) => (int)((a.transform.position.z - b.transform.position.z) * 1000.0f));//todo change to queue then the sorting won't be needed
            }
        }

        private Vector3 OffsetPosition(Vector3 position)
        {
            return position + new Vector3(0, 0, DepthBound);
        }

        private void RepositionWalls()//TODO
        {
            //reset all transforms, set info to all wall to reposition to starting configuration, use pool to reconfigure segments
        }
    }
}
