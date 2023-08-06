using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Behaviors
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private Rigidbody _playerRigidBody;

        [SerializeField] private float _forwardSpeed = 1.0f;//from SO
        [SerializeField] private float _maxLateralSpeed = 20.0f;
        [SerializeField] private float _upwardForce = 100.0f;
        // Moves the root forward but the player inside the root moves independently, but cannot move forward (or can move 
        // within the limits of the root
        
        private float _lateralDirection = 0;

        private void Awake()
        {
            //create another input controller here later
        }

        // Update is called once per frame
        void Update()
        {
            var translationZ = transform.forward * (_forwardSpeed * Time.deltaTime);
            var translationX = transform.right * (_maxLateralSpeed * _lateralDirection * Time.deltaTime);

            var translation = translationX + translationZ;
            transform.Translate(translation);
        }

        [UsedImplicitly]
        public void Move(InputAction.CallbackContext context)
        {
            if (context.action.phase == InputActionPhase.Canceled)
            {
                _lateralDirection = 0;
            }
            
            var vector = context.action.ReadValue<Vector2>();

            if (vector.y > 0)
            {
                _playerRigidBody.AddForce(Vector3.up * _upwardForce);
            }

            _lateralDirection = vector.x;
        }
    }
}
