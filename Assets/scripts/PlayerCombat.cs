using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private SpriteRenderer slashEffect; // opcional: puede quedar vacío
    [SerializeField] private LayerMask enemyLayer;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackStaminaCost = 20f;
    [SerializeField] private float riposteMultiplier = 2f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Tiempos del ataque (segundos)")]
    [SerializeField] private float windUp = 0.2f;     // hasta el momento del golpe en la animación
    [SerializeField] private float activeTime = 0.1f;
    [SerializeField] private float recovery = 0.2f;

    private PlayerStamina stamina;
    private PlayerMovement movement;
    private Animator animator; // NUEVO
    private bool isAttacking;

    private void Awake()
    {
        stamina = GetComponent<PlayerStamina>();
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>(); // NUEVO
    }

    private void Update()
    {
        bool attackPressed = Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);

        if (attackPressed && !isAttacking && movement.canMove)
        {
            if (stamina.TryUse(attackStaminaCost))
            {
                StartCoroutine(Attack());
            }
        }
    }

    private IEnumerator Attack()
    {
        isAttacking = true;
        movement.canMove = false;

        // NUEVO: arranca la animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        yield return new WaitForSeconds(windUp);

        if (slashEffect != null)
        {
            slashEffect.enabled = true;
        }
        DealDamage();

        yield return new WaitForSeconds(activeTime);

        if (slashEffect != null)
        {
            slashEffect.enabled = false;
        }

        yield return new WaitForSeconds(recovery);

        movement.canMove = true;
        isAttacking = false;
    }

    private void DealDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();
            if (health != null)
            {
                float damage = attackDamage;

                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null && enemy.IsStaggered)
                {
                    damage *= riposteMultiplier;
                }

                bool damageApplied = health.TakeDamage(damage);

                if (damageApplied)
                {
                    Knockback knockback = hit.GetComponent<Knockback>();
                    if (knockback != null)
                    {
                        knockback.Apply(transform.position, knockbackForce);
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}