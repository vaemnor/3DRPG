using UnityEngine;
using System.Collections;

public class JumpState : PlayerState
{
    private float verticalVelocity;

    public JumpState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Jump");
        verticalVelocity = Mathf.Sqrt(2.0f * player.Gravity * player.JumpHeight);
        player.StartCoroutine(JumpRoutine());
    }

    public override void Update()
    {
        Vector3 movementDirection = player.CalculateMovementDirection();
        movementDirection.y = verticalVelocity;

        player.Move(movementDirection, player.AirHorizontalMovementSpeed);
    }

    private IEnumerator JumpRoutine()
    {
        while (verticalVelocity > 0.0f)
        {
            verticalVelocity -= player.Gravity * Time.deltaTime;
            yield return null;
        }

        stateMachine.ChangeState(new FallState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Jump");
    }
}
