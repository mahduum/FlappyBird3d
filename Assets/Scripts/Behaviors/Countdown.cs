using System;
using UniRx;

namespace Behaviors
{
    public class Countdown : StateBase
    {
        const int CountTo = 3;
        private IDisposable _counter;

        public Countdown(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            GameplayManager.CurrentSegment.Value = 0;
            _counter = Observable.Interval(TimeSpan.FromSeconds(1)).Take(CountTo).Subscribe(seconds =>
                {
                    int displaySeconds = CountTo - (int)seconds;
                    GameplayManager.StateChannel.RaiseOnCountdown(displaySeconds);
                }, () =>
                {
                    GameplayManager.SetState(new Flight(GameplayManager));
                    GameplayManager.SetGameTimeCounter();
                })
                .AddTo(GameplayManager);
        }

        protected override void DisposeInternal()
        {
            _counter.Dispose();
        }
    }
}