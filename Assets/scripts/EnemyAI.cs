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

    [Header("Aturdimiento (tras un parry)")]
    [SerializeField] private float staggerDuration = 1.2f;                   // NUEVO
    [SerializeField] private Color staggerColor = Color.yellow;              // NUEVO

    // NUEVO: otros scripts (como PlayerCombat) pueden consultar si está aturdido
    public bool IsStaggered { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Color originalColor;
    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        spriteRenderer.color = telegraphColor;
        yield return new WaitForSeconds(windUp);
        spriteRenderer.color = originalColor;

        float waitAfter = recovery;

        if (player != null && Vector2.Distance(transform.position, player.position) <= attackHitRange)
        {
            PlayerParry parry = player.GetComponent<PlayerParry>();

            if (parry != null && parry.TryParry())
            {
                IsStaggered = true;
                spriteRenderer.color = staggerColor;
                waitAfter = staggerDuration;
            }
            else
            {
                Health playerHealth = player.GetComponent<Health>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }
        }

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