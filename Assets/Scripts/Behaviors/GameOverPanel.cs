using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Utilities.Scores;

namespace Behaviors
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _highScorePanel;
        [SerializeField] private Button[] _gameOverButtons;
        [SerializeField] private TMP_InputField _highScoreNameInput;
        [SerializeField] private TextMeshProUGUI _highScoreText;
        
        private long _highScore;
        private string _date;
        
        private IDisposable _highScoreDisposable;

        private void OnEnable()
        {
            _highScorePanel.SetActive(false);
            EnableGameOverButtons(false);
            _date = DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture);

            Score.HighScoreAsync.ContinueWith(score =>
            {
                var currentTotalScore = ScoreManager.CurrentTotalScore;
                if (score.GetValueOrDefault().Score < currentTotalScore)
                {
                    _highScore = currentTotalScore;
                    _highScorePanel.SetActive(true);
                    _highScoreText.text = _highScore.ToString();
                    _highScoreNameInput.ActivateInputField();
                }
                else
                {
                    EnableGameOverButtons(true);
                }
            });
        }

        private void EnableGameOverButtons(bool enable)
        {
            foreach (var button in _gameOverButtons)
            {
                button.enabled = enable;
            }
        }

        public void OnEditEnd(string entry)
        {
            var newHighScore = new ScoreInfo()
            {
                PlayerName = entry,
                Score = _highScore,
                Date = _date
            };
            
            _highScoreNameInput.DeactivateInputField();
            _highScoreDisposable?.Dispose();
            _highScoreDisposable = Score.SaveHighScoreAsObservable(newHighScore)
                .Subscribe(willBeSaved =>
            {
                _highScoreNameInput.DeactivateInputField();
                _highScorePanel.SetActive(false);
                EnableGameOverButtons(true);
            }).AddTo(this);
        }
    }
}