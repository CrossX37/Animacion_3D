using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Velocidades de Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("Físicas de Salto y Gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.62f;

    // Referencias internas
    private CharacterController _controller;
    private Animator _animator;
    [SerializeField] private Animator animatorBonny;
    private Transform _cameraTransform;

    // Acciones del New Input System
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;

    // Variables de estado
    private Vector3 _verticalVelocity;
    private float _currentTurnVelocity;

    // Hashes de animación para mayor rendimiento
    private static readonly int AnimCaminado = Animator.StringToHash("Caminado");
    private static readonly int AnimCorrer = Animator.StringToHash("Correr");
    private static readonly int AnimSaltar = Animator.StringToHash("Saltar");
    private static readonly int AnimEnSuelo = Animator.StringToHash("EnSuelo");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = animatorBonny;

        if (Camera.main != null)
            _cameraTransform = Camera.main.transform;

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        _jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        _sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
    }

    private void OnEnable()
    {
        _moveAction.Enable();
        _jumpAction.Enable();
        _sprintAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.Disable();
        _jumpAction.Disable();
        _sprintAction.Disable();
    }

    private void Update()
    {
        HandleGravityAndJump();
        HandleMovement();
    }

    private void HandleGravityAndJump()
    {
        bool isGrounded = _controller.isGrounded;

      

        if (isGrounded && _verticalVelocity.y < 0f)
        {
            _verticalVelocity.y = -2f;
        }

        if (_jumpAction.WasPressedThisFrame() && isGrounded)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // Disparamos la animación de salto al instante
            if (_animator != null)
                _animator.SetTrigger(AnimSaltar);
        }

        _verticalVelocity.y += gravity * Time.deltaTime;
    }

    private void HandleMovement()
    {
        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;

        bool isMoving = direction.magnitude >= 0.1f;
        bool isSprinting = _sprintAction.IsPressed() && isMoving;

        

        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 horizontalMovement = Vector3.zero;

        if (isMoving)
        {
            float cameraAngle = _cameraTransform != null ? _cameraTransform.eulerAngles.y : 0f;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraAngle;

            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentTurnVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothedAngle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            horizontalMovement = moveDirection.normalized * targetSpeed;
        }

        Vector3 finalVelocity = horizontalMovement + _verticalVelocity;
        _controller.Move(finalVelocity * Time.deltaTime);

        bool enSueloAhora = _controller.isGrounded;

        if (_animator != null)
        {
            _animator.SetBool(
                AnimEnSuelo,
                enSueloAhora
            );

            _animator.SetBool(
                AnimCaminado,
                enSueloAhora && isMoving && !isSprinting
            );

            _animator.SetBool(
                AnimCorrer,
                enSueloAhora && isSprinting
            );
        }

    }
}