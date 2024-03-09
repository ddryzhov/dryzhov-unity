using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

// Ensuring the GameObject this script is attached to has a CharacterController and Animator
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // Public fields to be set in the Unity Editor
    public VisualEffect swordVFX;
    public Camera playerCamera;
    public float walkSpeed = 30f;
    public float runSpeed = 50f;
    public float jumpPower = 40f;
    public float gravity = 70f;
    public float lookSpeed = 2f;
    public float lookXLimit = 45f;
    public float defaultHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchSpeed = 3f;

    // Private fields used for internal calculations and states
    private float initialWalkSpeed;
    private float initialRunSpeed;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;
    private int isPunchHash;
    private int isWalkingBackHash;
    public Transform swordTransform;
    private bool canMove = true;
    private GameObject currentTarget;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Convert animation state names to hashes for performance
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isPunchHash = Animator.StringToHash("isPunch");
        isWalkingBackHash = Animator.StringToHash("isWalkingBack");

        // Hide the cursor and lock it to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Remember the initial walk and run speeds
        initialWalkSpeed = walkSpeed;
        initialRunSpeed = runSpeed;

        // Setup the sword visual effect to follow the sword transform
        if (swordTransform != null && swordVFX != null)
        {
            swordVFX.transform.position = swordTransform.position;
            swordVFX.transform.parent = swordTransform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the character is grounded at the beginning of the frame
        bool isGrounded = characterController.isGrounded;

        // Get input from keyboard and mouse
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Destroy the barrel if it's the current target and the left mouse button is pressed
        if (currentTarget != null && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(DestroyAfterDelay(currentTarget));
        }

        // Handle movement and animation based on input
        ProcessMovement(verticalInput, horizontalInput, isRunning, isGrounded);
        ProcessAnimations(verticalInput, horizontalInput, isRunning);

        // Handle jumping and applying gravity
        ProcessJumpAndGravity(isGrounded);

        // Handle sword attacks
        ProcessPunch(isGrounded, isRunning, verticalInput, horizontalInput);

        // Handle camera rotation
        ProcessRotation();
    }

    // Handles character movement based on input and current state
    void ProcessMovement(float verticalInput, float horizontalInput, bool isRunning, bool isGrounded)
    {
        if (!canMove) return;

        // Calculate forward and right vectors relative to the camera's orientation
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        right.Normalize();

        // Determine target direction based on input
        Vector3 targetDirection = forward * verticalInput + right * horizontalInput;

        // Apply walking or running speed to the movement direction
        moveDirection.x = targetDirection.x * (isRunning ? runSpeed : walkSpeed);
        moveDirection.z = targetDirection.z * (isRunning ? runSpeed : walkSpeed);

        // Move the character
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            currentTarget = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Target"))
        {
            currentTarget = null;
        }
    }

    private IEnumerator DestroyAfterDelay(GameObject target)
    {
        // Waiting for the specified delay
        yield return new WaitForSeconds(0.6f);

        // Attempt to get the Destructible component and call the Destroy method if it exists
        Destructible destructible = target.GetComponent<Destructible>();
        if (destructible != null)
        {
            destructible.Destroy();
        }
    }

    // Handles character animations based on movement
    void ProcessAnimations(float verticalInput, float horizontalInput, bool isRunning)
    {
        // Set animation states based on whether there's movement input and if the character is running
        animator.SetBool(isWalkingHash, verticalInput != 0 || horizontalInput != 0);
        animator.SetBool(isRunningHash, isRunning);
        animator.SetBool(isWalkingBackHash, verticalInput < 0);
    }

    // Handles jumping and applying gravity to the character
    void ProcessJumpAndGravity(bool isGrounded)
    {
        // If grounded, then the character can jump
        if (isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpPower;
            }
        }
        else
        {
            // Apply gravity to the character
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Apply vertical movement
        characterController.Move(new Vector3(0, moveDirection.y, 0) * Time.deltaTime);
    }

    // Handles sword attack animations and effects
    void ProcessPunch(bool isGrounded, bool isRunning, float verticalInput, float horizontalInput)
    {
        // Check for sword attack input, making sure the character is grounded and not already attacking
        if (Input.GetMouseButtonDown(0) && canMove && isGrounded && !animator.GetBool(isPunchHash) && !isRunning && (verticalInput == 0 && horizontalInput == 0))
        {
            animator.SetBool(isPunchHash, true);
            StartCoroutine(PlayVFXWithDelay(0.6f)); // Delay the sword VFX
            StartCoroutine(ResetPunch()); // Reset the punch animation
        }
    }

    // Handles rotation of the character based on mouse input
    void ProcessRotation()
    {
        if (!canMove) return;

        // Rotate the character based on the mouse's horizontal movement
        transform.Rotate(0, Input.GetAxis("Mouse X") * lookSpeed, 0);

        // Rotate the camera based on the mouse's vertical movement within the specified limit
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localEulerAngles = new Vector3(rotationX, playerCamera.transform.localEulerAngles.y, 0);
    }

    // Coroutine to reset the punch animation state after a delay
    private IEnumerator ResetPunch()
    {
        yield return new WaitForSeconds(1f);
        animator.SetBool(isPunchHash, false);
    }

    // Coroutine to delay the sword visual effect
    private IEnumerator PlayVFXWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        swordVFX.Play();
    }
}
