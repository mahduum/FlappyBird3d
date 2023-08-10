namespace Utilities.Scores
{
    public interface IScore
    {
        int GetPoints { get; }
    }

    public struct GapScore : IScore
    {
        public int GetPoints => 10;//todo maybe make it dependable from gap size?
    }
    
    public struct BonusScore : IScore
    {
        public int GetPoints => 100;//todo implement for concrete rewards
    }
}