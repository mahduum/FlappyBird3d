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
        [SerializeField] private SO_GameStateChannel _gameStateChannel;
        [SerializeField] private Wall _topWall;
        [SerializeField] private Wall _bottomWall;
        [SerializeField] private int _gridCellsOnSide = 8;//from SO
        [SerializeField] private float _gapSize = 5.0f;//from SO depending on level

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
            SpawnObstacles();
        }
        
        //todo activate colliders on segment collider enter

        private void UpdateScoreThresholds()
        {
            
        }

        private void OnTriggerExit(Collider other)
        {
            _segmentUpdateChannel.RaiseEvent();
        }
        
        private void InitializeGrid()
        {
            var position = _bottomWall.transform.position;//todo make individual offsets for top and bottom
            _squareGrid2d ??= new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(
                _gridCellsOnSide,
                position,
                new Vector2(_bottomWallRenderer.bounds.size.x, _bottomWallRenderer.bounds.size.z),
                new SimpleGridObstacleDataCreator(MaxHeight, MinHeight));
            _squareGrid2d.CreateElements();
        }
        
        private void SpawnObstacles()
        {
            for (int i = 0; i < _squareGrid2d.Elements.Length; i++)
            {
                var obstacleData = _squareGrid2d.Elements[i];
                if (obstacleData.Type == ObstacleType.None)
                {
                    continue;
                }
                
                var index = i;

                

                (Bounds desiredBoundsBottom, Bounds desiredBoundsTop) = _squareGrid2d.GetCellBoundsComplementPair(i);//todo is squashed along the z or 2d y, and height position is wrong

                //todo make a method to do both
                //bottom
                GameObject obstacleBottom = null;
                if (obstacleData.BottomHeight > 0)
                {
                    obstacleBottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    var currentBoundsSizeBottom = obstacleBottom.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;
                    var scaleToFitBoundsBottom = new Vector3((desiredBoundsBottom.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.x,
                        desiredBoundsBottom.size.y / currentBoundsSizeBottom.y, (desiredBoundsBottom.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.z);

                    obstacleBottom.transform.localScale = scaleToFitBoundsBottom;
                    obstacleBottom.transform.SetParent(_bottomWall.transform);
                    obstacleBottom.transform.position = new Vector3(desiredBoundsBottom.center.x, _bottomWall.transform.position.y + desiredBoundsBottom.extents.y * _bottomWall.transform.up.y, desiredBoundsBottom.center.z);
                    obstacleBottom.AddComponent<BoxCollider>().OnTriggerEnterAsObservable().Subscribe(
                        c =>
                        {
                            Debug.Log($"Collided with {c.tag}, on bottom obstacle index: {index}");
                            _gameStateChannel.RaiseEvent(GameState.GameOver);
                        }).AddTo(this);
                }
                
                //top
                GameObject obstacleTop = null;
                if (obstacleData.TopHeight > 0)
                {
                    obstacleTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    var currentBoundsSizeTop = obstacleTop.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;
                    var scaleToFitBoundsTop = new Vector3((desiredBoundsTop.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeTop.x,
                        desiredBoundsTop.size.y / currentBoundsSizeTop.y, (desiredBoundsTop.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeTop.z);
                
                    obstacleTop.transform.localScale = scaleToFitBoundsTop;
                    obstacleTop.transform.SetParent(_topWall.transform);
                    obstacleTop.transform.position = new Vector3(desiredBoundsTop.center.x, _topWall.transform.position.y + desiredBoundsTop.extents.y * _topWall.transform.up.y, desiredBoundsTop.center.z);
                    obstacleTop.AddComponent<BoxCollider>().OnTriggerEnterAsObservable().Subscribe(
                        c =>
                        {
                            Debug.Log($"Collided with {c.tag}, on top obstacle index: {index}");
                            _gameStateChannel.RaiseEvent(GameState.GameOver);
                        }).AddTo(this);
                }

                //gap
                if (obstacleData.HasGap == false || obstacleTop == null)
                {
                    continue;
                }

                var currentObstacleBoundsTop = obstacleTop.GetComponent<Renderer>().bounds;
                var currentObstacleBoundsBottom = obstacleBottom.GetComponent<Renderer>().bounds;
                var bottomObstacleYOffset = new Vector3(currentObstacleBoundsBottom.center.x,
                    currentObstacleBoundsBottom.center.y + currentObstacleBoundsBottom.extents.y,
                    currentObstacleBoundsBottom.center.z);
                var topObstacleYOffset = new Vector3(currentObstacleBoundsTop.center.x,
                    currentObstacleBoundsTop.center.y - currentObstacleBoundsTop.extents.y,
                    currentObstacleBoundsTop.center.z);
                var gapCenter = Vector3.Lerp(bottomObstacleYOffset, topObstacleYOffset, 0.5f);
                var gap = new GameObject($"Gap_{index}");
                gap.transform.SetParent(transform);
                gap.transform.position = gapCenter;
                var gapCollider = gap.AddComponent<BoxCollider>();
                gapCollider.size = new Vector3(desiredBoundsTop.size.x, _gapSize * 2, desiredBoundsTop.size.z);//todo * 2 is temporary until more precise sizing of the gap and upper and lower part
                //gapCollider.OnEnableAsObservable() use with reactive property to subscribe to events???
                gapCollider.OnTriggerExitAsObservable().Subscribe(c =>
                {
                    //todo use later to give specific amount of points based on Obstacle SO settings, the obstacle will be instantiated and set component instead of temp cylinders and gaps
                    Debug.Log($"Triggered with {c.tag}, on gap index: {index}");
                    _scoreUpdateChannel.RaiseEvent(new GapScore());
                }).AddTo(this);
            }
        }
    }
}
