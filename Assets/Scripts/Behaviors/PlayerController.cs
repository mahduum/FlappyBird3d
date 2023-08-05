using UnityEngine;

namespace Behaviors
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform _player;

        [SerializeField] private float _forwardSpeed = 1.0f;
        // Moves the root forward but the player inside the root moves independently, but cannot move forward (or can move 
        // within the limits of the root
    
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            var translation = transform.forward * (_forwardSpeed * Time.deltaTime);
            transform.Translate(translation);
        }
    }
}
