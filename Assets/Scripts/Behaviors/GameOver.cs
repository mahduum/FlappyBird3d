using UnityEngine;

namespace Behaviors
{
    public class GameOver : StateBase
    {
        public GameOver(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            Time.timeScale = 0;
            GameplayManager.GameOver.gameObject.SetActive(true);
            GameplayManager.CurrentSegment.Value = 0;
        }

        public override void ResumeGame()
        {
            GameplayManager.GameTimeSeconds.Value = 0;
            GameplayManager.ResetGameTimer();
            GameplayManager.GameOver.gameObject.SetActive(false);
            GameplayManager.SetState(new Reset(GameplayManager));
            Time.timeScale = 1;
        }

        public override async void Exit()
        {
            Time.timeScale = 1;
            await LoadingManager.LoadMainMenuAsSingleScene();
        }
        
        protected override void DisposeInternal()
        {
        }
    }
}