using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Behaviors
{
    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private SO_GameStateChannel _gameStateChannel;
        [SerializeField] private UnityEvent _onGameStart;
        [SerializeField] private UnityEvent _onGameOver;
        [SerializeField] private UnityEvent<int> _onGameCountDown;
        
        const int CountTo = 3;
        //todo use button start, but later switch it to timed start.
        //or use timed start anyway
        private void Awake()
        {
            _gameStateChannel.OnGameStateChanged += OnGameStateChanged;
        }

        void Start()
        {
            enabled = false;
            Observable.Interval(TimeSpan.FromSeconds(1)).Take(CountTo).Subscribe(seconds =>
            {
                int displaySeconds = CountTo - (int)seconds;
                _onGameCountDown.Invoke(displaySeconds);
            }, () => _onGameStart.Invoke()).AddTo(this);
        }

        void OnGameStateChanged(GameState gameState)//todo maybe call directly?
        {
            if (gameState == GameState.GameOver)
            {
                _onGameOver.Invoke();//todo show game over panel, stop player
            }
        }
    }
}
