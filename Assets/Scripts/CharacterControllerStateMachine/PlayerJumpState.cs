using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState, IRootState
{
    IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(Ctx.TripleJumpResetTime); //El tiempo de espera es de medio segundo
        Ctx.JumpCount = 0;
        //Revisar
        Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount); //Debugging, no afecta a nada  //???
    }

    IEnumerator JumpBufferTimeRoutine()
    {
        yield return new WaitForSeconds(Ctx.JumpBufferTime); //El tiempo de espera es de medio segundo
        Ctx.InsideJumpBufferTime = false;
    }

    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        Debug.Log("Ha entrado en Jump");

        //Para no activar el buffer nada mas entrar
        Ctx.RequireNewBuffer = true;
        Ctx.RequireNewDouble = true;

        Ctx.InsideJumpBufferTime = false;

        InitializeSubState();

        //No puede depender de allowedDJ

        //Hacer un RayCast para que si viene del dash solo pueda hacer un salto y no dos

        if (Ctx.CanDoubleJump) { HandleJump(); }
        else { Ctx.JumpCount = 1; HandleDoubleJump(); Ctx.JumpCount = 0; } //Debe entrar con el salto minimo?
    }

    public override void UpdateState()
    {

        //Un metodo para jump press?

        if (Ctx.JumpPressed)
        {
            //Jump Buffer
            if (!Ctx.RequireNewBuffer && !Ctx.InsideJumpBufferTime)
            {
                Ctx.RequireNewBuffer = true; //Esto es para que no dejes pulsado el boton y salte automaticamente
                Ctx.InsideJumpBufferTime = true;
                Ctx.CurrentJumpBufferTimeRoutine = Ctx.StartCoroutine(JumpBufferTimeRoutine());
            }

            //Double Jump //Requiere un nuevo press
            else if (Ctx.CanDoubleJump && !Ctx.RequireNewDouble && Ctx.AllowedDJ)
            {
                Ctx.CanDoubleJump = false;
                Ctx.RequireNewDouble = true;
                HandleDoubleJump();
            }
        }

        //CheckJumpCount();

        if (!Ctx.IsDashing) HandleGravity();
        CheckSwitchState();
    }
    public override void ExitState()
    {
        Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
        Ctx.Animator.SetBool(Ctx.IsDoubleJumpingHash, false);
        Ctx.IsJumping = false;

        if (Ctx.JumpPressed) { Ctx.RequireNewJumpPress = true; } //si el boton de salto esta pulsado al salir necesitara que se pulse otra vez

        Ctx.CurrentJumpResetRoutine = Ctx.StartCoroutine(JumpResetRoutine()); //Aqui es el momento para ver si salta de nuevo

        //CheckJumpCount();
    }
    public override void InitializeSubState()
    {
        if (!Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Idle()); }
        else if (Ctx.MovementPressed && !Ctx.RunPressed) { SetSubState(Factory.Walk()); }
        else { SetSubState(Factory.Run()); }
    }

    public override void CheckSwitchState()
    {

        if (Ctx.CharacterController.isGrounded && Ctx.InsideJumpBufferTime)
        {
            CheckJumpCount(); //Debe ir aqui para que se actualice solo una vez pueda volver a saltar

            Ctx.CanDoubleJump = true;
            HandleJump();
            //Debug.Log("Opcion buffer"); 
        }
        else if (Ctx.CharacterController.isGrounded && !Ctx.InsideJumpBufferTime)
        {
            CheckJumpCount(); //Debe ir aqui para que se actualice solo una vez pueda volver a saltar

            SwitchState(Factory.Grounded());
            //Debug.Log("Opcion suelo"); 
        }

    }

    void CheckJumpCount()
    {
        if (Ctx.JumpCount == 3)
        {
            Ctx.JumpCount = 0;
            Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
        }
    }

    void HandleDoubleJump()
    {
        Ctx.Animator.SetBool(Ctx.IsDoubleJumpingHash, true);

        //Debug.Log(Ctx.InitialJumpVelocities[Ctx.JumpCount]);
        //Debug.Log(Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f);
        Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f;
        Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount] * 0.875f;

        Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
        Ctx.IsJumping = true;
        Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
    }

    void HandleJump()
    {
        if (Ctx.JumpCount < 3 && Ctx.CurrentJumpResetRoutine != null)
        {
            //Queremos cancelar la corutina si esta activa debido a que no queremos que vuelva a 0 el jump count
            Ctx.StopCoroutine(Ctx.CurrentJumpResetRoutine);
        }


        //Aqui cuando esta saltando
        Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
        Ctx.IsJumping = true;
        Ctx.JumpCount += 1; //Incrementarlo es facil pero tiene que haber un timer (corutina)
        Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);

        Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
        Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];

        Ctx.maxJumpReach = 0;

        if (Ctx.InsideJumpBufferTime)
        {
            Ctx.InsideJumpBufferTime = false;
            //Ctx.StopCoroutine(Ctx.CurrentJumpBufferTimeRoutine);
        }
    }

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
