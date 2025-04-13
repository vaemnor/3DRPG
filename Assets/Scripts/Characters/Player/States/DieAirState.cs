using System.Collections;
using UnityEngine;

public class DieAirState : PlayerState
{
    private float verticalVelocity = 0.0f;

    private Vector3 knockbackDirection = Vector3.zero;
    private float knockbackDuration = 0.0f;
    private float knockbackSpeed = 0.0f;

    private float elapsedTime = 0.0f;

    private Coroutine dieRoutine;

    public DieAirState(PlayerController player, StateMachine stateMachine, Vector3 _knockbackDirection, float _knockbackDuration, float _knockbackSpeed) : base(player, stateMachine)
    {
        knockbackDirection = _knockbackDirection;
        knockbackDuration = _knockbackDuration;
        knockbackSpeed = _knockbackSpeed;
    }

    public override void Enter()
    {
        dieRoutine = player.StartCoroutine(DieRoutine());
        player.Animator.SetTrigger("DieAir");
    }

    public override void Update()
    {
        elapsedTime += Time.deltaTime;
        verticalVelocity -= player.Gravity * Time.deltaTime;

        Vector3 movement = Vector3.zero;

        if (elapsedTime < knockbackDuration)
            movement += knockbackDirection * knockbackSpeed;

        movement += Vector3.up * verticalVelocity;

        player.CharacterController.Move(movement * Time.deltaTime);
        player.RotateToward(-knockbackDirection);
    }

    public override void Exit()
    {
        if (dieRoutine != null)
        {
            player.StopCoroutine(dieRoutine);
            dieRoutine = null;
        }

        player.Animator.ResetTrigger("DieAir");
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(player.DeathDuration);
        stateMachine.ChangeState(new ResurrectState(player, stateMachine));
    }
}
