using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Salto")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Otros scripts (combate, esquive) pueden bloquear el movimiento
    public bool canMove = true;

    // Mientras es true, este script no toca la velocidad (la controla el esquive)
    public bool isDodging = false;

    public bool FacingRight => facingRight;
    public bool IsGrounded => isGrounded;

    private Rigidbody2D rb;
    private Knockback knockback; // NUEVO
    private float moveInput;
    private bool facingRight = true;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knockback = GetComponent<Knockback>(); // NUEVO
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

        // NUEVO: si te están empujando, dejamos que la física actúe
        if (knockback != null && knockback.IsKnockedBack) return;

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