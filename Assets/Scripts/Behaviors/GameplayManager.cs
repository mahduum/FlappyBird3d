using System;
using JetBrains.Annotations;
using SOs;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Behaviors
{
    public class GameplayManager : StateMachineBase
    {
        [SerializeField] private SO_StateChannel _stateChannel;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
        [SerializeField] private SO_SpeedThresholds _speedThresholds;
        [SerializeField] private GameObject _gameOver;
        [SerializeField] private GameObject _gamePaused;

        public SO_StateChannel StateChannel => _stateChannel;
        public SO_SegmentUpdateChannel SegmentUpdateChannel => _segmentUpdateChannel;//todo whatever may need it?
        public SO_SpeedThresholds SpeedThresholds => _speedThresholds;

        public readonly IntReactiveProperty CurrentSegment = new();
        //public IReadOnlyReactiveProperty<int> CurrentSegment => _currentSegment;

        //todo use button start, but later switch it to timed start.
        //or use timed start anyway
        private void Awake()
        {
            _segmentUpdateChannel.OnSegmentExit += IncrementSurpassedSegmentsCount;
            //_stateChannel.OnSpeedChanged += OnSpeedChanged;
            /*control the flow constantly, receive events that:
             - change the speed (each state sets speed and modifies speed, runs a subscription on observable)
             - change the points
             - change the density
             - change level
             */
            /*
             * Set state sets the state in state machine (like here)
             * the internal state of the machine is this state
             * and each method in manager now calls methods through this state
             * while switching state this state can be activated by the call
             * it may for example start executing a particular observable
             * or there may be observables that filter concrete state and react to the state change
             * let's make a state reactive property and with DistinctUntilChanged this reactive property
             * can initiate the state by call on it its state method: "process events"
             * each state will process or receive different events etc.
             * runs observable or a coroutine or based on events observable that reacts to events
             * and thus manages the game
             */
        }

        private void OnDestroy()
        {
            _segmentUpdateChannel.OnSegmentExit -= IncrementSurpassedSegmentsCount;
        }

        void Start()
        {
            SetState(new Countdown(this));
        }

        private void IncrementSurpassedSegmentsCount()
        {
            CurrentSegment.Value++;
        }

        [UsedImplicitly]
        public void OnClickPause()
        {
            
        }
        
        [UsedImplicitly]
        public void OnClickResume()
        {
            
        }

        [UsedImplicitly]
        public void OnClickMainMenu()
        {
            
        }
        
        [UsedImplicitly]
        public void OnClickRetry()
        {
            SetState(new Countdown(this));//before countdown
        }
    }
}
