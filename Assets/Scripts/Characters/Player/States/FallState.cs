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
        verticalVelocity -= player.Gravity * Time.deltaTime;

        Vector3 movementDirection = player.CalculateMovementDirection();
        movementDirection.y = verticalVelocity;

        player.Move(movementDirection, player.AirHorizontalMovementSpeed);

        if (player.IsGrounded())
            stateMachine.ChangeState(new LandState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.SetBool("isFalling", false);
    }
}
