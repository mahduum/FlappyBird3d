using UnityEngine;
using UnityEngine.Events;

namespace SOs
{
    [CreateAssetMenu(menuName = "Events/Segment Update Channel")]
    public class SO_SegmentUpdateChannel : ScriptableObject
    {
        public UnityAction OnSegmentExit;//segments must have indices
        public void RaiseEvent()//todo add arguments like game object instance id (can be directly accessed in dictionary)
        {
            OnSegmentExit.Invoke();
        }
    }
}
