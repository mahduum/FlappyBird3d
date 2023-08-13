using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Utilities.Saves;
using Application = UnityEngine.Device.Application;

namespace Utilities.Scores
{
    public static class Score
    {
        private static char SEP = Path.DirectorySeparatorChar;
        private static readonly string SCORES_FOLDER_PATH = Application.persistentDataPath + SEP + "HighScores" + SEP;
        private static readonly string FILE_BASE_NAME = "HighScore";
        private const int CAPACITY = 20;

        private static ScoreInfo? _highScore;
        public static ScoreInfo? HighScore
        {
            get
            {
                if (_highScore.HasValue == false)
                {
                    var score = SavesHelper.LoadMostRecentFiles(SCORES_FOLDER_PATH);
                    if (score.Length < 1 || string.IsNullOrEmpty(score[0]))
                    {
                        return null;
                    }

                    _highScore = JsonUtility.FromJson<ScoreInfo>(score[0]);
                }

                return _highScore;
            }
        }
        
        public static UniTask<ScoreInfo?> HighScoreAsync
        {
            get
            {
                if (_highScore.HasValue == false)
                {
                    var score = SavesHelper.LoadMostRecentFilesAsync(SCORES_FOLDER_PATH);
                    return score.ContinueWith(result =>
                    {
                        if (result.Length < 1 || string.IsNullOrEmpty(result[0]))
                        {
                            return UniTask.FromResult<ScoreInfo?>(null);
                        }

                        _highScore = JsonUtility.FromJson<ScoreInfo>(result[0]);
                        return UniTask.FromResult<ScoreInfo?>(_highScore);
                    });
                }
                
                return UniTask.FromResult<ScoreInfo?>(_highScore);
            }
        }

        public static ScoreInfo[] LoadRecentScores(int maxCount)
        {
            var contents = SavesHelper.LoadMostRecentFiles(SCORES_FOLDER_PATH, maxCount);
            return contents.Select(JsonUtility.FromJson<ScoreInfo>).ToArray();
        }
        
        public static UniTask<ScoreInfo[]> LoadRecentScoresAsync()
        {
            var contents = SavesHelper.LoadMostRecentFilesAsync(SCORES_FOLDER_PATH, CAPACITY);
            return contents.ContinueWith(c => c.Select(JsonUtility.FromJson<ScoreInfo>).ToArray());
        }

        public static IObservable<bool> SaveHighScoreAsObservable(ScoreInfo scoreInfo)
        {
            return Observable.Create<bool>(observer =>
            {
                var lastHighScore = HighScoreAsync.ToObservable();
                bool isHighScore = false;
                lastHighScore.Subscribe(score =>
                    {
                        isHighScore = score.GetValueOrDefault().Score < scoreInfo.Score;
                        observer.OnNext(isHighScore);
                    },
                    () =>
                    {
                        if (isHighScore == false)
                        {
                            observer.OnCompleted();
                            return;
                        }
                        
                        _highScore = scoreInfo;
                        var contents = JsonUtility.ToJson(scoreInfo);
                        SavesHelper.Save(SCORES_FOLDER_PATH, FILE_BASE_NAME, contents, scoreInfo.Score.ToString(), CAPACITY);
                    });
                
                return Disposable.Empty;
            });
        }
    }
}