using UnityEngine;

namespace Behaviors
{
    public abstract class StateMachineBase : MonoBehaviour
    {
        protected StateBase State;

        public void SetState(StateBase state)
        {
            Debug.Log($"New state set: {state}");
            State?.Dispose();
            State = state;
            State.Start();
        }
    }
}