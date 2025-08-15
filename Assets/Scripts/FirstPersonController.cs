using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Look Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Ground Check Settings")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask;
    public Transform groundCheckOrigin;

    [Header("Head Bob Settings")]
    public float bobFrequency = 1.5f;
    public float bobAmplitude = 0.05f;
    public float runBobMultiplier = 1.5f;
    private float bobTimer = 0f;
    private Vector3 originalCamPos;

    [Header("Footstep Settings")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    private bool footstepPlayed = false;

    [Header("Jump & Land Sounds")]
    public AudioClip jumpClip;
    public AudioClip landClip;
    public float landSoundCooldown = 0.2f;
    private bool wasGroundedLastFrame = true;
    private float landSoundTimer = 0f;

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction runAction;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];

        originalCamPos = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleHeadBob();
        HandleFootsteps();
        HandleLandingSound();
    }

    void HandleMovement()
    {
        moveInput = moveAction.ReadValue<Vector2>();
        bool isRunning = runAction.IsPressed();
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Ground check using raycast
        isGrounded = Physics.Raycast(
            groundCheckOrigin.position,
            Vector3.down,
            groundCheckDistance,
            groundMask
        );

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (jumpAction.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            footstepSource?.PlayOneShot(jumpClip);
        }
    }

    void HandleLook()
    {
        lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity;

        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * lookInput.x);
    }

    void HandleHeadBob()
    {
        bool isMoving = moveInput.magnitude > 0.1f && isGrounded;
        bool isRunning = runAction.IsPressed();

        if (isMoving)
        {
            float frequency = bobFrequency * (isRunning ? runBobMultiplier : 1f);
            bobTimer += Time.deltaTime * frequency;

            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            Vector3 newCamPos = originalCamPos;
            newCamPos.y += bobOffset;
            cameraTransform.localPosition = newCamPos;
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, originalCamPos, Time.deltaTime * 8f);
        }
    }

    void HandleFootsteps()
    {
        bool isMoving = moveInput.magnitude > 0.1f && isGrounded;

        if (isMoving)
        {
            float frequency = bobFrequency * (runAction.IsPressed() ? runBobMultiplier : 1f);
            float bobValue = Mathf.Sin(bobTimer);

            // Trigger footstep at bottom of bob wave
            if (bobValue < -0.95f && !footstepPlayed)
            {
                PlayFootstep();
                footstepPlayed = true;
            }

            // Reset trigger after wave passes
            if (bobValue > -0.5f)
            {
                footstepPlayed = false;
            }
        }
        else
        {
            footstepPlayed = false;
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length > 0 && footstepSource != null)
        {
            int index = Random.Range(0, footstepClips.Length);
            footstepSource.PlayOneShot(footstepClips[index]);
        }
    }

    void HandleLandingSound()
    {
        landSoundTimer += Time.deltaTime;

        if (!wasGroundedLastFrame && isGrounded && landSoundTimer > landSoundCooldown)
        {
            footstepSource?.PlayOneShot(landClip);
            landSoundTimer = 0f;
        }

        wasGroundedLastFrame = isGrounded;
    }
}