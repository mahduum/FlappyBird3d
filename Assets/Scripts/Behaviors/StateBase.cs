using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using static Behaviors.LoadingManager;

namespace Behaviors
{
    public abstract class StateBase : IDisposable
    {
        protected readonly GameplayManager GameplayManager;
        protected StateBase(GameplayManager gameplayManager)
        {
            GameplayManager = gameplayManager;
        }

        public abstract void Start();

        public virtual void KeepScore()
        {
            
        }

        public virtual void PauseGame()
        {
        }

        public virtual void ResumeGame()
        {
        }

        public virtual async void Exit()
        {
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        protected abstract void DisposeInternal();
    }

    public class Reset : StateBase
    {
        public Reset(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            GameplayManager.StateChannel.RaiseOnReset();
            GameplayManager.SetState(new Countdown(GameplayManager));
        }

        protected override void DisposeInternal()
        {
        }
    }

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
            }, () => GameplayManager.SetState(new Flight(GameplayManager))).AddTo(GameplayManager);
        }

        protected override void DisposeInternal()
        {
            _counter.Dispose();
        }
    }

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

    public class Pause : StateBase
    {
        private float _timeScale;

        public Pause(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 0;
            GameplayManager.GamePaused.SetActive(true);
        }

        public override void ResumeGame()
        {
            GameplayManager.GamePaused.SetActive(false);
            GameplayManager.SetState(new Flight(GameplayManager));
            Time.timeScale = _timeScale;
        }

        public override async void Exit()
        {
            Time.timeScale = 1;
            await LoadMainMenuAsSingleScene();
        }


        protected override void DisposeInternal()
        {
        }
    }
    
    public class GameOver : StateBase
    {
        public GameOver(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            Time.timeScale = 0;
            GameplayManager.GameOver.SetActive(true);
            GameplayManager.CurrentSegment.Value = 0;
            //todo prompt saving and showing all the results
        }

        public override void ResumeGame()
        {
            GameplayManager.GameOver.SetActive(false);
            GameplayManager.SetState(new Reset(GameplayManager));
            Time.timeScale = 1;
        }

        public override async void Exit()
        {
            Time.timeScale = 1;
            await LoadMainMenuAsSingleScene();
        }
        
        protected override void DisposeInternal()
        {
        }
    }
}