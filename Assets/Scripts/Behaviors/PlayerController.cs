using JetBrains.Annotations;
using SOs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Behaviors
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private SO_StateChannel _stateChannel;
        [SerializeField] private Transform _player;
        [SerializeField] private Rigidbody _playerRigidBody;
        [SerializeField] private float _maxLateralSpeed = 20.0f;
        [SerializeField] private float _upwardForce = 100.0f;
        [SerializeField] private PlayerInput _input;

        // Moves the root forward but the player inside the root moves independently, but cannot move forward (or can move 
        // within the limits of the root
        private float _forwardSpeed = 1.0f;//from SO
        private float _lateralDirection = 0;

        private void Awake()
        {
            //create another input controller here later???
            _stateChannel.OnSpeedChanged += OnSpeedChanged;
            _stateChannel.OnReset += OnReset;
        }

        private void OnDestroy()
        {
            _stateChannel.OnSpeedChanged -= OnSpeedChanged;
            _stateChannel.OnReset -= OnReset;
        }

        private void Start()
        {
            Reset();
        }

        private void Reset()
        {
            _forwardSpeed = 0;
            _input.enabled = false;
            _playerRigidBody.useGravity = false;
            _playerRigidBody.velocity = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = _player.rotation = Quaternion.identity;
            _player.position = Vector3.zero;
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

        [UsedImplicitly]
        public void OnSpeedChanged(float currentSpeed)
        {
            _forwardSpeed = currentSpeed;
            _playerRigidBody.useGravity = _forwardSpeed > 0;
            _input.enabled = _forwardSpeed > 0;
        }

        private void OnReset()
        {
            Reset();
        }
    }
}
