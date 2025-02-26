public class IdleState : PlayerState
{
    public IdleState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetBool("isIdling", true);
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
        player.Animator.SetBool("isIdling", false);
    }
}
