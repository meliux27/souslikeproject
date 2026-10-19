using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Color hitColor = Color.white;
    [SerializeField] private float deathDelay = 0f; // NUEVO: segundos que espera tras morir antes de reiniciar o destruirse

    public bool IsInvulnerable { get; set; }

    public event System.Action<float> OnHealthChanged;

    public float HealthNormalized => currentHealth / maxHealth;

    private float currentHealth;
    private bool isDead; // NUEVO
    private SpriteRenderer spriteRenderer;
    private Animator animator; // NUEVO
    private Rigidbody2D rb;    // NUEVO
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // NUEVO
        rb = GetComponent<Rigidbody2D>();    // NUEVO
        originalColor = spriteRenderer.color;
    }

    public bool TakeDamage(float amount)
    {
        if (isDead) return false; // NUEVO

        if (IsInvulnerable)
        {
            Debug.Log(name + " esquivó el golpe");
            return false;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log(name + " vida: " + currentHealth);

        OnHealthChanged?.Invoke(HealthNormalized);

        StopAllCoroutines();
        StartCoroutine(Flash());

        if (currentHealth <= 0f)
        {
            Die();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Hurt"); // NUEVO
        }

        return true;
    }

    private IEnumerator Flash()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void Die()
    {
        isDead = true;

        // NUEVO: paramos y desactivamos el resto de scripts (movimiento, combate, IA...)
        foreach (MonoBehaviour script in GetComponents<MonoBehaviour>())
        {
            if (script == this) continue;
            script.StopAllCoroutines();
            script.enabled = false;
        }

        // NUEVO: frenamos el movimiento horizontal y lanzamos la animación de muerte
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        StartCoroutine(DeathRoutine());
    }

    // NUEVO: espera a que termine la animación y entonces reinicia (jugador) o destruye (enemigo)
    private IEnumerator DeathRoutine()
    {
        if (deathDelay > 0f)
        {
            yield return new WaitForSeconds(deathDelay);
        }

        if (CompareTag("Player"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}