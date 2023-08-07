using UnityEngine;

namespace Behaviors
{
    public abstract class StateMachineBase : MonoBehaviour
    {
        protected StateBase State;//todo make reactive???

        public void SetState(StateBase state)
        {
            Debug.Log($"New state set: {state}");
            State = state;
            State.Start();
        }
    }
}