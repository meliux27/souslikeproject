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

    public bool canMove = true;

    public bool isDodging = false;

    public bool FacingRight => facingRight;
    public bool IsGrounded => isGrounded;

    private Rigidbody2D rb;
    private Knockback knockback;
    private Animator animator; // NUEVO
    private float moveInput;
    private bool facingRight = true;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knockback = GetComponent<Knockback>();
        animator = GetComponent<Animator>(); // NUEVO
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (canMove)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
            FaceDirection(moveInput);

            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                jumpRequested = true;
            }
        }
        else
        {
            moveInput = 0f;
            jumpRequested = false;
        }

        UpdateAnimator(); // NUEVO
    }

    private void FixedUpdate()
    {
        if (isDodging) return;

        if (knockback != null && knockback.IsKnockedBack) return;

        float yVelocity = rb.linearVelocity.y;

        if (jumpRequested)
        {
            yVelocity = jumpForce;
            jumpRequested = false;
        }

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, yVelocity);
    }

    // NUEVO: le pasa al Animator el estado actual del jugador
    private void UpdateAnimator()
    {
        if (animator == null) return;

        animator.SetBool("IsWalking", moveInput != 0f);
        animator.SetBool("IsGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
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