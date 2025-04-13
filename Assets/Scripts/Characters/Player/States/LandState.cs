public class LandState : PlayerState
{
    public LandState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Land");
    }

    public override void Update()
    {
        if (player.MovementInput.magnitude > 0.0f)
        {
            if (player.IsRunPressed)
                stateMachine.ChangeState(new RunState(player, stateMachine));
            else
                stateMachine.ChangeState(new WalkState(player, stateMachine));
        }
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Land");
    }
}
