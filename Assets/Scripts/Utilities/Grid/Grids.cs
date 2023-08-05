using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities.Grid
{
    public class SimpleGridObstacleDataCreator : IGridElementMaker<ObstacleData>
    {
        public float MaxHeight { get; }
        //todo
        public SimpleGridObstacleDataCreator(float maxHeight)
        {
            MaxHeight = maxHeight;
        }
        public ObstacleData Create(IGridInfoProvider gridInfoProvider, int index)
        {
            //todo set elements with seed
            var elementSeed = gridInfoProvider.Origin.magnitude + index;
            var height = Random.value * MaxHeight;
            if (height > MaxHeight / 2 && Random.value < 0.8f)
            {
                height = MaxHeight - height;
            }
            
            return new ObstacleData() {Height = height, Type = ObstacleType.Column};
        }
    }
    
    public interface IGridElementMaker<out TGridElement>
    {
        TGridElement Create(IGridInfoProvider gridInfoProvider, int index);
    }
    
    public interface IGridInfoProvider
    {
        int NumCellsX { get; }
        int NumCellsY { get; }
        Vector2 Origin { get; set; }
    }
    
    public interface IHasHeight
    {
        float Height { get; }
    }
    
    public class Grid2d<TGridElement, TGridElementMaker> : IGridInfoProvider where TGridElementMaker : IGridElementMaker<TGridElement> where TGridElement : IHasHeight
    {
        public int NumCellsX { get; }
        public int NumCellsY { get; }
        public Vector2 Origin { get; set; }
        
        public float YOffset { get; }
        public Vector2 Size { get; set; }
    
        public Vector2 CellSize//todo cache
        {
            get
            {
                var sizeX = Size.x / NumCellsX;
                var sizeY = Size.y / NumCellsY;
                return new Vector2(sizeX, sizeY);
            }
        }
    
        public TGridElement[] Elements;
    
        private TGridElementMaker ElementMaker;
    
        public Grid2d(int numCellsX, int numCellsY, Vector3 origin, Vector2 size, TGridElementMaker elementMaker)//todo inject height limits provider??? or noise generator??? 
        {
            NumCellsX = numCellsX;
            NumCellsY = numCellsY;
            Origin = new Vector2(origin.x + size.x/2, origin.z - size.y/2);
            YOffset = origin.y;
            Size = size;
            ElementMaker = elementMaker;
            Elements = new TGridElement[NumCellsX * numCellsY];
        }
    
        public void ResizeGrid(int numCellsX, int numCellsY)
        {
            //todo (or make new grid)
        }
    
        public void UpdateOffset()
        {
            //todo
        }
    
        public void CreateElements()
        {
            for (int i = 0; i < Elements.Length; i++)
            {
                Elements[i] = ElementMaker.Create(this, i);
            }
        }
    
        public TGridElement GetElement(Vector2 position)
        {
            var localCoordinate = position - Origin;
            (int gridSpaceX, int gridSpaceY) = ((int)(Size.x / localCoordinate.x), (int)(Size.y / localCoordinate.y));
            var elementIndex = gridSpaceY * NumCellsY + gridSpaceX;
            return Elements[elementIndex];
        }
    
        public Bounds GetCellBounds(int index)
        {
            var gridSpaceY = index / NumCellsX;
            var gridSpaceX = gridSpaceY * NumCellsX - index;
            var center = new Vector3(Origin.x + (CellSize.x * gridSpaceX) - CellSize.x / 2, YOffset,
                Origin.y + (CellSize.y * gridSpaceY) - CellSize.y / 2);
            var cellHeight = Elements.Length > index ? Elements[index].Height : 0;
            var bounds = new Bounds(center, new Vector3(CellSize.x, cellHeight, CellSize.y));
            return bounds;
        }
    }
    
    public class SquareGrid2d<TGridElement, TGridElementMaker> : Grid2d<TGridElement, TGridElementMaker> where TGridElementMaker : IGridElementMaker<TGridElement> where TGridElement : IHasHeight
    {
        public SquareGrid2d(int numCells, Vector3 origin, Vector2 size, TGridElementMaker obstacleMaker) : base(numCells, numCells, origin, size, obstacleMaker)
        {
            
        }
        
        public void ResizeGrid(int numCells)
        {
            //todo (or make new grid)
        }
    }
    
    public class ObstacleData : IHasHeight//todo maybe replace data with actual behaviours?
    {
        public float Height { get; set; }
        public ObstacleType Type { get; set; }
    }
    
    public enum ObstacleType
    {
        None,
        Column,
    }
}