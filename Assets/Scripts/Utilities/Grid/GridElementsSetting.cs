
namespace Utilities.Grid
{
    public struct GridElementsSetting
    {
        public GridElementsSetting((int row, int colums) maxElementsPerSide, float elementMinHeightLimit,
            float elementMaxHeightLimit, float gapSize, float elementProbability)
        {
            MaxElementsPerSide = maxElementsPerSide;
            ElementMinHeightLimit = elementMinHeightLimit;
            ElementMaxHeightLimit = elementMaxHeightLimit;
            GapSize = gapSize;
            ElementProbability = elementProbability;
        }
        
        public (int, int) MaxElementsPerSide { get; }
        public float ElementMinHeightLimit { get; }
        public float ElementMaxHeightLimit { get; }
        public float GapSize { get; }
        public float ElementProbability { get; }

    }
}