using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool doCrouching = true;
    public bool doSprinting = true;

    public float walkSpeed = 5f;
    public float sprintMultiplier = 1.8f;
    public float crouchMultiplier = 0.5f;

    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchScale = 0.5f;
    private Vector3 originalScale;
    private bool isCrouching = false;

    public KeyCode sprintKey = KeyCode.LeftShift;
    private bool isSprinting = false;

    public float gravity = -9.81f;
    private float verticalVelocity = 0f;
    public float groundCheckDistance = 0.4f;
    public LayerMask groundMask;
    private bool isGrounded;
    public bool canMove = true;

    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (canMove)
        {
            HandleGroundCheck();
            HandleCrouch();
            HandleSprint();
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 horizontalMove = transform.right * moveX + transform.forward * moveZ;

        float currentSpeed = walkSpeed;

        if (isSprinting && !isCrouching)
        {
            currentSpeed *= sprintMultiplier;
        }
        else if (!isSprinting && isCrouching)
        {
            currentSpeed *= crouchMultiplier;
        }

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = horizontalMove * currentSpeed + Vector3.up * verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    void HandleCrouch()
    {
        if (doCrouching)
        {
            if (Input.GetKeyDown(crouchKey))
            {
                isCrouching = true;
                isSprinting = false;
                transform.localScale = new Vector3(originalScale.x, originalScale.y * crouchScale, originalScale.z);
            }
            else if (Input.GetKeyUp(crouchKey))
            {
                isCrouching = false;
                transform.localScale = originalScale;
            }
        }
    }

    void HandleSprint()
    {
        if (doSprinting)
        {
            if (Input.GetKeyDown(sprintKey) && !isCrouching)
            {
                isSprinting = true;
            }
            else if (Input.GetKeyUp(sprintKey))
            {
                isSprinting = false;
            }
        }
    }

    void HandleGroundCheck()
    {
        Vector3 groundCheckOrigin = transform.position + Vector3.up * 0.1f;
        isGrounded = Physics.Raycast(groundCheckOrigin, Vector3.down, groundCheckDistance, groundMask);
    }
}