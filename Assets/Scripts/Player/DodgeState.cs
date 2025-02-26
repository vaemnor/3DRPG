using System.Collections;
using UnityEngine;

public class DodgeState : PlayerState
{
    private Vector3 dodgeDirection = Vector3.zero;
    private float dodgeSpeed = 0.0f;
    private float elapsedTime = 0.0f;

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetBool("isDodging", true);

        dodgeDirection = player.CalculateMovementDirection();
        dodgeSpeed = player.DodgeDistance / player.DodgeDuration;
        
        player.StartCoroutine(DodgeRoutine());
    }

    public override void Update()
    {
        player.RotateToward(dodgeDirection);
    }

    private IEnumerator DodgeRoutine()
    {
        elapsedTime = 0.0f;
        while (elapsedTime < player.DodgeDuration)
        {
            player.CharacterController.Move((dodgeDirection * dodgeSpeed) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (player.IsGrounded())
            stateMachine.ChangeState(new IdleState(player, stateMachine));
        else
            stateMachine.ChangeState(new FallState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.SetBool("isDodging", false);
    }
}
