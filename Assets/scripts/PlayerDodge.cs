using System.Collections;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [Header("Esquive")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.35f;        // duración total del rodar
    [SerializeField] private float invulnerableDuration = 0.25f; // parte del rodar en que eres intocable
    [SerializeField] private float dodgeStaminaCost = 25f;
    [SerializeField] private float dodgeCooldown = 0.2f;         // pausa antes de poder rodar de nuevo

    private Rigidbody2D rb;
    private PlayerMovement movement;
    private PlayerStamina stamina;
    private Health health;
    private SpriteRenderer spriteRenderer;

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
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        bool dodgePressed = Input.GetKeyDown(KeyCode.LeftShift)
                         || Input.GetKeyDown(KeyCode.Space)
                         || Input.GetMouseButtonDown(1);

        if (dodgePressed && !isDodging && movement.canMove && Time.time >= nextDodgeTime)
        {
            if (stamina.TryUse(dodgeStaminaCost))
            {
                StartCoroutine(Dodge());
            }
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        movement.canMove = false;   // no caminar ni atacar mientras ruedas
        movement.isDodging = true;  // PlayerMovement deja de controlar la velocidad

        // Dirección: la tecla que mantengas; si no pulsas ninguna, hacia donde miras
        float direction = Input.GetAxisRaw("Horizontal");
        if (direction == 0f)
        {
            direction = movement.FacingRight ? 1f : -1f;
        }
        movement.FaceDirection(direction);

        // Empieza la invulnerabilidad y el efecto visual (semitransparente)
        health.IsInvulnerable = true;
        SetAlpha(0.4f);

        float timer = 0f;
        while (timer < dodgeDuration)
        {
            rb.linearVelocity = new Vector2(direction * dodgeSpeed, rb.linearVelocity.y);

            // Termina la invulnerabilidad antes del final del rodar (ventana de riesgo)
            if (health.IsInvulnerable && timer >= invulnerableDuration)
            {
                health.IsInvulnerable = false;
                SetAlpha(1f);
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate(); // esperar al siguiente paso de física
        }

        // Seguridad por si invulnerableDuration >= dodgeDuration
        health.IsInvulnerable = false;
        SetAlpha(1f);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        movement.isDodging = false;
        movement.canMove = true;

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