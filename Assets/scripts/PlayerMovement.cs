using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;

    public bool canMove = true;


    public bool isDodging = false;

    public bool FacingRight => facingRight;

    private Rigidbody2D rb;
    private float moveInput;
    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!canMove)
        {
            moveInput = 0f;
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");
        FaceDirection(moveInput);
    }

    private void FixedUpdate()
    {
        if (isDodging) return;

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
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
}