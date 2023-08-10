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
}