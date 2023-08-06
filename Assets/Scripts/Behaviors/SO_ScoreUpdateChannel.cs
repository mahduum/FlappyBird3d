using UnityEngine;
using UnityEngine.Events;
using Utilities.Scores;

namespace Behaviors
{
    [CreateAssetMenu(menuName = "Events/Score Update Channel")]
    public class SO_ScoreUpdateChannel : ScriptableObject
    {
        public UnityAction<IScore> OnScore;
        public void RaiseEvent(IScore score)//todo add arguments like game object instance id (can be directly accessed in dictionary)
        {
            OnScore.Invoke(score);
        }
    }
}