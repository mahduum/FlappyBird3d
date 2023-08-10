using UnityEngine;

namespace Utilities.Grid
{
    public interface IGridInfoProvider
    {
        int NumCellsX { get; }
        int NumCellsY { get; }
        float AbsYOffset { get; }
        Vector2 CellSize { get; }
        (int row, int column) GetRowAndColumnFromIndex(int index);
        public Vector3 GetCellWorldPositionNoYOffset(int row, int column);
    }
}