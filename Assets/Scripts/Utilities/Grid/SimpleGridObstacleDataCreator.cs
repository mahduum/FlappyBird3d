using UnityEngine;

namespace Utilities.Grid
{
    public interface IGridElementMaker<out TGridElement>
    {
        TGridElement Create(IGridInfoProvider gridInfoProvider, int index);
    }
    
    public class SimpleGridObstacleDataCreator : IGridElementMaker<ObstacleData>
    {
        private readonly GridElementsSetting _gridElementsSetting;
        public bool SkipEverySecondRow { get; } = true;
        
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
}