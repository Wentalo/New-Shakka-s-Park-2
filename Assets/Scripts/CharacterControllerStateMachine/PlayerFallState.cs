using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
    public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        Debug.Log("Ha entrado en Fall");

        Ctx.CanFallingJump = true; 

        InitializeSubState();
        Ctx.Animator.SetBool(Ctx.IsFallingHash, true);
    }

    public override void UpdateState()
    {
        if (Ctx.JumpPressed)
        {
            //Double Jump //Requiere un nuevo press
            if (Ctx.CanDoubleJump && Ctx.AllowedDJ)
            {
                Ctx.CanDoubleJump = false;
                Ctx.CanFallingJump = false;
            }
        }

        if (!Ctx.IsDashing) HandleGravity(); //Solo se aplica la gravedad si no esta haciendo dash
        CheckSwitchState();
    }

    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsFallingHash, false);
    }

    public override void InitializeSubState()
    {
        if (!Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Idle()); }
        else if (Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Walk()); }
        else { SetSubState(Factory.Run()); }
    }

    public override void CheckSwitchState()
    {
        if (Ctx.CharacterController.isGrounded) { SwitchState(Factory.Grounded()); }
        else if (!Ctx.CanFallingJump) { SwitchState(Factory.Jump()); }
    }

    public void HandleGravity()
    {
        //Ligeramente distinta

        float previousYVelocity = Ctx.CurrentMovementY;                                                                     //Recoge la nueva anterior
        Ctx.CurrentMovementY = Ctx.CurrentMovementY + Ctx.CommonGravity * Time.deltaTime;                                   //Calcula la siguiente
        Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * 0.5f, Ctx.TopFallingVelocity);       //Calcula la media
    }
}
