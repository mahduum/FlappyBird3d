using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using static Behaviors.LoadingManager;

namespace Behaviors
{
    public abstract class StateBase
    {
        protected GameplayManager GameplayManager;
        protected StateBase(GameplayManager gameplayManager)
        {
            GameplayManager = gameplayManager;
        }

        ~StateBase()
        {
            Debug.Log($"Destructor called on {this}");
        }

        public virtual void Start()
        {
            //return observable???
        }

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
    }

    public class Countdown : StateBase
    {
        const int CountTo = 3;

        public Countdown(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            GameplayManager.CurrentSegment.Value = 0;
            Observable.Interval(TimeSpan.FromSeconds(1)).Take(CountTo).Subscribe(seconds =>
            {
                int displaySeconds = CountTo - (int)seconds;
                GameplayManager.StateChannel.RaiseOnCountdown(displaySeconds);
            }, () => GameplayManager.SetState(new Flight(GameplayManager))).AddTo(GameplayManager);
        }
    }

    public class Flight : StateBase
    {
        public Flight(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            GameplayManager.CurrentSegment
                .DistinctUntilChanged()
                .Subscribe(count =>
                {
                    Debug.Log($"Flight update on threshold: {count}");
                    //check if the speed can be changed
                    var speeds = GameplayManager.SpeedThresholds.ThresholdSpeeds;
                    foreach (var pair in speeds)
                    {
                        if (pair.Threshold == count)
                        {
                            GameplayManager.StateChannel.RaiseOnSpeedChanged(pair.Speed);
                            break;
                        }
                    }
                });
        }

        public override void PauseGame()
        {
            GameplayManager.SetState(new Pause(GameplayManager));
        }
    }

    public class Pause : StateBase
    {
        private float timeScale;
        public Pause(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            timeScale = Time.timeScale;
            Time.timeScale = 0;//cache and set back to cached
            GameplayManager.GamePaused.SetActive(true);
            //show the game paused screen, subscribe to resume button
        }

        public override void ResumeGame()
        {
            GameplayManager.GamePaused.SetActive(false);
            GameplayManager.SetState(new Flight(GameplayManager));
            Time.timeScale = timeScale;
        }

        public override async void Exit()
        {
            Time.timeScale = 1;
            await LoadMainMenuAsSingleScene();
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
            //prompt saving and showing all the results
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
    }
}