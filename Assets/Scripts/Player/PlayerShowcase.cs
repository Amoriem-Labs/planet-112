using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerShowcase : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpSpeed = 5f;

    Controls controls;
    PlayerInput playerInput;
    Animator animator;
    SpriteRenderer spriteRenderer;
    public RuntimeAnimatorController yraMovementAnimator;

    Rigidbody2D rb;
    [SerializeField] float groundRay; // serialized to 0.5f
    [SerializeField] float diagonalRay; // serialized to 0.56f

    private void Awake()
    {
        controls = new Controls();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
        if (rb is null)
        {
            Debug.LogError("RigidBody2D is null!");
        }
    }

    private void OnEnable()
    {
        controls.Main.Enable();
    }

    private void OnDisable()
    {
        controls.Main.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rb.velocity;
        // Get character movement
        // If statement ensures that player cannot move while inventory screen is on
        Vector2 moveInput = controls.Main.Movement.ReadValue<Vector2>();

        // Flip sprite according to movement
        if (moveInput.x != 0) { spriteRenderer.flipX = moveInput.x > 0; }
                
            velocity.x = moveInput.x * speed;

            // First condition only triggers jump if player is pressing Up or W on keyboard, second condition and third condition prevents you from double jumping
        if (moveInput.y > 0 && isGrounded() && velocity.y == 0)
        {
            velocity.y = moveInput.y * jumpSpeed;
        } else if (moveInput.x != 0){
            animator.runtimeAnimatorController = yraMovementAnimator;
        } else {
            animator.runtimeAnimatorController = null;
        }
        rb.velocity = velocity; // needed to ensure the changes we make go back to the rb
    }

    private bool isGrounded(){
        LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacle");
        RaycastHit2D obstacleDownCheck = Physics2D.Raycast(transform.position, Vector2.down, groundRay, obstacleLayerMask);
        RaycastHit2D obstacleDiagonalDownLeftCheck = Physics2D.Raycast(transform.position, new Vector2(-1,-1), diagonalRay, obstacleLayerMask);
        RaycastHit2D obstacleDiagonalDownRightCheck = Physics2D.Raycast(transform.position, new Vector2(1,-1), diagonalRay, obstacleLayerMask);
        LayerMask groundLayerMask = LayerMask.GetMask("Ground");
        RaycastHit2D groundCheck = Physics2D.Raycast(transform.position, Vector2.down, groundRay, groundLayerMask);
        //8 is binary -- to look at just layer 3, we need binary 1000 

        return obstacleDownCheck.collider != null || obstacleDiagonalDownLeftCheck.collider != null || obstacleDiagonalDownRightCheck.collider != null || groundCheck.collider != null;
    }
}
