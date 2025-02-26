using UnityEngine;

public class FallState : PlayerState
{
    private float verticalVelocity = 0.0f;

    public FallState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetBool("isFalling", true);
    }

    public override void Update()
    {
        player.Move(player.WalkSpeed);

        verticalVelocity -= player.Gravity * Time.deltaTime;
        player.CharacterController.Move(new Vector3(0.0f, verticalVelocity * Time.deltaTime, 0.0f));

        if (player.IsGrounded())
            stateMachine.ChangeState(new LandState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.SetBool("isFalling", false);
    }
}
