using System;
using JetBrains.Annotations;
using SOs;
using UniRx;
using UnityEngine;
using Utilities.Scores;

namespace Behaviors
{
    public class GameplayManager : StateMachineBase
    {
        [SerializeField] private SO_StateChannel _stateChannel;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
        [SerializeField] private SO_SpeedThresholds _speedThresholds;
        [SerializeField] private GameOverPanel _gameOver;
        [SerializeField] private GameObject _gamePaused;
        [SerializeField] private WallsManager _wallsManager;

        public SO_StateChannel StateChannel => _stateChannel;
        public SO_SegmentUpdateChannel SegmentUpdateChannel => _segmentUpdateChannel;
        public SO_SpeedThresholds SpeedThresholds => _speedThresholds;
        public GameOverPanel GameOver => _gameOver;
        public GameObject GamePaused => _gamePaused;
        public WallsManager WallsManager => _wallsManager;

        public static readonly IntReactiveProperty CurrentSegment = new();
        //public IReadOnlyReactiveProperty<int> CurrentSegment => _currentSegment;
        public static readonly IntReactiveProperty GameTimeSeconds = new();
        private IDisposable _gameTimer;

        public static ScoreInfo? CachedHighScore;

        
        public void ResetGameTimer()
        {
            _gameTimer?.Dispose();
        }
        
        public void SetGameTimeCounter()
        {
            _gameTimer?.Dispose();
            _gameTimer = Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Subscribe(c =>
                {
                    var seconds = (int) c;
                    GameTimeSeconds.Value = seconds;
            }).AddTo(this);
        }
        
        private void Awake()
        {
            _stateChannel.OnGameOver += OnGameOver;
            _stateChannel.OnPaused += OnPaused;
        }

        private void OnDestroy()
        {
            _stateChannel.OnGameOver -= OnGameOver;
            _stateChannel.OnPaused -= OnPaused;
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

        private void OnPaused(bool isPaused)
        {
            if (isPaused)
            {
                State.PauseGame();
            }
        }
    }
}
