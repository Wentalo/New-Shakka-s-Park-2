using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    public GhostOdisseyCamera ghost;
    CharacterController characterController;
    Animator animator;
    PlayerInput input;

    [Header("Animation Hash")]
    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;
    int isDoubleJumpingHash;
    int isFallingHash;
    int jumpCountHash;
    int isChangingIdleHash;
    int idleCountHash;
    int isDashingHash;
    int posXHash;

    [Header("Camera")]
    public Camera cameraManager;

    [Header("Movement")]
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 appliedMovement;
    public Vector3 cameraRelativeMovement;

    float horizontalInput;
    float verticalInput;


    [Header("Buttons Pressed")]
    bool movementPressed;
    bool runPressed;
    public bool jumpPressed = false;
    bool dashPressed;

    [Header("Movement Speeds")]
    float walkSpeedMultiplier = 3.0f;
    float runSpeedMultiplier = 9.0f;
    float rotationSpeedMultiplier = 15.0f;

    [Header("Jump Settings")]
    bool isGrounded;
    public bool isJumping = false; //In Air
    //bool isJumpAnimating = false;
    bool requireNewJumpPress = false;
    public float maxJumpReach;

    float groundedGravity = -0.5f;
    float commonGravity = -9.8f; //Esto se cambia en el setUp

    float topFallingVelocity = -20.0f; //velocidad terminal de caida

    float initialJumpVelocity;
    float maxJumpHeight = 4.0f; //meters?
    float maxJumpTime = 1f; //seconds

    [Header("Triple Jump")]
    public int jumpCount = 0;
    float tripleJumpResetTime = 0.2f; //Rebajable a 0.15?
    Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
    Dictionary<int, float> jumpGravities = new Dictionary<int, float>();
    Coroutine currentJumpResetRoutine = null; //Nos sirve para almacenar una corutina

    [Header("Idle Rotation")]
    public bool insideIdleRotation = false;
    public int idleCount = 0;
    float idleTime = 10.0f;
    Coroutine currentIdleRotationRoutine = null;

    [Header("Coyote Time Settings")]
    float coyoteTime = 0.1f; //Subir tiempo? 
    public bool insideCoyoteTime = false;
    public bool requireNewCoyote = false; //Necesario?
    Coroutine currentCoyoteTimeRoutine = null;

    [Header("Jump Buffer Time Settings")]
    float jumpBufferTime = 0.2f; //Rebajable a 0.15?
    public bool insideJumpBufferTime = false;
    public bool requireNewBuffer = false;
    Coroutine currentJumpBufferTimeRoutine = null;

    [Header("Double Jump Settings")]
    public bool canDoubleJump = false;
    public bool requireNewDouble = false;
    public bool canFallingJump;


    [Header("Dash settings")]
    public bool canDash = false;
    public bool isDashing = false;
    public float dashingMultiplier = 5.0f;
    public float dashingTime = 0.2f;
    float dashingCooldown = 1.0f;
    public float actualGravity;

    public bool grounded;

    RaycastHit hit2;
    public bool allowedDashDoubleJump;
    public float offsetDash = 0.45f;

    [SerializeField] private TrailRenderer tr;


    [Header("Falling And RayCast")]

    //public Vector3 boxHalfDimensions = new Vector3(0.23f, 0.03f, 0.21f);
    //float rayCastHeightOffSet = 0.9f; //Mitad de la altura del personaje
    //float rayCastHeightOffSetZ = 0.115f; //Offset que encaja con los pies

    RaycastHit hit;
    public bool allowedDJ;
    public float offsetDJ = 0.9f;
    public LayerMask groundLayer;

    //Manejando instancias de estados (clases concretas)(adv)
    PlayerBaseState currentState;
    PlayerStateFactory states;

    //BlendTree;
    float angle;
    float adjustedAngle;

    #region Getters & Setters
    public PlayerBaseState CurrentState { get { return currentState; } set { currentState = value; } }
    public CharacterController CharacterController { get { return characterController; } }
    public Animator Animator { get { return animator; } }
    public int IsWalkingHash { get { return isWalkingHash; } }
    public int IsRunningHash { get { return isRunningHash; } }
    public int IsJumpingHash { get { return isJumpingHash; } }
    public int IsDoubleJumpingHash { get { return isDoubleJumpingHash; } }
    public int IsFallingHash { get { return isFallingHash; } }
    public int JumpCountHash { get { return jumpCountHash; } }
    public int IsChangingIdleHash { get { return isChangingIdleHash; } }
    public int IdleCountHash { get { return idleCountHash; } }
    public Vector2 CurrentMovementInput { get { return currentMovementInput; } }
    public float CurrentMovementX { get { return currentMovement.x; } set { currentMovement.x = value; } }
    public float CurrentMovementY { get { return currentMovement.y; } set { currentMovement.y = value; } }
    public float CurrentMovementZ { get { return currentMovement.z; } set { currentMovement.z = value; } }
    public float AppliedMovementX { get { return appliedMovement.x; } set { appliedMovement.x = value; } }
    public float AppliedMovementY { get { return appliedMovement.y; } set { appliedMovement.y = value; } }
    public float AppliedMovementZ { get { return appliedMovement.z; } set { appliedMovement.z = value; } }
    public bool MovementPressed { get { return movementPressed; } }
    public bool RunPressed { get { return runPressed; } }
    public bool JumpPressed { get { return jumpPressed; } }
    public float WalkSpeedMultiplier { get { return walkSpeedMultiplier; } }
    public float RunSpeedMultiplier { get { return runSpeedMultiplier; } }
    public bool IsJumping { get { return isJumping; } set { isJumping = value; } }
    public bool RequireNewJumpPress { get { return requireNewJumpPress; } set { requireNewJumpPress = value; } }
    public float GroundedGravity { get { return groundedGravity; } }
    public float CommonGravity { get { return commonGravity; } }
    public float TopFallingVelocity { get { return topFallingVelocity; } }
    public float MaxJumpReach { set { maxJumpReach = value; } get { return maxJumpReach; } }
    public int JumpCount { get { return jumpCount; } set { jumpCount = value; } }
    public float TripleJumpResetTime { get { return tripleJumpResetTime; } }
    public Dictionary<int, float> InitialJumpVelocities { get { return initialJumpVelocities; } }
    public Dictionary<int, float> JumpGravities { get { return jumpGravities; } }
    public Coroutine CurrentJumpResetRoutine { get { return currentJumpResetRoutine; } set { currentJumpResetRoutine = value; } }

    public bool InsideIdleRotation { get { return insideIdleRotation; } set { insideIdleRotation = value; } }
    public int IdleCount { get { return idleCount; } set { idleCount = value; } }
    public float IdleTime { get { return idleTime; } set { idleTime = value; } }
    public Coroutine CurrentIdleRotationRoutine { get { return currentIdleRotationRoutine; } set { currentIdleRotationRoutine = value; } }

    public float CoyoteTime { get { return coyoteTime; } }
    public bool InsideCoyoteTime { get { return insideCoyoteTime; } set { insideCoyoteTime = value; } }
    public bool RequireNewCoyote { get { return requireNewCoyote; } set { requireNewCoyote = value; } }
    public Coroutine CurrentCoyoteTimeRoutine { get { return currentCoyoteTimeRoutine; } set { currentCoyoteTimeRoutine = value; } }
    public float JumpBufferTime { get { return jumpBufferTime; } }
    public bool InsideJumpBufferTime { get { return insideJumpBufferTime; } set { insideJumpBufferTime = value; } }
    public bool RequireNewBuffer { get { return requireNewBuffer; } set { requireNewBuffer = value; } }
    public Coroutine CurrentJumpBufferTimeRoutine { get { return currentJumpBufferTimeRoutine; } set { currentJumpBufferTimeRoutine = value; } }
    public bool CanDoubleJump { get { return canDoubleJump; } set { canDoubleJump = value; } }
    public bool RequireNewDouble { get { return requireNewDouble; } set { requireNewDouble = value; } }
    public bool CanFallingJump { get { return canFallingJump; } set { canFallingJump = value; } }
    public bool CanDash { get { return canDash; } set { canDash = value; } }
    public bool IsDashing { get { return isDashing; } set { isDashing = value; } }
    public bool AllowedDashDoubleJump { get { return allowedDashDoubleJump; } set { allowedDashDoubleJump = value; } }
    public bool AllowedDJ { get { return allowedDJ; } set { allowedDJ = value; } }
    #endregion

    //una vez se llama pues se aplica en el update, problema con la gravedad?

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        Animator.SetTrigger(isDashingHash);
        tr.emitting = true;       
        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        isDashing = false;

        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;

    }

    private void Awake()
    {
        //Time.timeScale = 0.5f;


        //variable de referencias
        input = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        cameraManager = Camera.main; ////

        //Se instancia la factoria de estados y setUp de estados
        states = new PlayerStateFactory(this);
        currentState = states.Grounded();       //a traves de la factoria de estados creo uno y se lo asigno
        currentState.EnterState();              //procedo a entrar

        //hashes
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");
        isDoubleJumpingHash = Animator.StringToHash("isDoubleJumping");
        isFallingHash = Animator.StringToHash("isFalling");
        jumpCountHash = Animator.StringToHash("jumpCount");
        isChangingIdleHash = Animator.StringToHash("isChangingIdle");
        idleCountHash = Animator.StringToHash("idleCount");
        isDashingHash = Animator.StringToHash("isDashing");
        //posXHash = Animator.StringToHash("posX");

        //Seria quitar el canceled?

        //eventos: un evento se esta suscribiendo a un metodo
        input.CharacterControls.Movement.started += OnMovementInput;
        input.CharacterControls.Movement.canceled += OnMovementInput;
        input.CharacterControls.Movement.performed += OnMovementInput;
        input.CharacterControls.Run.started += OnRunInput;
        input.CharacterControls.Run.canceled += OnRunInput;
        input.CharacterControls.Jump.started += OnJumpInput;
        input.CharacterControls.Jump.canceled += OnJumpInput;
        input.CharacterControls.Dash.started += OnDashInput;
        //input.CharacterControls.Dash.canceled += OnDashInput;

        SetupJumpVariables();
    }

    void SetupJumpVariables()
    {
        float timeToApex = maxJumpTime / 2;

        //Esta es la gravedad aplicada al salto, no a la caída
        float initialGravity = (-2 * maxJumpHeight) / (timeToApex * timeToApex); //Refactorizar para ahorra consumo
        //commonGravity = (-2 * maxJumpHeight) / (timeToApex * timeToApex); //No podemos hacer esto pq el personaje caeria muy rapido
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;

        //adds 2 in Height and i 25% longer in time
        float secondJumpGravity = (-2 * (maxJumpHeight * 1.5f)) / ((timeToApex * 1.25f) * (timeToApex * 1.25f));
        float secondJumpInitialVelocity = (2 * (maxJumpHeight * 1.5f)) / (timeToApex * 1.25f);

        //adds 4 in Height and i 50% longer in time
        float thirdJumpGravity = (-2 * (maxJumpHeight * 2.0f)) / ((timeToApex * 1.5f) * (timeToApex * 1.5f));
        float thirdJumpInitialVelocity = (2 * (maxJumpHeight * 2.0f)) / (timeToApex * 1.5f);

        initialJumpVelocities.Add(1, initialJumpVelocity);
        initialJumpVelocities.Add(2, secondJumpInitialVelocity);
        initialJumpVelocities.Add(3, thirdJumpInitialVelocity);

        jumpGravities.Add(0, initialGravity); //Para cuando se resetee los saltos
        jumpGravities.Add(1, initialGravity);
        jumpGravities.Add(2, secondJumpGravity);
        jumpGravities.Add(3, thirdJumpGravity);
    }

    private void Start()
    {
        characterController.Move(appliedMovement * Time.deltaTime); //Que se fije en el primer frame en el suelo
    }

    void OnMovementInput(InputAction.CallbackContext ctx)
    {
        currentMovementInput = ctx.ReadValue<Vector2>();
        horizontalInput = currentMovementInput.x; //
        verticalInput = currentMovementInput.y; //
        movementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    void OnRunInput(InputAction.CallbackContext ctx)
    {
        runPressed = ctx.ReadValueAsButton();
    }

    void OnJumpInput(InputAction.CallbackContext ctx)
    {
        jumpPressed = ctx.ReadValueAsButton();
        requireNewJumpPress = false;
        requireNewBuffer = false;
        if (canDoubleJump) requireNewDouble = false;
    }

    void OnDashInput(InputAction.CallbackContext ctx)
    {
        dashPressed = ctx.ReadValueAsButton();
        if (canDash) StartCoroutine(Dash());
    }

    void HandleRotation()
    {

        //En vez del input se puede usar directamente el cameraRelativeMovement y nos ahorramos el ConverToCameraSpace
        //Funciona exactamente igual      

        Vector3 positionToLookAt;

        /*
        //the change in position our character should point to
        positionToLookAt.x = currentMovementInput.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovementInput.y;

        positionToLookAt = ConvertToCameraSpace(positionToLookAt);
        */

        positionToLookAt.x = cameraRelativeMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = cameraRelativeMovement.z;



        //the current rotation for our character
        Quaternion currentRotation = transform.rotation;

        if (movementPressed)
        {
            //crea una nuvea rotacion basada en donde el jugador esta pulsando (moviendose)
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);

            //angle = Quaternion.Angle(currentRotation, targetRotation);

            //rotate the character to face the position positionToLookAt
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeedMultiplier * Time.deltaTime); //El time.deltaTime permite que no sea tan brusco
        }
    }

    //ARREGLAR RAYCASTS

    void HandleDashJumpCheck()
    {
        //Si esta "practicamente" tocando el suelo puede hacer doble salto ya que no queda raro con el dash

        //Debug.Log("hola");

        if (Physics.Raycast(transform.position, -Vector3.up, out hit2, offsetDash, groundLayer)) { allowedDashDoubleJump = true; }
        else { allowedDashDoubleJump = false; }
    }

    void HandleDoubleJumpCheck()
    {
        //Si esta "casi" tocando el suelo no puede hacer doble salto ya que interfiere con el Jump Buffering

        if (Physics.Raycast(transform.position, -Vector3.up, out hit, offsetDJ, groundLayer)) { allowedDJ = false; }
        else { allowedDJ = true; }
    }

    //EN DESUSO
    void HandleBlendTreeAnimation()
    {
        Vector2 vectorController = new Vector2(characterController.transform.forward.x, characterController.transform.forward.z);
        Vector2 vectorCamera = new Vector2(cameraManager.transform.forward.x, cameraManager.transform.forward.z);


        angle = Vector2.SignedAngle(vectorController, vectorCamera);
        adjustedAngle = angle / 180;

        animator.SetFloat(posXHash, adjustedAngle);
    }

    // Update is called once per frame
    void Update()
    {
        //no queremos que entre si esta haciendo el dash

        actualGravity = JumpGravities[JumpCount];
        grounded = CharacterController.isGrounded;

        if (!isDashing)
        {

            HandleRotation();

            //grounded = CharacterController.isGrounded;

            currentState.UpdateStates(); //Accede al de la clase asbtracta, no la concreta

            cameraRelativeMovement = ConvertToCameraSpace(appliedMovement);

            characterController.Move(cameraRelativeMovement * Time.deltaTime); //Al multiplicarse aquí x deltaTime no se hace arriba en appliedMovement
        }
        else
        {
            cameraRelativeMovement = ConvertToCameraSpace(appliedMovement * dashingMultiplier);

            cameraRelativeMovement.y = 0;

            characterController.Move(cameraRelativeMovement * Time.deltaTime);

            

            
        }

        //Solo habia que poner cual era la capa a usar

        HandleDashJumpCheck();

        HandleDoubleJumpCheck();

        //Aqui es donde se actualiza la camara free look si toca el limite de la camara o no

        if (characterController.transform.position.y > maxJumpReach) { maxJumpReach = characterController.transform.position.y; }


        //En caso de caer al vacio

        if (this.transform.position.y < -20)
        {
            this.transform.position = new Vector3(0.0f, 0.15f, 0.0f);
        }

        //angle = (Vector3.Angle(characterController.transform.forward, cameraManager.transform.forward));
    }

    //Calculamos hacia donde se tiene que mover el personaje Y APUNTAR (mirar) en relacion a la camara

    Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        //Necesitamos guardar nuestra Y si no el personaje flota y no se aplica la gravedad
        float currentYValue = vectorToRotate.y;

        Vector3 cameraForward = cameraManager.transform.forward;
        Vector3 cameraRight = cameraManager.transform.right;

        //Ignoramos el movimiento arriba y abajo de la cámara
        cameraForward.y = 0;
        cameraRight.y = 0;

        //Normalizarlos permite crear el Camera Coordinate Space
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraRightXProduct = vectorToRotate.x * cameraRight;

        Vector3 vectorRotatedToCameraSpace = cameraForwardZProduct + cameraRightXProduct;
        vectorRotatedToCameraSpace.y = currentYValue;   //Aqui reintroducimos el movimiento en Y
        return vectorRotatedToCameraSpace;
    }

    // (EN DESUSO POR UPGRADE) Calculamos hacia donde se tiene que mover el personaje en relacion a la camara 
    void MovePlayerRelativeToCamera()
    {
        //Recogemos los vectores normalizados de la camara, que nos indican como ir hacia delante y como girar

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        //Aplicamos 0 para que no salga volando (no se vea afectado por el tilt de la camara)
        //Normalizamos para que la velocidad del personaje no se vea afectada por la rotacion arriba y abajo de la camara
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized; 
        right = right.normalized;

        //Create direction-relative-input vectors, Hacemos que los inputs se multipliquen por la camara
        Vector3 forwardRelativeVerticalInput = verticalInput * forward;
        Vector3 rightRelativeHorizontalInput = horizontalInput * right;

        //Create and apply camera relative movement, lo aplicamos
        Vector3 cameraRelativeMovement = forwardRelativeVerticalInput + rightRelativeHorizontalInput;
        Camera.main.transform.Translate(cameraRelativeMovement, Space.World);
    }

    private void OnDrawGizmosSelected()
    {
        //Gizmos.color = Color.red;
        //Debug.DrawLine(origin, origin + direction * currentHitDistance);
        //Debug.DrawRay(originCamera, directionCamera * raycastCameraDistance, Color.blue);

        Vector3 end = new Vector3(transform.position.x, transform.position.y - offsetDJ, transform.position.z);
        Debug.DrawLine(transform.position, end, Color.red);

        Vector3 end2 = new Vector3(transform.position.x, transform.position.y - offsetDash, transform.position.z);
        Debug.DrawLine(transform.position, end2, Color.blue);
        //Gizmos.DrawWireSphere(origin + direction * currentHitDistance, cameraCollisionRadius);
        //Gizmos.DrawWireSphere(transform.position , 0.1f);

        //El DrawCube usa el tamaño entero, no la mitad
        //Gizmos.DrawCube(transform.position + transform.forward * rayCastHeightOffSetZ, boxHalfDimensions * 2);
    }
    private void OnEnable() { input.CharacterControls.Enable(); }
    private void OnDisable() { input.CharacterControls.Disable(); }
}
