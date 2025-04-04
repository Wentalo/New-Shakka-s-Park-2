using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NO SE USA ?????

public class PlayerDoubleJumpState: PlayerBaseState, IRootState
{
    // Start is called before the first frame update
    public PlayerDoubleJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState() { }
    public override void UpdateState() { }
    public override void ExitState() { }

    public override void InitializeSubState()
    {
        if (!Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Idle()); }
        else if (Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Walk()); }
        else { SetSubState(Factory.Run()); }
    }

    public override void CheckSwitchState() { }

    void HandleDoubleJump()
    {
        //Debug.Log(Ctx.InitialJumpVelocities[Ctx.JumpCount]);
        //Debug.Log(Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f);
        Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f;
        Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f;
    }


    //Cambiar HandleGravity para Double Jump?
    public void HandleGravity()
    {
        //Si el bóton deja de estar pulsado se cae antes tambien
        bool isFalling = Ctx.CurrentMovementY <= 0.0f || !Ctx.JumpPressed;
        float fallMultiplier = 2.0f;

        if (isFalling)
        {
            float previousYVelocity = Ctx.CurrentMovementY;    //Recoge la nueva anterior
            Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * fallMultiplier * Time.deltaTime);  //Calcula la siguiente
            Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * 0.5f, Ctx.TopFallingVelocity);       //Calcula la media
        }
        else
        {
            float previousYVelocity = Ctx.CurrentMovementY;
            Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * Time.deltaTime);
            Ctx.AppliedMovementY = (previousYVelocity + Ctx.CurrentMovementY) * 0.5f; //Estas sumando 2 entonces lo haces la mitad 
        }
    }
}
