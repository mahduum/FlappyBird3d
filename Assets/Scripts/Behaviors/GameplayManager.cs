using System;
using JetBrains.Annotations;
using SOs;
using UniRx;
using UnityEngine;

namespace Behaviors
{
    public class GameplayManager : StateMachineBase
    {
        [SerializeField] private SO_StateChannel _stateChannel;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
        [SerializeField] private SO_SpeedThresholds _speedThresholds;
        [SerializeField] private GameObject _gameOver;
        [SerializeField] private GameObject _gamePaused;
        [SerializeField] private WallsManager _wallsManager;

        public SO_StateChannel StateChannel => _stateChannel;
        public SO_SegmentUpdateChannel SegmentUpdateChannel => _segmentUpdateChannel;
        public SO_SpeedThresholds SpeedThresholds => _speedThresholds;
        public GameObject GameOver => _gameOver;
        public GameObject GamePaused => _gamePaused;
        public WallsManager WallsManager => _wallsManager;

        public static IntReactiveProperty CurrentSegment = new();
        //public IReadOnlyReactiveProperty<int> CurrentSegment => _currentSegment;

        //todo use button start, but later switch it to timed start.
        //or use timed start anyway
        private void Awake()
        {
            _stateChannel.OnGameOver += OnGameOver;
        }

        private void OnDestroy()
        {
            _stateChannel.OnGameOver -= OnGameOver;
        }

        void Start()
        {
            if (State == null)
            {
                SetState(new Countdown(this));
            }
        }

        private void OnGameOver()
        {
            SetState(new GameOver(this));
        }

        [UsedImplicitly]
        public void OnClickPause()
        {
            State.PauseGame();
        }

        [UsedImplicitly]
        public void OnClickResume()
        {
            State.ResumeGame();
        }

        [UsedImplicitly]
        public void OnClickMainMenu()
        {
            State.Exit();
        }
        
        [UsedImplicitly]
        public void OnClickRetry()
        {
            State.ResumeGame();
        }
    }
}
