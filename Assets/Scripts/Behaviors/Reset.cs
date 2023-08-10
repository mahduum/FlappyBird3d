namespace Behaviors
{
    public class Reset : StateBase
    {
        public Reset(GameplayManager gameplayManager) : base(gameplayManager)
        {
        }

        public override void Start()
        {
            GameplayManager.StateChannel.RaiseOnReset();
            GameplayManager.SetState(new Countdown(GameplayManager));
        }

        protected override void DisposeInternal()
        {
        }
    }
}