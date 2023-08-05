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

        // Update is called once per frame
        void LateUpdate()
        {
            transform.position = _toFollow.position - _offset;
        }
    }
}
