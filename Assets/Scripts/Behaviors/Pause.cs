using UnityEngine;

namespace Behaviors
{
    public class Pause : StateBase
    {
        private float _timeScale;

        public Pause(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            _timeScale = Time.timeScale;
            Time.timeScale = 0;
            GameplayManager.GamePaused.SetActive(true);
        }

        public override void ResumeGame()
        {
            GameplayManager.GamePaused.SetActive(false);
            GameplayManager.SetState(new Flight(GameplayManager));
            Time.timeScale = _timeScale;
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