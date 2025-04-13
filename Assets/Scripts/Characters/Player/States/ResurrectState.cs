public class ResurrectState : PlayerState
{
    public ResurrectState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Resurrect");
    }

    public override void Exit()
    {
        player.InitializeHealth();
        player.Animator.ResetTrigger("Resurrect");
    }
}
