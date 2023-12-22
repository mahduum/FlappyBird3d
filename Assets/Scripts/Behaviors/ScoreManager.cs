using SOs;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
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
        [FormerlySerializedAs("_highScorePanel")] [SerializeField] private GameOverPanel _gameOverPanel;

        private static readonly IntReactiveProperty GapScore = new IntReactiveProperty();
        private static readonly IntReactiveProperty BonusScore = new IntReactiveProperty();
        public static long CurrentTotalScore => GameplayManager.GameTimeSeconds.Value + GapScore.Value + BonusScore.Value;

        private void Awake()
        {
            _scoreUpdateChannel.OnScore += OnScore;
            _stateChannel.OnReset += OnReset;
            _stateChannel.OnGameOver += OnGameOver;
            GapScore.DistinctUntilChanged().Subscribe(points => _gapScoreText.text = $"GAPS: {points}").AddTo(this);
            BonusScore.DistinctUntilChanged().Subscribe(points => _bonusScoreText.text = $"BONUS: {points}").AddTo(this);
            GameplayManager.GameTimeSeconds.DistinctUntilChanged().Subscribe(points => _timeScoreText.text = $"TIME: {points}").AddTo(this);
        }

        private void OnDestroy()
        {
            _scoreUpdateChannel.OnScore -= OnScore;
            _stateChannel.OnReset -= OnReset;
            _stateChannel.OnGameOver -= OnGameOver;
        }

        private void OnScore(IScore score)//todo add on signal from DI
        {
            if (score is GapScore { })
            {
                GapScore.Value += score.GetPoints;
            }
            else if (score is BonusScore { })
            {
                BonusScore.Value += score.GetPoints;
            }
        }

        private void OnGameOver()
        {
        }

        public void OnReset()
        {
            GapScore.Value = 0;
            BonusScore.Value = 0;
        }
    }
}
