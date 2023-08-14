using UnityEngine;

namespace Utilities.Grid
{
    public enum ObstacleType
    {
        None,
        Column,
    }
    
    public interface IHasBounds
    {
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
        public float GapSize { get; set; }//or total height minus bottom minus top

        public (Bounds? bottomSegmentBounds, Bounds? topSegmentBounds, Bounds? middleSegmentBounds) GetBounds()//cache this
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

            Bounds? middle = null;
            if (bottom.HasValue && top.HasValue)
            {
                var bottomTopY = bottom.Value.center.y + bottom.Value.extents.y;
                var topBottomY = top.Value.center.y - top.Value.extents.y;
                var middleSegmentCenterY = Mathf.Lerp(bottomTopY, topBottomY, 0.5f);
                var center = new Vector3(worldPositionNoYOffset.x, middleSegmentCenterY, worldPositionNoYOffset.z);
                middle = new Bounds(center, new Vector3(Owner.CellSize.x, GapSize, Owner.CellSize.y));
                //or:
                //var YCoord = -Owner.AbsYOffset + BottomSegmentHeight + GapSize / 2;
            }

            return (bottom, top, middle);
        }
    }
}