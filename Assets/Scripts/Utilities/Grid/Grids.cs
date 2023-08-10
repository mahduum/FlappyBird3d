using UnityEngine;

namespace Utilities.Grid
{
    public class Grid2d<TGridElement, TGridElementMaker> : IGridInfoProvider
        where TGridElementMaker : IGridElementMaker<TGridElement> where TGridElement : IHasBounds
    {
        public int NumCellsX { get; }
        public int NumCellsY { get; }
        public Vector2 Origin { get; set; }
        public float AbsYOffset { get; }
        public Vector2 Size { get; set; }

        public Vector2 CellSize //todo cache this
        {
            get
            {
                var sizeX = Size.x / NumCellsX;
                var sizeY = Size.y / NumCellsY;
                return new Vector2(sizeX, sizeY);
            }
        }

        public readonly TGridElement[] Elements;

        private TGridElementMaker _elementMaker;

        public Grid2d(int numCellsX, int numCellsY, Vector3 origin, Vector2 size,
            TGridElementMaker elementMaker)
        {
            NumCellsX = numCellsX;
            NumCellsY = numCellsY;
            Origin = new Vector2(origin.x + size.x / 2, origin.z - size.y / 2);
            AbsYOffset = Mathf.Abs(origin.y);
            Size = size;
            _elementMaker = elementMaker;
            Elements = new TGridElement[NumCellsX * numCellsY];
        }

        public void CreateElements()
        {
            for (int i = 0; i < Elements.Length; i++)
            {
                Elements[i] = _elementMaker.Create(this, i);
            }
        }

        public TGridElement GetElement(Vector2 worldPosition)
        {
            var localCoordinate = worldPosition - Origin;
            (int gridSpaceX, int gridSpaceY) = ((int) (Size.x / localCoordinate.x), (int) (Size.y / localCoordinate.y));
            var elementIndex = gridSpaceY * NumCellsX + gridSpaceX;
            return elementIndex < Elements.Length ? Elements[elementIndex] : default;
        }
        
        private TGridElement GetElement(int gridSpaceX, int gridSpaceY)
        {
            var elementIndex = gridSpaceY * NumCellsX + gridSpaceX;
            return elementIndex < Elements.Length ? Elements[elementIndex] : default;
        }

        private (int gridSpaceX, int gridSpaceY) GetGridSpaceCoordsFromIndex(int index)
        {
            var gridSpaceY = index / NumCellsX;
            var gridSpaceX = index - gridSpaceY * NumCellsX;
            return (gridSpaceX, gridSpaceY);
        }

        public Vector3 GetCellWorldPositionNoYOffset(int row, int column)
        {
            return new Vector3(Origin.x - (CellSize.x * column) - CellSize.x / 2, 0.0f,
                Origin.y + (CellSize.y * row) + CellSize.y / 2);
        }

        public (int row, int column) GetRowAndColumnFromIndex(int index)
        {
            (int gridSpaceX, int gridSpaceY) = GetGridSpaceCoordsFromIndex(index);
            return (gridSpaceY, gridSpaceX);
        }
    }
    
    public class SquareGrid2d<TGridElement, TGridElementMaker> : Grid2d<TGridElement, TGridElementMaker>
        where TGridElementMaker : IGridElementMaker<TGridElement>
        where TGridElement : IHasBounds
    {
        public SquareGrid2d(int numCells, Vector3 origin, Vector2 size, TGridElementMaker obstacleMaker) : base(numCells, numCells, origin, size, obstacleMaker)
        {
        }
    }
}