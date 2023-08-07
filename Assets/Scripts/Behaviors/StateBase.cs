using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Behaviors
{
    public abstract class StateBase
    {
        protected GameplayManager GameplayManager;
        protected StateBase(GameplayManager gameplayManager)
        {
            GameplayManager = gameplayManager;
            Time.timeScale = 1;
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

        public virtual void SetSpeed()
        {
            
            
        }

        public virtual void SetInput()
        {
            
        }
        
    }
    
    //states: Loading, CountdownCounter, Paused, GameOver, Flight, 
    //public class

    public class Countdown : StateBase
    {
        const int CountTo = 3;

        public Countdown(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            //todo set gravity off
            GameplayManager.CurrentSegment.Value = 0;
            Observable.Interval(TimeSpan.FromSeconds(1)).Take(CountTo).Subscribe(seconds =>
            {
                int displaySeconds = CountTo - (int)seconds;
                GameplayManager.StateChannel.OnCountdown.Invoke(displaySeconds);
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
            //todo set gravity on
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

        public override void SetSpeed()//how to get the last speed???
        {
            base.SetSpeed();
            
        }
    }

    public class Pause : StateBase//todo 
    {
        public Pause(GameplayManager gameplayManager) : base(gameplayManager)
        {
            //optionally remember the exact current speed of simply pause the game?
        }

        public override void Start()
        {
            Time.timeScale = 0;//cache and set back to cached
            //show the game paused screen, subscribe to resume button
        }
    }
}