using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    Controls controls;

    Rigidbody2D rb;
    Vector2 moveInput;

    private void Awake()
    {
        controls = new Controls();
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

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        moveInput.x = controls.Main.Movement.ReadValue<Vector2>().x;
        //moveInput.y = 0f;
        Vector2 vector2 = new Vector2(moveInput.x * speed, rb.velocity.y);
        rb.velocity = vector2;
    }

    private bool IsGrounded()
    {
        var groundCheck = Physics2D.Raycast(transform.position, Vector2.down, 0.7f);
        return groundCheck.collider != null && groundCheck.collider.CompareTag("Ground");
    }
}
