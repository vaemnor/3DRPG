public class WalkState : PlayerState
{
    public WalkState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetBool("isWalking", true);
    }

    public override void Update()
    {
        // Stop walking if no movement input is detected
        if (player.MovementInput.magnitude == 0.0f)
        {
            stateMachine.ChangeState(new IdleState(player, stateMachine));
            return;
        }

        // Switch to Run if Run key is pressed
        if (player.IsRunPressed)
        {
            stateMachine.ChangeState(new RunState(player, stateMachine));
            return;
        }

        player.Move(player.WalkSpeed);
    }

    public override void Exit()
    {
        player.Animator.SetBool("isWalking", false);
    }
}
