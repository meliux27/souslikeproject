using System.Collections;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("Esquive")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.35f;        // duración total del rodar
    [SerializeField] private float invulnerableDuration = 0.25f; // parte del rodar en que eres intocable
    [SerializeField] private float dodgeStaminaCost = 25f;
    [SerializeField] private float dodgeCooldown = 0.2f;

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerStamina stamina;
    private Health health;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // NUEVO

    private Color originalColor;
    private bool isDodging;
    private float nextDodgeTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
        stamina = GetComponent<PlayerStamina>();
        health = GetComponent<Health>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // NUEVO
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        bool dodgePressed = Input.GetKeyDown(KeyCode.LeftShift);

        if (dodgePressed && !isDodging && movement.canMove && Time.time >= nextDodgeTime)
        {
            if (stamina.TryUse(dodgeStaminaCost))
            {
                StartCoroutine(Dodge());
            }
        }
    }

    // NUEVO: si el script se desactiva (por ejemplo al morir), apagamos la animación de esquive
    private void OnDisable()
    {
        if (animator != null)
        {
            animator.SetBool("IsDodging", false);
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        movement.canMove = false;
        movement.isDodging = true;

        // NUEVO
        if (animator != null)
        {
            animator.SetBool("IsDodging", true);
        }

        float direction = Input.GetAxisRaw("Horizontal");
        if (direction == 0f)
        {
            direction = movement.FacingRight ? 1f : -1f;
        }
        movement.FaceDirection(direction);

        health.IsInvulnerable = true;
        SetAlpha(0.4f);

        float timer = 0f;
        while (timer < dodgeDuration)
        {
            rb.linearVelocity = new Vector2(direction * dodgeSpeed, rb.linearVelocity.y);

            if (health.IsInvulnerable && timer >= invulnerableDuration)
            {
                health.IsInvulnerable = false;
                SetAlpha(1f);
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        health.IsInvulnerable = false;
        SetAlpha(1f);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        movement.isDodging = false;
        movement.canMove = true;

        // NUEVO
        if (animator != null)
        {
            animator.SetBool("IsDodging", false);
        }

        nextDodgeTime = Time.time + dodgeCooldown;
        isDodging = false;
    }

    private void SetAlpha(float alpha)
    {
        Color c = originalColor;
        c.a = alpha;
        spriteRenderer.color = c;
    }
}