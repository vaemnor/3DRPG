using System.Collections;
using UnityEngine;

public class DieGroundState : PlayerState
{
    private Vector3 knockbackDirection = Vector3.zero;
    private float knockbackDuration = 0.0f;
    private float knockbackSpeed = 0.0f;

    private float elapsedTime = 0.0f;

    private Coroutine dieRoutine;

    public DieGroundState(PlayerController player, StateMachine stateMachine, Vector3 _knockbackDirection, float _knockbackDuration, float _knockbackSpeed) : base(player, stateMachine)
    {
        knockbackDirection = _knockbackDirection;
        knockbackDuration = _knockbackDuration;
        knockbackSpeed = _knockbackSpeed;
    }

    public override void Enter()
    {
        dieRoutine = player.StartCoroutine(DieRoutine());
        player.Animator.SetTrigger("DieGround");
    }

    public override void Update()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime < knockbackDuration)
            player.CharacterController.Move((knockbackDirection * knockbackSpeed) * Time.deltaTime);

        player.RotateToward(-knockbackDirection);
    }

    public override void Exit()
    {
        if (dieRoutine != null)
        {
            player.StopCoroutine(dieRoutine);
            dieRoutine = null;
        }

        player.Animator.ResetTrigger("DieGround");
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(player.DeathDuration);
        stateMachine.ChangeState(new ResurrectState(player, stateMachine));
    }
}
