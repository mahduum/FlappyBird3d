using System;
using Cysharp.Threading.Tasks;

namespace Behaviors
{
    public abstract class StateBase : IDisposable
    {
        protected readonly GameplayManager GameplayManager;
        protected StateBase(GameplayManager gameplayManager)
        {
            GameplayManager = gameplayManager;
        }

        public abstract void Start();

        public virtual void PauseGame()
        {
        }

        public virtual void ResumeGame()
        {
        }

        public virtual async void Exit()
        {
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            DisposeInternal();
        }

        protected abstract void DisposeInternal();
    }
}