using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private SpriteRenderer slashEffect;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private float attackDamage = 25f;
    [SerializeField] private float attackStaminaCost = 20f;

    [Header("Tiempos del ataque (segundos)")]
    [SerializeField] private float windUp = 0.1f;    
    [SerializeField] private float activeTime = 0.1f; 
    [SerializeField] private float recovery = 0.25f; 

    private PlayerStamina stamina;
    private PlayerMovement movement;
    private bool isAttacking;

    private void Awake()
    {
        stamina = GetComponent<PlayerStamina>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        bool attackPressed = Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);

        if (attackPressed && !isAttacking)
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

        yield return new WaitForSeconds(windUp);

        slashEffect.enabled = true;
        DealDamage();

        yield return new WaitForSeconds(activeTime);
        slashEffect.enabled = false;

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
                health.TakeDamage(attackDamage);
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