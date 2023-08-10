using System;
using UniRx;

namespace Behaviors
{
    public class Flight : StateBase
    {
        private IDisposable _segmentUpdate;
        private IDisposable _segmentUpdate2;

        public Flight(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            var currentSegment = GameplayManager.CurrentSegment;
            GameplayManager.SegmentUpdateChannel.OnSegmentExit += IncrementSurpassedSegmentsCount;
            _segmentUpdate = currentSegment
                .DistinctUntilChanged()
                .Subscribe(count =>
                {
                    //check if the speed can be changed
                    var speeds = GameplayManager.SpeedThresholds.ThresholdSpeeds;
                    foreach (var pair in speeds)
                    {
                        if (pair.Threshold == count)
                        {
                            GameplayManager.StateChannel.RaiseOnSpeedChanged(pair.Speed);//todo maybe player should be responsible for lookup logic?
                            break;
                        }
                    }
                    
                }).AddTo(GameplayManager);

            _segmentUpdate2 =
                currentSegment
                    .Skip(2)
                    .Subscribe(_ => 
                        GameplayManager.WallsManager.UpdateSegmentsPosition())
                    .AddTo(GameplayManager);
        }

        public override void PauseGame()
        {
            GameplayManager.SetState(new Pause(GameplayManager));
        }

        private static void IncrementSurpassedSegmentsCount()
        {
            GameplayManager.CurrentSegment.Value++;
        }

        protected override void DisposeInternal()
        {
            GameplayManager.SegmentUpdateChannel.OnSegmentExit -= IncrementSurpassedSegmentsCount;
            _segmentUpdate.Dispose();
            _segmentUpdate2.Dispose();
        }
    }
}