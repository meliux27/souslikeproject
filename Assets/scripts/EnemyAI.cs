using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detección y movimiento")]
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Ataque")]
    [SerializeField] private float attackTriggerRange = 1.5f;
    [SerializeField] private float attackHitRange = 1.7f;
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float windUp = 0.6f;
    [SerializeField] private float recovery = 0.8f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.6f, 0f); // naranja

    [Header("Retroceso (Knockback)")]
    [SerializeField] private float knockbackForce = 6f;       // NUEVO: cuánto empuja al jugador
    [SerializeField] private float parryKnockbackForce = 6f;  // NUEVO: cuánto retrocede si le haces parry

    [Header("Aturdimiento (tras un parry)")]
    [SerializeField] private float staggerDuration = 1.2f;
    [SerializeField] private Color staggerColor = Color.yellow;

    // Otros scripts (como PlayerCombat) pueden consultar si está aturdido
    public bool IsStaggered { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Knockback selfKnockback; // NUEVO
    private Transform player;
    private Color originalColor;
    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        selfKnockback = GetComponent<Knockback>(); // NUEVO
        originalColor = spriteRenderer.color;

        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("EnemyAI no encontró ningún objeto con el Tag 'Player'");
        }
    }

    private void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackTriggerRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
        }
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // NUEVO: si lo están empujando, dejamos que la física actúe
        if (selfKnockback != null && selfKnockback.IsKnockedBack) return;

        // Mientras ataca (o está aturdido, que ocurre dentro del ataque) se queda quieto
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        if (distance > detectionRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        FaceTarget(dx);

        if (distance > stopDistance)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(dx) * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;

        // 1. Aviso: se pone naranja
        spriteRenderer.color = telegraphColor;
        yield return new WaitForSeconds(windUp);
        spriteRenderer.color = originalColor;

        float waitAfter = recovery;

        // 2. Golpe: solo si sigues dentro del alcance
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackHitRange)
        {
            PlayerParry parry = player.GetComponent<PlayerParry>();

            if (parry != null && parry.TryParry())
            {
                // Parry exitoso: sin daño, el enemigo queda aturdido y retrocede
                IsStaggered = true;
                spriteRenderer.color = staggerColor;
                waitAfter = staggerDuration;

                // NUEVO
                if (selfKnockback != null)
                {
                    selfKnockback.Apply(player.position, parryKnockbackForce);
                }
            }
            else
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    // NUEVO: solo empuja si el golpe realmente te alcanzó (no si lo esquivaste)
                    bool damageApplied = playerHealth.TakeDamage(attackDamage);

                    if (damageApplied)
                    {
                        Knockback playerKnockback = player.GetComponent<Knockback>();
                        if (playerKnockback != null)
                        {
                            playerKnockback.Apply(transform.position, knockbackForce);
                        }
                    }
                }
            }
        }

        // 3. Recuperación (más larga si te hicieron parry)
        yield return new WaitForSeconds(waitAfter);

        spriteRenderer.color = originalColor;
        IsStaggered = false;
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private void FaceTarget(float dx)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dx);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackHitRange);
    }
}