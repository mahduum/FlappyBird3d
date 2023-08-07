using System;
using SOs;
using TMPro;
using UniRx;
using UnityEngine;

namespace Behaviors
{
    public class CountdownCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _counter;
        [SerializeField] private SO_StateChannel _stateChannel;
        private readonly IntReactiveProperty _remainingSeconds = new();
        private IDisposable _counterFader;

        private void Awake()
        {
            _remainingSeconds.DistinctUntilChanged().Skip(1).Subscribe(seconds =>
            {
                _counter.text = seconds.ToString();
                StartAlphaDecrease();
            }).AddTo(_counter);
            _stateChannel.OnCountdown += OnCountdown;
        }

        private void OnDestroy()
        {
            _stateChannel.OnCountdown -= OnCountdown;
        }

        public void OnCountdown(int seconds)
        {
            _remainingSeconds.Value = seconds;
        }

        public void OnCountdownFinished()
        {
            _counter.alpha = 0;
        }
        
        /*1000ms, interval every frame, */
        
        private void StartAlphaDecrease()
        {
            _counterFader?.Dispose();
            _counter.alpha = 1.0f;
            _counterFader = Observable.EveryUpdate().TakeWhile(_ => _counter.alpha > 0).Subscribe(_ =>
            {
                _counter.alpha -= Time.deltaTime;
            }).AddTo(_counter);
        }
    }
}
