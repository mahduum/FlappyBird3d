using UnityEngine;

namespace Behaviors
{
    public class WallSegment : MonoBehaviour
    {
        [SerializeField] private BoxCollider _boxCollider;
        [SerializeField] private Renderer _topWall;
        [SerializeField] private Renderer _bottomWall;
        [SerializeField] private SO_SegmentUpdateChannel _segmentUpdateChannel;
    
        // Start is called before the first frame update

        private void Awake()
        {
            _boxCollider.center = Vector3.zero;
            _boxCollider.size = new Vector3(_topWall.bounds.size.x,
                Vector3.Distance(_topWall.transform.position, _bottomWall.transform.position), _topWall.bounds.size.z);
        }

        private void OnTriggerExit(Collider other)
        {
            _segmentUpdateChannel.RaiseEvent();
        }
    }
}
