using UnityEngine;

namespace Behaviors
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _toFollow;
        private Vector3 _offset;
        // Start is called before the first frame update
        void Start()
        {
            SetOffset();
        }

        void SetOffset()
        {
            _offset = _toFollow.position - transform.position;
        }
        
        void LateUpdate()
        {
            var positionFullOffset = _toFollow.position - _offset;
            transform.position = new Vector3(positionFullOffset.x, transform.position.y, positionFullOffset.z);
            transform.LookAt(_toFollow);
        }
    }
}
