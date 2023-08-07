using UnityEngine;
using UnityEngine.Events;

namespace SOs
{
    [CreateAssetMenu(menuName = "Events/Game States Channel")]
    public class SO_StateChannel : ScriptableObject
    {
        public UnityAction<float> OnSpeedChanged;
        public UnityAction OnGameOver;
        public UnityAction<int> OnCountdown;

        public void RaiseOnSpeedChanged(float currentSpeed)
        {
            OnSpeedChanged?.Invoke(currentSpeed);
        }

        public void RaiseOnGameOver()
        {
            OnGameOver?.Invoke();
        }

        public void RaiseOnCountdown(int seconds)
        {
            OnCountdown?.Invoke(seconds);
        }
    }
}
