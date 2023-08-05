using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities.Grid;

namespace Behaviors
{
    public class Wall : MonoBehaviour//todo add interface and pass it as data initializer to grid
    {
        // Base cells divisions based on level of difficulty later, for now set it as serialized fields
        [SerializeField] private int _gridCellsOnSide = 8;
        [SerializeField] private AssetReference _obstacleTypeA;
        [SerializeField] private float _maxHeight = 10;
        [SerializeField] private Renderer _renderer;
        [Range(0, 99)]
        [SerializeField] private int _obstacleBoundsReductionPercent = 10;
    
        //todo texture to visualize perlin noise
        public Bounds Bounds => _renderer.bounds;
        private float ObstacleBoundsReductionMultiplier => (100 - _obstacleBoundsReductionPercent) / 100.0f;

        private SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator> _squareGrid2d;
    
        //todo provide common perlin noise to fill the grid with values
        private void Awake()
        {
            InitializeGrid();
        }
    
        //todo make a functionality that finds pairs of obstacles from opposite directions and make them clash

        void Start()
        {
            SpawnObstacles();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void UpdateObstacles()
        {
            // prompt modification of obstacles height and modify meshes
        }

        private void SpawnObstacles()
        {
            for (int i = 0; i < _squareGrid2d.Elements.Length; i++)
            {
                var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                var currentBoundsSize = obstacle.GetComponent<Renderer>()?.bounds.size?? Vector3.zero;
                var desiredBounds = _squareGrid2d.GetCellBounds(i);//todo is squashed along the z or 2d y, and height position is wrong
                var scaleToFitBounds = new Vector3((desiredBounds.size.x * ObstacleBoundsReductionMultiplier) / currentBoundsSize.x,
                    desiredBounds.size.y / currentBoundsSize.y, (desiredBounds.size.z * ObstacleBoundsReductionMultiplier) / currentBoundsSize.z);
                obstacle.transform.localScale = scaleToFitBounds;
                obstacle.transform.SetParent(transform);
                obstacle.transform.position = new Vector3(desiredBounds.center.x, transform.position.y + desiredBounds.extents.y * transform.up.y, desiredBounds.center.z);//todo drop y by half max height and then move up by extent y
            }
        }

        private void InitializeGrid()
        {
            var position = transform.position;//todo add offset to the center
            _squareGrid2d ??= new SquareGrid2d<ObstacleData, SimpleGridObstacleDataCreator>(_gridCellsOnSide, position, new Vector2(Bounds.size.x, Bounds.size.z), new SimpleGridObstacleDataCreator(_maxHeight));
            _squareGrid2d.CreateElements();
        }
    }
}