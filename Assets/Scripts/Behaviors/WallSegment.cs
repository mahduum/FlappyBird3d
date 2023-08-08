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
        [SerializeField] private int _gridCellsOnSide = 8;//from SO
        [SerializeField] private float _gapSize = 5.0f;//from SO depending on level

        private List<GameObject> _obstacles = new List<GameObject>();//take obstacle with current index, at the end of the loop deactivate remaining obstacles, or first deactivate them
        //todo make TotalHeight, and MaxHeight for single obstacle (top and bottom) such that there is no too short obstacles to display the top of the mesh
        private float MaxHeight =>
            Vector3.Distance(_topWallRenderer.transform.position, _bottomWallRenderer.transform.position) -
            _gapSize * 2.0f;

        private float MinHeight => 3.0f;
        
        // Start is called before the first frame update
        // Based on index augment score on thresholds
        [Range(0, 99)]
        [SerializeField] private int _obstacleBoundsReductionPercent = 10;
        
        private float ObstacleBoundsReductionMultiplier => (100 - _obstacleBoundsReductionPercent) / 100.0f;

        private SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator> _squareGrid2d;

        private void Awake()
        {
            _boxCollider.center = Vector3.zero;
            _boxCollider.size = new Vector3(_topWallRenderer.bounds.size.x,
                Vector3.Distance(_topWallRenderer.transform.position, _bottomWallRenderer.transform.position), _topWallRenderer.bounds.size.z);
            
            InitializeGrid();
        }
        
        void Start()
        {
            PlaceObstacles();
        }
        
        //todo activate colliders on segment collider enter, disable all far away segments

        private void UpdateScoreThresholds()
        {
            
        }

        private void OnTriggerExit(Collider other)//increment the number of surpassed segments
        {
            _segmentUpdateChannel.RaiseEvent();
        }
        
        private void InitializeGrid()
        {
            var position = _bottomWall.transform.position;
            _squareGrid2d ??= new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(
                _gridCellsOnSide,
                position,
                new Vector2(_bottomWallRenderer.bounds.size.x, _bottomWallRenderer.bounds.size.z),
                new SimpleGridObstacleDataCreator(MaxHeight, MinHeight));
            _squareGrid2d.CreateElements();
        }

        private void DeactivateUnusedObstacles()
        {
            for (int i = _squareGrid2d.Elements.Length; i < _obstacles.Count; i++)
            {
                _obstacles[i].SetActive(false);//todo that are less than pairs! total number
            }
        }

        private GameObject PlaceObstacleRelativeToWall(Transform wallParent, Bounds? desiredBounds)
        {
            GameObject obstacle = null;
            if (desiredBounds.HasValue)
            {
                obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                var currentBoundsSizeBottom = obstacle.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;
                var scaleToFitBoundsBottom = new Vector3((desiredBounds.Value.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.x,
                    desiredBounds.Value.size.y / currentBoundsSizeBottom.y, (desiredBounds.Value.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.z);

                obstacle.transform.localScale = scaleToFitBoundsBottom;
                obstacle.transform.SetParent(wallParent);
                obstacle.transform.position = new Vector3(desiredBounds.Value.center.x, desiredBounds.Value.center.y, desiredBounds.Value.center.z);
                obstacle.AddComponent<BoxCollider>().OnTriggerEnterAsObservable().Subscribe(
                    c =>
                    {
                        Debug.Log($"Collided {c.tag} with obstacle");
                        _stateChannel.RaiseOnGameOver();
                    }).AddTo(this);
            }

            return obstacle;
        }
        
        private void PlaceObstacles()//TODO BEFORE SPAWNING OBSTACLES TRY GETTING THEM FROM CACHE
        {
            DeactivateUnusedObstacles();
            
            for (int i = 0; i < _squareGrid2d.Elements.Length; i++)
            {
                _topWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
                _bottomWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
                // _leftWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
                // _rightWall.OnTriggerEnterAsObservable().Subscribe(_ => _stateChannel.RaiseOnGameOver()).AddTo(this);
                
                var obstacleData = _squareGrid2d.Elements[i];
                if (obstacleData.Type == ObstacleType.None)
                {
                    continue;
                }
                
                var index = i;//optionally use to mark obstacles

                (Bounds? desiredBoundsBottom, Bounds? desiredBoundsTop) = obstacleData.GetBounds();

                GameObject obstacleBottom = PlaceObstacleRelativeToWall(_bottomWall.transform, desiredBoundsBottom);
                GameObject obstacleTop = PlaceObstacleRelativeToWall(_topWall.transform, desiredBoundsTop);

                SetGap(obstacleData, obstacleTop, obstacleBottom, index, desiredBoundsTop);
            }
        }

        private void SetGap(ObstacleData obstacleData, GameObject obstacleTop, GameObject obstacleBottom, int index,
            Bounds? desiredBounds)
        {
            if (obstacleData.HasGap == false || obstacleTop == null || obstacleBottom == null || desiredBounds.HasValue == false)
            {
                return;
            }

            var currentObstacleBoundsTop = obstacleTop.GetComponent<Renderer>().bounds;
            var currentObstacleBoundsBottom = obstacleBottom.GetComponent<Renderer>().bounds;
            
            var gapCenter = FindMidPointBetweenBoundsTopAndBottom(currentObstacleBoundsBottom, currentObstacleBoundsTop);

            var gap = new GameObject($"Gap_{index}"); //todo add special transparent material, make cache (gaps can have rewards on them
            
            gap.transform.SetParent(transform);
            gap.transform.position = gapCenter;
            var gapCollider = gap.AddComponent<BoxCollider>();
            gapCollider.size =
                new Vector3(
                    desiredBounds.Value.size.x,
                    _gapSize * 2,//todo * 2 is temporary until more precise sizing of the gap and upper and lower part
                    desiredBounds.Value.size.z
                    );
            //gapCollider.OnEnableAsObservable() use with reactive property to subscribe to events???
            gapCollider.OnTriggerExitAsObservable().Subscribe(c =>
            {
                //todo use later to give specific amount of points based on Obstacle SO settings, the obstacle will be instantiated and set component instead of temp cylinders and gaps
                Debug.Log($"Triggered with {c.tag}, on gap index: {index}");
                _scoreUpdateChannel.RaiseEvent(new GapScore());
            }).AddTo(this);
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
