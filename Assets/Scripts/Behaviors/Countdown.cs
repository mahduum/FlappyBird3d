using System;
using TMPro;
using UniRx;
using UnityEngine;

namespace Behaviors
{
    public class Countdown : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _counter;

        private readonly IntReactiveProperty _remainingSeconds = new IntReactiveProperty();
        private IDisposable _counterFader;

        private void Awake()
        {
            _remainingSeconds.DistinctUntilChanged().Skip(1).Subscribe(seconds =>
            {
                _counter.text = seconds.ToString();
                StartAlphaDecrease();
            }).AddTo(_counter);
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
