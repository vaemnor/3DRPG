using System.Collections;
using UnityEngine;

public class SitState : PlayerState
{
    private Coroutine sitCoroutine;

    public SitState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Sit_Start");
        sitCoroutine = player.StartCoroutine(SitRoutine());
    }

    public override void Update()
    {
        if (player.MovementInput.magnitude > 0.0f)
        {
            if (player.IsRunPressed)
                stateMachine.ChangeState(new RunState(player, stateMachine));
            else
                stateMachine.ChangeState(new WalkState(player, stateMachine));
        }
    }

    public override void Exit()
    {
        if (sitCoroutine != null)
        {
            player.StopCoroutine(sitCoroutine);
            sitCoroutine = null;
        }

        player.Animator.ResetTrigger("Sit_Start");
        player.Animator.ResetTrigger("Sit_End");
    }

    private IEnumerator SitRoutine()
    {
        yield return new WaitForSeconds(player.SitDuration);
        player.Animator.SetTrigger("Sit_End");
    }
}
