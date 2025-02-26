public class RunState : PlayerState
{
    public RunState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetBool("isRunning", true);
    }

    public override void Update()
    {
        // Stop running if no movement input is detected
        if (player.MovementInput.magnitude == 0.0f)
        {
            stateMachine.ChangeState(new IdleState(player, stateMachine));
            return;
        }

        // Switch to Walk if Run key is released
        if (!player.IsRunPressed)
        {
            stateMachine.ChangeState(new WalkState(player, stateMachine));
            return;
        }

        player.Move(player.RunSpeed);
    }

    public override void Exit()
    {
        player.Animator.SetBool("isRunning", false);
    }
}
