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
        player.Move(player.WalkSpeed);
    }

    private IEnumerator JumpRoutine()
    {
        while (verticalVelocity > 0.0f) // Jumping upwards
        {
            player.CharacterController.Move(new Vector3(0.0f, verticalVelocity * Time.deltaTime, 0.0f));
            verticalVelocity -= player.Gravity * Time.deltaTime;
            yield return null;
        }

        // Transition to Fall State when velocity turns negative (falling)
        stateMachine.ChangeState(new FallState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Jump");
    }
}
