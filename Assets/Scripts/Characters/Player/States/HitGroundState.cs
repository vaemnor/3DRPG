using UnityEngine;

public class HitGroundState : PlayerState
{
    private Vector3 knockbackDirection = Vector3.zero;
    private float knockbackDuration = 0.0f;
    private float knockbackSpeed = 0.0f;

    private float elapsedTime = 0.0f;

    public HitGroundState(PlayerController player, StateMachine stateMachine, Vector3 _knockbackDirection, float _knockbackDuration, float _knockbackSpeed) : base(player, stateMachine)
    {
        knockbackDirection = _knockbackDirection;
        knockbackDuration = _knockbackDuration;
        knockbackSpeed = _knockbackSpeed;
    }

    public override void Enter()
    {
        player.Animator.SetTrigger("HitGround");
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
        player.Animator.ResetTrigger("HitGround");
    }
}
