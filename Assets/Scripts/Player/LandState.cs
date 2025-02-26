using System.Collections;
using UnityEngine;

public class LandState : PlayerState
{
    public LandState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Land");
        player.StartCoroutine(LandRoutine());
    }

    private IEnumerator LandRoutine()
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < player.LandingDuration)
        {
            if (player.MovementInput.magnitude > 0.0f) // Transition to walk or run if moving
            {
                if (player.IsRunPressed)
                {
                    stateMachine.ChangeState(new RunState(player, stateMachine));
                }
                else
                {
                    stateMachine.ChangeState(new WalkState(player, stateMachine));
                }
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        stateMachine.ChangeState(new IdleState(player, stateMachine));
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Land");
    }
}
