using System.Collections.Generic;
using SOs;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Grid;
using Utilities.Scores;

namespace Behaviors
{
    public class WallSegment : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        [FormerlySerializedAs("_topWall")] [SerializeField] private Renderer _topWallRenderer;
        [FormerlySerializedAs("_bottomWall")] [SerializeField] private Renderer _bottomWallRenderer;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
        [SerializeField] private SO_ScoreUpdateChannel _scoreUpdateChannel;
        [FormerlySerializedAs("_gameStateChannel")] [SerializeField] private SO_StateChannel _stateChannel;//todo???
        [SerializeField] private Wall _topWall;
        [SerializeField] private Wall _bottomWall;
        [SerializeField] private Wall _leftWall;
        [SerializeField] private Wall _rightWall;

        private readonly List<GameObject> _obstacles = new();
        private readonly List<GameObject> _gaps = new();

        private readonly List<BoxCollider> _onGridColliders = new();
        
        public int? Index { get; private set; }

        private WallsManager _owner;
        private WallsManager Owner =>_owner ??= GetComponentInParent<WallsManager>();
        
        [Range(0, 99)]
        [SerializeField] private int _obstacleBoundsReductionPercent = 10;
        
        private float ObstacleBoundsReductionMultiplier => (100 - _obstacleBoundsReductionPercent) / 100.0f;

        private SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator> _squareGrid2d;

        private void Awake()
        {
            _boxCollider.center = Vector3.zero;
            _boxCollider.size = new Vector3(_topWallRenderer.bounds.size.x,
                Vector3.Distance(_topWallRenderer.transform.position, _bottomWallRenderer.transform.position), _topWallRenderer.bounds.size.z);
            
            _topWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
            _bottomWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
            _leftWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
            _rightWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
            _boxCollider.OnTriggerExitAsObservable().Subscribe(c =>
            {
                if (c.CompareTag("Player") == false)
                {
                    return;
                }
                
                _segmentUpdateChannel.RaiseEvent();
                foreach (var col in _onGridColliders)
                {
                    col.enabled = false;
                }   
            }).AddTo(this);
            
            _boxCollider.OnTriggerEnterAsObservable().Subscribe(_ =>
            {
                _segmentUpdateChannel.RaiseEvent();
                foreach (var col in _onGridColliders)
                {
                    col.enabled = true;
                }   
            }).AddTo(this);
        }

        public void Set(int index)
        {
            Index = index;
            InitializeGrid();
            PlaceObstacles();
        }

        private void InitializeGrid()
        {
            var gridSetting = Owner.GetOrCreateSegmentSettings(Index.GetValueOrDefault());
            var position = _bottomWall.transform.position;
            _squareGrid2d = null;
            _squareGrid2d = new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(
                gridSetting.MaxElementsPerSide.Item1,
                position,
                new Vector2(_bottomWallRenderer.bounds.size.x, _bottomWallRenderer.bounds.size.z),
                new SimpleGridObstacleDataCreator(gridSetting));
            _squareGrid2d.CreateElements();
        }

        private void PlaceObstacles()
        {
            int obstacleNumber = 0;
            int gapNumber = 0;

            for (int i = 0; i < _squareGrid2d.Elements.Length; i++)
            {
                var obstacleData = _squareGrid2d.Elements[i];
                if (obstacleData.Type == ObstacleType.None)
                {
                    continue;
                }
                
                var index = i;//optionally use to mark obstacles

                (Bounds? desiredBoundsBottom, Bounds? desiredBoundsTop) = obstacleData.GetBounds();
                
                GameObject obstacleBottom = PlaceObstacleRelativeToWall(_bottomWall.transform, desiredBoundsBottom, ref obstacleNumber);
                GameObject obstacleTop = PlaceObstacleRelativeToWall(_topWall.transform, desiredBoundsTop, ref obstacleNumber);

                SetGap(obstacleData, obstacleTop, obstacleBottom, index, desiredBoundsTop, ref gapNumber);
            }
            
            DeactivateUnused(_obstacles, obstacleNumber);
            DeactivateUnused(_gaps, gapNumber);
        }

        private void DeactivateUnused(IList<GameObject> gameObjects, int startIndex)
        {
            //up to start index set delayed activation
            for (int i = startIndex; i < gameObjects.Count; i++)
            {
                gameObjects[i].SetActive(false);
            }
        }

        private GameObject PlaceObstacleRelativeToWall(Transform wallParent, Bounds? desiredBounds, ref int obstacleNumber)
        {
            GameObject obstacle = null;
            if (desiredBounds.HasValue)
            {
                bool canBePooled = obstacleNumber < _obstacles.Count;
                obstacle = canBePooled
                    ? _obstacles[obstacleNumber]
                    : GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                if (canBePooled == false)
                {
                    _obstacles.Add(obstacle);
                }

                var obstacleTransform = obstacle.transform;
                obstacleTransform.localScale = Vector3.one;
                var currentBoundsSizeBottom = obstacle.GetComponent<Renderer>()?.bounds.size ?? Vector3.zero;
                var scaleToFitBoundsBottom = new Vector3(
                    (desiredBounds.Value.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.x,
                    desiredBounds.Value.size.y / currentBoundsSizeBottom.y,
                    (desiredBounds.Value.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.z);

                obstacleTransform.localScale = scaleToFitBoundsBottom;
                obstacleTransform.position = new Vector3(desiredBounds.Value.center.x, desiredBounds.Value.center.y,
                    desiredBounds.Value.center.z);
                obstacleTransform.SetParent(wallParent, true);
                
                if (obstacle.GetComponent<BoxCollider>() == false)
                {
                    var boxCollider = obstacle.AddComponent<BoxCollider>();
                    boxCollider.OnTriggerEnterAsObservable().Subscribe(
                        c =>
                        {
                            _stateChannel.RaiseOnGameOver();
                        }).AddTo(this);

                    boxCollider.enabled = false;
                    _onGridColliders.Add(obstacle.GetComponent<BoxCollider>());
                }

                if (obstacle.GetComponent<CapsuleCollider>() is
                    { } capsuleCollider) //temp work around, I don't need this on primitives is by default
                {
                    capsuleCollider.enabled = false;
                }
                
                obstacle.SetActive(true);
                obstacleNumber++;
            }

            return obstacle;
        }

        private void SetGap(ObstacleData obstacleData, GameObject obstacleTop, GameObject obstacleBottom, int index,
            Bounds? desiredBounds, ref int gapNumber)
        {
            if (obstacleData.HasGap == false || obstacleTop == null || obstacleBottom == null || desiredBounds.HasValue == false)
            {
                return;
            }

            bool canBePooled = gapNumber < _gaps.Count;
            var gap = canBePooled ? _gaps[gapNumber] : new GameObject($"Gap_{index}"); //todo add special transparent material, make cache (gaps can have rewards on them

            if (canBePooled == false)
            {
                _gaps.Add(gap);
            }

            var currentObstacleBoundsTop = obstacleTop.GetComponent<Renderer>().bounds;
            var currentObstacleBoundsBottom = obstacleBottom.GetComponent<Renderer>().bounds;
            gap.transform.SetParent(transform);
            var gapCenter = FindMidPointBetweenBoundsTopAndBottom(currentObstacleBoundsBottom, currentObstacleBoundsTop);
            gap.transform.position = gapCenter;

            if (gap.GetComponent<BoxCollider>() == false)
            {
                var boxCollider = gap.AddComponent<BoxCollider>();
                boxCollider.OnTriggerExitAsObservable().Subscribe(c =>
                {
                    //todo use later to give specific amount of points based on Obstacle SO settings, the obstacle will be instantiated and set component instead of temp cylinders and gaps
                    _scoreUpdateChannel.RaiseEvent(new GapScore());
                }).AddTo(this);

                boxCollider.enabled = false;
                _onGridColliders.Add(boxCollider);
            }
            
            var gapCollider = gap.GetComponent<BoxCollider>();
            gapCollider.enabled = false;
            gapCollider.size =
                new Vector3(
                    desiredBounds.Value.size.x,
                    obstacleData.GapSize,
                    desiredBounds.Value.size.z
                    );
            
            //gapCollider.OnEnableAsObservable() use with reactive property to subscribe to events???
            gap.SetActive(true);
            gapNumber++;
        }

        private static Vector3 FindMidPointBetweenBoundsTopAndBottom(Bounds boundsA, Bounds boundsB)
        {
            //no matter the arg order establish offset direction
            var deltaYFromAtoB = boundsB.center.y - boundsA.center.y;
            var deltaYNormalizedFromAtoB = deltaYFromAtoB / Mathf.Abs(deltaYFromAtoB);
            var deltaYNormalizedFromBtoA = -deltaYNormalizedFromAtoB;
            var obstacleAOffset = new Vector3(boundsA.center.x,
                boundsA.center.y + boundsA.extents.y * deltaYNormalizedFromAtoB,
                boundsA.center.z);
            var obstacleBOffset = new Vector3(boundsB.center.x,
                boundsB.center.y + boundsB.extents.y * deltaYNormalizedFromBtoA,
                boundsB.center.z);
            var gapCenter = Vector3.Lerp(obstacleBOffset, obstacleAOffset, 0.5f);
            return gapCenter;
        }
    }
}
