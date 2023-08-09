using UnityEngine;
using UnityEngine.Events;

namespace SOs
{
    [CreateAssetMenu(menuName = "Events/Segment Update Channel")]
    public class SO_SegmentUpdateChannel : ScriptableObject
    {
        public UnityAction OnSegmentExit;
        public void RaiseEvent()
        {
            OnSegmentExit?.Invoke();
        }
    }
}
