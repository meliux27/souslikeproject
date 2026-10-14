using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private Color hitColor = Color.white;

    // Cuando es true, TakeDamage no hace nada. El esquive lo enciende y apaga.
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

    // NUEVO: devuelve true si el daño se aplicó, false si fue esquivado
    public bool TakeDamage(float amount)
    {
        if (IsInvulnerable)
        {
            Debug.Log(name + " esquivó el golpe");
            return false;
        }

        currentHealth -= amount;
        Debug.Log(name + " vida: " + currentHealth);

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
            // Si muere el jugador, reiniciamos la escena
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}