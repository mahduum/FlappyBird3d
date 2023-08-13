using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities.Scores;

public class ManagerHighscores : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject _highScoreSlotTemplate;
    
    private readonly List<SlotHighscore> _instantiated = new ();
    private readonly List<AsyncOperationHandle<GameObject>> _slotHandles = new();

    private void OnEnable()
    {
        Score.LoadRecentScoresAsync().ContinueWith(scores =>
        {
            var i = 0;
            for (; i < scores.Length; i++)
            {
                var scoreInfo = scores[i];
                var index = i;
                
                if (i < _instantiated.Count)
                {
                   var slot = _instantiated[i];
                   slot.Set(scoreInfo, index);
                   slot.gameObject.SetActive(true);
                }
                else
                {
                    InstantiateSlot(scoreInfo, index);
                }
            }

            for (; i < _instantiated.Count; i++)
            {
                _instantiated[i].gameObject.SetActive(false);
            }
        });
    }

    private void InstantiateSlot(ScoreInfo scoreInfo, int index)
    {
        var handle = _highScoreSlotTemplate.InstantiateAsync(transform);
        handle.Completed += h =>
        {
            SetSlot(h.Result, scoreInfo, index);
        };
        _slotHandles.Add(handle);
    }

    private void SetSlot(GameObject go, ScoreInfo scoreInfo, int index)
    {
        if (go.GetComponent<SlotHighscore>() is { } slotHighScore)
        {
            _instantiated.Add(slotHighScore);
            slotHighScore.Set(scoreInfo, index);
            go.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _slotHandles.Count; i++)
        {
            Addressables.Release(_slotHandles[i]);
        }
    }

    [UsedImplicitly]
    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
