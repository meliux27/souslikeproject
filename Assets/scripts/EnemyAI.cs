using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Detección y movimiento")]
    [SerializeField] private float detectionRange = 7f;   // a esta distancia te empieza a perseguir
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1.2f;   // se detiene a esta distancia de ti

    [Header("Ataque")]
    [SerializeField] private float attackTriggerRange = 1.5f; // empieza a atacar si estás a esta distancia
    [SerializeField] private float attackHitRange = 1.7f;     // el golpe te alcanza si sigues a esta distancia
    [SerializeField] private float attackDamage = 20f;
    [SerializeField] private float windUp = 0.6f;             // aviso antes del golpe (tu ventana para esquivar)
    [SerializeField] private float recovery = 0.8f;           // queda expuesto después de golpear
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.6f, 0f); // naranja

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

    // Update: decide cuándo atacar
    private void Update()
    {
        if (player == null || isAttacking) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackTriggerRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(Attack());
        }
    }

    // FixedUpdate: decide cómo moverse (física)
    private void FixedUpdate()
    {
        if (player == null) return;

        // Mientras ataca se queda quieto
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float dx = player.position.x - transform.position.x;
        float distance = Mathf.Abs(dx);

        // Muy lejos: no hace nada
        if (distance > detectionRange)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        FaceTarget(dx);

        if (distance > stopDistance)
        {
            // Perseguir
            rb.linearVelocity = new Vector2(Mathf.Sign(dx) * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Ya está cerca: se detiene
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

        // 2. Golpe: solo daña si sigues dentro del alcance
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackHitRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        // 3. Recuperación: queda vulnerable
        yield return new WaitForSeconds(recovery);

        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }

    private void FaceTarget(float dx)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dx);
        transform.localScale = scale;
    }

    // Dibuja los rangos en la ventana Scene cuando seleccionas el enemigo
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackHitRange);
    }
}