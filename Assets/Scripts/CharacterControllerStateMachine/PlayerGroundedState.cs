using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState
{
    IEnumerator CoyoteTimeRoutine()
    {
        Debug.Log("Hola");
        yield return new WaitForSeconds(Ctx.CoyoteTime); //El tiempo de espera es de CoyoteTime
        Ctx.InsideCoyoteTime = false;
    }

    public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public void HandleGravity()
    {
        Ctx.CurrentMovementY = Ctx.CommonGravity;
        Ctx.AppliedMovementY = Ctx.CommonGravity;
    }

    public override void EnterState()
    {
        Debug.Log("Ha entrado en Grounded");

        Ctx.CanDoubleJump = true;  //Una vez toca el suelo puede hacer salto doble
        Ctx.InsideCoyoteTime = true; //El coyote time esta disponible nada mas entrar al suelo (renta?)
        Ctx.RequireNewCoyote = false; //Solo se puede hacer un coyote time por entrada al suelo

        InitializeSubState();
        HandleGravity();
    }

    public override void UpdateState()
    {
        //No debe activarse la corutina del coyote time mientras este haciendo dash
        //debido a la mala deteccion del suelo

        if (!Ctx.IsDashing && !Ctx.CharacterController.isGrounded && !Ctx.RequireNewCoyote)
        {
            Ctx.RequireNewCoyote = true;
            Ctx.CurrentCoyoteTimeRoutine = Ctx.StartCoroutine(CoyoteTimeRoutine());
            //Se puede refactorizar como en el salto sabiendo si la corutine es nula
        }
        else if (Ctx.CharacterController.isGrounded && Ctx.CurrentCoyoteTimeRoutine != null)
        {
            Ctx.RequireNewCoyote = false; //Necesario?
            Ctx.StopCoroutine(Ctx.CurrentCoyoteTimeRoutine);
        }

        CheckSwitchState();
    }

    public override void ExitState() { /* Investtigar poner aqui InsideCoyoteTime nulo y como*/}

    public override void InitializeSubState()
    {
        if (!Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Idle()); }
        else if (Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Walk()); }
        else { SetSubState(Factory.Run()); }
    }

    public override void CheckSwitchState()
    {
        //cuando el jugador este en el suelo y pulse el boton de saltar cambiar a Jump
        if (Ctx.JumpPressed && !Ctx.RequireNewJumpPress)
        {

            if (!Ctx.AllowedDashDoubleJump) Ctx.CanDoubleJump = false;

            //if (!Ctx.InsideCoyoteTime) Debug.Log("Opcion Salto"); else Debug.Log("Opcion Coyote");
            SwitchState(Factory.Jump());

        }
        else if (!Ctx.CharacterController.isGrounded && !Ctx.JumpPressed && !Ctx.InsideCoyoteTime)
        {
            //Problemas con el dash

            Debug.Log("Opcion Caida");
            SwitchState(Factory.Fall()); //Si el jugador no esta en el suelo y el salto no esta pulsado

        }

        //else if (!Ctx.CharacterController.isGrounded && !Ctx.JumpPressed && !Ctx.InsideCoyoteTime) { Debug.Log("Opcion Caida"); SwitchState(Factory.Fall()); }  //Si el jugador no esta en el suelo y el salto no esta pulsado
    }
}
