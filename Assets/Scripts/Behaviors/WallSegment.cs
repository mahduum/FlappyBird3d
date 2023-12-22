using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using SOs;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;
using Utilities.Grid;
using Utilities.Scores;
using Zenject;

namespace Behaviors
{
    public class WallSegment : MonoBehaviour
    {
        //Zenject
        [Inject]
        public void Construct(int index)//todo also set position!!!
        {
            Index = index;
        }
        
        //end
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
        [SerializeField] private GameObject _basicObstacleTemplate;
        [SerializeField] private GameObject _basicGapTemplate;

        private readonly List<GameObject> _obstacles = new();
        private readonly List<GameObject> _gaps = new();

        private readonly HashSet<BoxCollider> _onGridColliders = new();
        
        public Renderer UnitRenderer => _topWallRenderer;
        
        public int? Index { get; private set; }//todo make it a wrapper type struct

        private WallsManager _owner;
        private WallsManager Owner => _owner ??= GetComponentInParent<WallsManager>();//todo this can be injected on spawn DI
        
        [Range(0, 99)]
        [SerializeField] private int _obstacleBoundsReductionPercent = 10;
        
        private float ObstacleBoundsReductionMultiplier => (100 - _obstacleBoundsReductionPercent) / 100.0f;

        private SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator> _squareGrid2d;

        private GameObject InstantiateObstacleElement(WallsManager.ObstacleElementType obstacleElementType, Transform parent = null)
        {
            if (Owner.AllObstacleElementsTask.Status != UniTaskStatus.Succeeded)
            {
                Debug.LogError("All obstacle element assets should be loaded before instantiation inside wall segment!");
            }
            else if (Owner.ObstacleElementHandles.TryGetValue(obstacleElementType, out var handle) &&
                     handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
            {
                return Instantiate(handle.Result, parent == null ? transform : parent);
            }

            return new GameObject("MISSING ASSET!!!");
        }

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
            var position = _bottomWall.transform.position;//todo needs bottom wall for position
            _squareGrid2d = null;//todo grid can be injected but must be defined how, and it needs index for settings
            _squareGrid2d = new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(//todo this should be a grid from factory DI, but it needs the index when the segment is being created
                gridSetting.MaxElementsPerSide.Item1,
                position,
                new Vector2(_bottomWallRenderer.bounds.size.x, _bottomWallRenderer.bounds.size.z),
                new SimpleGridObstacleDataCreator(gridSetting));//todo creator is dependent on setting
            _squareGrid2d.CreateElements();
        }

        public class GridFactory : PlaceholderFactory<int, Vector3, Vector2, IGridElementMaker<ObstacleData>, SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>>//first come dependencies to the creation, then the type of the creation
        {
        }

        public class Factory : PlaceholderFactory<int, WallSegment>//int is index we are injecting on creation, can be passed in Contruct [Inject] method
        {
        }

        private async void PlaceObstacles()
        {
            //load all handles first, assure that grid has info about all types of obstacle assets to be loaded (todo):
            await Owner.AllObstacleElementsTask;

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

                (Bounds? desiredBoundsBottom, Bounds? desiredBoundsTop, _) = obstacleData.GetBounds();
                
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
                    : InstantiateObstacleElement(WallsManager.ObstacleElementType.BasicShaft, wallParent);//todo get element type from grid data?
                if (canBePooled == false)
                {
                    _obstacles.Add(obstacle);
                }

                var obstacleTransform = SetElementDesiredBounds(desiredBounds, obstacle);
                obstacleTransform.SetParent(wallParent, true);

                var boxCollider = GetOrAddBoxCollider(obstacle, desiredBounds, null);//todo obstacle component will have serialized components
                boxCollider.enabled = false;
                if (_onGridColliders.Add(boxCollider))
                {
                    boxCollider.OnTriggerEnterAsObservable().Subscribe(
                        c =>
                        {
                            _stateChannel.RaiseOnGameOver();
                        }).AddTo(this);
                }

                if (obstacle.GetComponent<CapsuleCollider>() != null) //temp work around, I don't need this on primitives is by default
                {
                    obstacle.GetComponent<CapsuleCollider>().enabled = false;
                }
                
                obstacle.SetActive(true);
                obstacleNumber++;
            }

            return obstacle;
        }

        private Transform SetElementDesiredBounds(Bounds? desiredBounds, GameObject element)
        {
            var rendererComponent = element.GetComponent<Renderer>();
            if (rendererComponent == null)
            {
                return element.transform;
            }
            var elementTransform = element.transform;
            elementTransform.position = Vector3.zero;
            elementTransform.localScale = Vector3.one;
            var currentBoundsSizeBottom = rendererComponent.bounds.size;
            var scaleToFitBoundsBottom = new Vector3(
                (desiredBounds.Value.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.x,
                desiredBounds.Value.size.y / currentBoundsSizeBottom.y,
                (desiredBounds.Value.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.z);

            elementTransform.localScale = scaleToFitBoundsBottom;
            elementTransform.position = new Vector3(desiredBounds.Value.center.x, desiredBounds.Value.center.y,
                desiredBounds.Value.center.z);
            return elementTransform;
        }

        private void SetGap(ObstacleData obstacleData, GameObject obstacleTop, GameObject obstacleBottom, int index,
            Bounds? desiredBounds, ref int gapNumber)
        {
            if (obstacleData.HasGap == false || obstacleTop == null || obstacleBottom == null || desiredBounds.HasValue == false)
            {
                return;
            }

            bool canBePooled = gapNumber < _gaps.Count;
            var gap = canBePooled ? _gaps[gapNumber] : InstantiateObstacleElement(WallsManager.ObstacleElementType.BasicGap);

            if (canBePooled == false)
            {
                _gaps.Add(gap);
            }

            var currentObstacleBoundsTop = obstacleTop.GetComponent<Renderer>().bounds;
            var currentObstacleBoundsBottom = obstacleBottom.GetComponent<Renderer>().bounds;
            
            (_, _, Bounds? desiredBoundsMiddle) = obstacleData.GetBounds();
            SetElementDesiredBounds(desiredBoundsMiddle, gap);

            gap.transform.SetParent(transform);
            var gapCenter = FindMidPointBetweenBoundsTopAndBottom(currentObstacleBoundsBottom, currentObstacleBoundsTop);
            gap.transform.position = gapCenter;

            var gapCollider = GetOrAddBoxCollider(gap, desiredBounds, obstacleData);
            
            if (_onGridColliders.Add(gapCollider))
            {
                gapCollider.OnTriggerExitAsObservable().Subscribe(c =>
                {
                    //todo use later to give specific amount of points based on Obstacle SO settings, the obstacle will be instantiated and set component instead of temp cylinders and gaps
                    _scoreUpdateChannel.RaiseEvent(new GapScore());
                }).AddTo(this);
            };
            
            gapCollider.enabled = false;

            //gapCollider.OnEnableAsObservable() use with reactive property to subscribe to events???
            gap.SetActive(true);
            gapNumber++;
        }

        private static BoxCollider GetOrAddBoxCollider(GameObject gameObjectElement, Bounds? desiredBounds, ObstacleData? obstacleData)
        {
            var boxCollider = gameObjectElement.GetComponent<BoxCollider>();

            if (boxCollider != null)
            {
                return boxCollider;
            }

            boxCollider = gameObjectElement.AddComponent<BoxCollider>();

            if (gameObjectElement.GetComponent<Renderer>())
            {
                return boxCollider;
            }
            
            boxCollider.size =
                new Vector3(
                    desiredBounds.Value.size.x,
                    obstacleData.GetValueOrDefault().GapSize,
                    desiredBounds.Value.size.z
                );
            
            return boxCollider;
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
