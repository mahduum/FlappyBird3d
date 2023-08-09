using UnityEngine;
using Random = UnityEngine.Random;

namespace Utilities.Grid
{
    public class SimpleGridObstacleDataCreator : IGridElementMaker<ObstacleData>
    {
        private GridElementsSetting _gridElementsSetting;
        public bool SkipEverySecondRow { get; } = true;
        
        //todo
        public SimpleGridObstacleDataCreator(GridElementsSetting gridElementsSetting)
        {
            _gridElementsSetting = gridElementsSetting;
        }
        public ObstacleData Create(IGridInfoProvider gridInfoProvider, int index)
        {
            //todo set elements with seed
            //var elementSeed = gridInfoProvider.Origin.magnitude + index;

            if (ShouldSkip(gridInfoProvider, index) || ShouldSkipRandom())
            {
                return new ObstacleData(gridInfoProvider, index);
            }

            var topToBottomHeight = gridInfoProvider.AbsYOffset * 2;
            var maxHeight = _gridElementsSetting.ElementMaxHeightLimit * topToBottomHeight;
            var minHeight = _gridElementsSetting.ElementMinHeightLimit * topToBottomHeight;
            
            var bottomHeight = Random.value * maxHeight;
            var topHeight = maxHeight - bottomHeight;
            
            bool hasBottom = bottomHeight > minHeight;
            bool hasTop = topHeight > minHeight;

            var gapSize = 0.0f;
            if (hasBottom && hasTop)
            {
                gapSize = _gridElementsSetting.GapSize * topToBottomHeight;
                var currentGap = (topToBottomHeight - bottomHeight - topHeight);
                var gapDelta = currentGap - gapSize;

                if (bottomHeight > topHeight)
                {
                    bottomHeight += gapDelta;
                }
                else
                {
                    topHeight += gapDelta;
                }
            }
            
            return new ObstacleData(gridInfoProvider, index)
            {
                BottomSegmentHeight = hasBottom ? bottomHeight : 0,
                TopSegmentHeight = hasTop ? topHeight : 0,
                GapSize = gapSize,
                Type = ObstacleType.Column
            };
        }

        private bool ShouldSkip(IGridInfoProvider gridInfoProvider, int index)
        {
            (int row, int column) = gridInfoProvider.GetRowAndColumnFromIndex(index);
            return SkipEverySecondRow && row % 2 == 0;
        }

        private bool ShouldSkipRandom() => Random.value > _gridElementsSetting.ElementProbability;
    }
    
    public interface IGridElementMaker<out TGridElement>
    {
        TGridElement Create(IGridInfoProvider gridInfoProvider, int index);
    }
    
    public interface IGridInfoProvider
    {
        int NumCellsX { get; }
        int NumCellsY { get; }
        float AbsYOffset { get; }
        Vector2 CellSize { get; }
        (int row, int column) GetRowAndColumnFromIndex(int index);
        public Vector3 GetCellWorldPositionNoYOffset(int row, int column);
    }
    
    public interface IHasBounds
    {
    }

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

        public TGridElement[] Elements;

        private TGridElementMaker _elementMaker;

        public Grid2d(int numCellsX, int numCellsY, Vector3 origin, Vector2 size,
            TGridElementMaker elementMaker) //todo inject height limits provider??? or noise generator??? 
        {
            NumCellsX = numCellsX;
            NumCellsY = numCellsY;
            Origin = new Vector2(origin.x + size.x / 2, origin.z - size.y / 2);
            AbsYOffset = Mathf.Abs(origin.y);
            Size = size;
            _elementMaker = elementMaker;
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

        /*todo implement row and diagonal spacings such that we can do this:
            _____
            |***|
            |...|
            |***|
            -----
            
            and this:
            _____
            |*.*|
            |.*.|
            |*.*|
            -----
            
            obstacles maker can implement this
         */
    }
    
    public class SquareGrid2d<TGridElement, TGridElementMaker> : Grid2d<TGridElement, TGridElementMaker>
        where TGridElementMaker : IGridElementMaker<TGridElement>
        where TGridElement : IHasBounds
    {
        public SquareGrid2d(int numCells, Vector3 origin, Vector2 size, TGridElementMaker obstacleMaker) : base(numCells, numCells, origin, size, obstacleMaker)
        {
            
        }
        
        public void ResizeGrid(int numCells)
        {
            //todo (or remake grid?)
        }
    }
    
    public struct ObstacleData : IHasBounds
    {
        public ObstacleData(IGridInfoProvider owner, int index)
        {
            Owner = owner;
            Index = index;
            BottomSegmentHeight = 0;
            TopSegmentHeight = 0;
            GapSize = 0;
            Type = ObstacleType.None;
        }

        public int Index { get; set; }
        private IGridInfoProvider Owner { get; }
        public float BottomSegmentHeight { get; set; }
        public float TopSegmentHeight { get; set; }
        public ObstacleType Type { get; set; }
        public bool HasGap => GapSize > 0;
        public float GapSize { get; set; }

        public (Bounds? bottomSegmentBounds, Bounds? topSegmentBounds) GetBounds()
        {
            (int row, int column) = Owner.GetRowAndColumnFromIndex(Index);
            var worldPositionNoYOffset = Owner.GetCellWorldPositionNoYOffset(row, column);

            Bounds? bottom = null;
            if (BottomSegmentHeight > 0)
            {
                var center = new Vector3(worldPositionNoYOffset.x, -Owner.AbsYOffset + BottomSegmentHeight/2, worldPositionNoYOffset.z);
                bottom = new Bounds(center, new Vector3(Owner.CellSize.x, BottomSegmentHeight, Owner.CellSize.y));
            }
            
            Bounds? top = null;
            if (TopSegmentHeight > 0)
            {
                var center = new Vector3(worldPositionNoYOffset.x, Owner.AbsYOffset - TopSegmentHeight/2, worldPositionNoYOffset.z);
                top = new Bounds(center, new Vector3(Owner.CellSize.x, TopSegmentHeight, Owner.CellSize.y));
            }

            return (bottom, top);
        }
    }
    
    public enum ObstacleType
    {
        None,
        Column,
    }
}