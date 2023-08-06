using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Grid;

namespace Behaviors
{
    public class WallSegment : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        [FormerlySerializedAs("_topWall")] [SerializeField] private Renderer _topWallRenderer;
        [FormerlySerializedAs("_bottomWall")] [SerializeField] private Renderer _bottomWallRenderer;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
        [SerializeField] private Wall _topWall;
        [SerializeField] private Wall _bottomWall;
        [SerializeField] private int _gridCellsOnSide = 8;//from SO
        [SerializeField] private float _gapSize = 5.0f;

        //todo make TotalHeight, and MaxHeight for single obstacle (top and bottom) such that there is no too short obstacles to display the top of the mesh
        private float MaxHeight =>
            Vector3.Distance(_topWallRenderer.transform.position, _bottomWallRenderer.transform.position) -
            _gapSize * 2.0f;
        
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
            _squareGrid2d ??= new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(_gridCellsOnSide, position, new Vector2(_bottomWallRenderer.bounds.size.x, _bottomWallRenderer.bounds.size.z), new SimpleGridObstacleDataCreator(MaxHeight));
            _squareGrid2d.CreateElements();
        }
        
        private void SpawnObstacles()
        {
            for (int i = 0; i < _squareGrid2d.Elements.Length; i++)
            {
                if (_squareGrid2d.Elements[i].Type == ObstacleType.None)
                {
                    continue;
                }
                
                var obstacleBottom = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                var currentBoundsSizeBottom = obstacleBottom.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;
                var currentBoundsSizeTop = obstacleBottom.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;

                (Bounds desiredBoundsBottom, Bounds desiredBoundsTop) = _squareGrid2d.GetCellBoundsComplementPair(i);//todo is squashed along the z or 2d y, and height position is wrong

                //todo make a method to do both
                //bottom
                var scaleToFitBoundsBottom = new Vector3((desiredBoundsBottom.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.x,
                    desiredBoundsBottom.size.y / currentBoundsSizeBottom.y, (desiredBoundsBottom.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeBottom.z);

                obstacleBottom.transform.localScale = scaleToFitBoundsBottom;
                obstacleBottom.transform.SetParent(_bottomWall.transform);
                obstacleBottom.transform.position = new Vector3(desiredBoundsBottom.center.x, _bottomWall.transform.position.y + desiredBoundsBottom.extents.y * _bottomWall.transform.up.y, desiredBoundsBottom.center.z);

                //top
                var obstacleTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                var scaleToFitBoundsTop = new Vector3((desiredBoundsTop.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSizeTop.x,
                    desiredBoundsTop.size.y / currentBoundsSizeTop.y, (desiredBoundsTop.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSizeTop.z);
                
                obstacleTop.transform.localScale = scaleToFitBoundsTop;
                obstacleTop.transform.SetParent(_topWall.transform);
                obstacleTop.transform.position = new Vector3(desiredBoundsTop.center.x, _topWall.transform.position.y + desiredBoundsTop.extents.y * _topWall.transform.up.y, desiredBoundsTop.center.z);
            }
        }
    }
}
