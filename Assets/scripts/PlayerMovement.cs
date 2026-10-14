using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;           // NUEVO: velocidad inicial hacia arriba
    [SerializeField] private Transform groundCheck;           // NUEVO: punto de los pies
    [SerializeField] private float groundCheckRadius = 0.2f;  // NUEVO: tamaño del círculo de detección
    [SerializeField] private LayerMask groundLayer;           // NUEVO: qué capa cuenta como suelo

    public bool canMove = true;

    public bool isDodging = false;

    public bool FacingRight => facingRight;
    public bool IsGrounded => isGrounded; // NUEVO: otros scripts pueden consultarlo

    private Rigidbody2D rb;
    private float moveInput;
    private bool facingRight = true;
    private bool isGrounded;       // NUEVO
    private bool jumpRequested;    // NUEVO

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (!canMove)
        {
            moveInput = 0f;
            jumpRequested = false;
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        FaceDirection(moveInput);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDodging) return;

        float yVelocity = rb.linearVelocity.y;

        if (jumpRequested)
        {
            yVelocity = jumpForce;
            jumpRequested = false;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, yVelocity);
    }

    public void FaceDirection(float direction)
    {
        if (direction > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction < 0 && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}