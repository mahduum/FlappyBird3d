using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using Utilities.Scores;

public class SlotHighscore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerName;
    [SerializeField] private TextMeshProUGUI _score;
    [SerializeField] private TextMeshProUGUI _date;
    
    public int Index { get; set; }
    private ScoreInfo _scoreInfo;

    public void Set(ScoreInfo scoreInfo, int index)
    {
        _scoreInfo = scoreInfo;
        _playerName.text = scoreInfo.PlayerName;
        _score.text = scoreInfo.Score.ToString();
        _date.text = scoreInfo.Date.ToString(CultureInfo.InvariantCulture);
        Index = index;
    }
}
