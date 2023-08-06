using UnityEngine;
using UnityEngine.Events;

namespace Behaviors
{
    [CreateAssetMenu(menuName = "Events/Game States Channel")]
    public class SO_GameStateChannel : ScriptableObject
    {
        public UnityAction<GameState> OnGameStateChanged;

        public void RaiseEvent(GameState gameState)
        {
            OnGameStateChanged.Invoke(gameState);
        }
    }

    public enum GameState
    {
        GameOver,
        Paused,
        Loading,
        MainMenu,
    }
}
