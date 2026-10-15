using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Color hitColor = Color.white;
    public bool IsInvulnerable { get; set; }

    // NUEVO: aviso que se lanza cada vez que cambia la vida (valor de 0 a 1)
    public event System.Action<float> OnHealthChanged;

    // NUEVO: vida actual como porcentaje (1 = llena, 0 = muerto)
    public float HealthNormalized => currentHealth / maxHealth;

    private float currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public bool TakeDamage(float amount)
    {
        if (IsInvulnerable)
        {
            Debug.Log(name + " esquivó el golpe");
            return false;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0f); // NUEVO: nunca menos de 0
        Debug.Log(name + " vida: " + currentHealth);

        OnHealthChanged?.Invoke(HealthNormalized); // NUEVO: avisar a las barras

        StopAllCoroutines();
        StartCoroutine(Flash());

        if (currentHealth <= 0f)
        {
            Die();
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