using System;
using JetBrains.Annotations;
using SOs;
using TMPro;
using UniRx;
using UnityEngine;
using Utilities.Scores;

namespace Behaviors
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private SO_ScoreUpdateChannel _scoreUpdateChannel;
        [SerializeField] private SO_StateChannel _stateChannel;

        [SerializeField] private TextMeshProUGUI _timeScoreText;//emit every second until reactive property in play is active
        [SerializeField] private TextMeshProUGUI _gapScoreText;
        [SerializeField] private TextMeshProUGUI _bonusScoreText;
        // Start is called before the first frame update
        
        //todo on game over event save the score
        //todo on game start reset the score

        private readonly IntReactiveProperty _gapScore = new IntReactiveProperty();
        private readonly IntReactiveProperty _bonusScore = new IntReactiveProperty();
        private readonly IntReactiveProperty _timeScore = new IntReactiveProperty();

        private void Awake()
        {
            _scoreUpdateChannel.OnScore += OnScore;
            _stateChannel.OnGameOver += OnGameOver;
            _gapScore.DistinctUntilChanged().Subscribe(points => _gapScoreText.text = $"GAPS: {points}").AddTo(this);
            _bonusScore.DistinctUntilChanged().Subscribe(points => _bonusScoreText.text = $"BONUS: {points}").AddTo(this);
            _timeScore.DistinctUntilChanged().Subscribe(points => _timeScoreText.text = $"TIME: {points}").AddTo(this);
        }

        private void OnDestroy()
        {
            _scoreUpdateChannel.OnScore -= OnScore;
            _stateChannel.OnGameOver -= OnGameOver;
        }

        private void OnScore(IScore score)
        {
            if (score is GapScore { })
            {
                _gapScore.Value += score.GetPoints;
            }
            else if (score is BonusScore { })
            {
                _bonusScore.Value += score.GetPoints;
            }
        }

        public void OnGameOver()
        {
            //todo first save the score, show the result, and then reset variables
            _gapScore.Value = 0;
            _bonusScore.Value = 0;
            _timeScore.Value = 0;
        }
    }
}
