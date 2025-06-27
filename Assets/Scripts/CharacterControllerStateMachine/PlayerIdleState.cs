using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    IEnumerator IdleRotationRoutine()
    {
        //Debug.Log("hola Idle Roation");

        yield return new WaitForSeconds(Ctx.IdleTime); //El tiempo de espera es de 10 segundos, ajustar a algo
        //Ctx.Animator.SetBool(Ctx.IsChangingIdleHash, true);
        Ctx.Animator.SetTrigger(Ctx.IsChangingIdleHash);
        Ctx.IdleCount = Random.Range(0,2);
        Ctx.Animator.SetInteger(Ctx.IdleCountHash, Ctx.IdleCount);

        //Al ser sin exit time no hay problema
        //Ctx.Animator.SetBool(Ctx.IsChangingIdleHash, false);
        Ctx.InsideIdleRotation = false;
    }

    public override void EnterState()
    {
        Ctx.InsideIdleRotation = false;

        Ctx.Animator.SetBool(Ctx.IsWalkingHash, false);
        Ctx.Animator.SetBool(Ctx.IsRunningHash, false);
        Ctx.AppliedMovementX = 0;
        Ctx.AppliedMovementZ = 0;
    }
    public override void UpdateState()
    {
        CheckSwitchState();

        if (!Ctx.InsideIdleRotation)
        {
            Ctx.IdleTime = Random.Range(10.0f, 21.0f); //Esperara entre 10 y 20 segundos
            Ctx.InsideIdleRotation = true;
            Ctx.CurrentIdleRotationRoutine = Ctx.StartCoroutine(IdleRotationRoutine());
        }

    }

    public override void ExitState() 
    {
        //Debug.Log("He salido de Idle State");
        Ctx.StopCoroutine(Ctx.CurrentIdleRotationRoutine); //nos aseguramos de que el trigger no vaya a saltar
        //Ctx.Animator.SetBool(Ctx.IsChangingIdleHash, false);
        Ctx.Animator.ResetTrigger(Ctx.IsChangingIdleHash); 
    }

    public override void InitializeSubState() { }

    public override void CheckSwitchState()
    {
        if (Ctx.MovementPressed && Ctx.RunPressed) { SwitchState(Factory.Run()); }
        else if (Ctx.MovementPressed) { SwitchState(Factory.Walk()); }
    }
}
