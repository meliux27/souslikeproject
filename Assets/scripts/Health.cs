using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Color hitColor = Color.white;

    public bool IsInvulnerable { get; set; }

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (CompareTag("Player") && Input.GetKeyDown(KeyCode.H))
        {
            TakeDamage(10f);
        }
    }
    // =====================================================================

    public void TakeDamage(float amount)
    {
        if (IsInvulnerable)
        {
            Debug.Log(name + " esquivó el golpe");
            return;
        }

        currentHealth -= amount;

        StopAllCoroutines();
        StartCoroutine(Flash());

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}