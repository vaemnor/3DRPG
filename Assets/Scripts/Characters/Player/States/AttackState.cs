public class AttackState : PlayerState
{
    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        player.Animator.SetTrigger("Attack");
    }

    public override void Exit()
    {
        player.Animator.ResetTrigger("Attack");
    }
}
