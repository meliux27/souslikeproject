using System.Collections;
using UnityEngine;

public class PlayerParry : MonoBehaviour
{
    [Header("Parry")]
    [SerializeField] private float parryWindow = 0.25f;    // tiempo en que el parry bloquea golpes
    [SerializeField] private float failRecovery = 0.35f;   // castigo si no bloqueaste nada
    [SerializeField] private float successRecovery = 0.1f; // pausa breve tras un parry exitoso
    [SerializeField] private float parryStaminaCost = 15f;
    [SerializeField] private Color parryColor = Color.cyan;

    private PlayerMovement movement;
    private PlayerStamina stamina;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // NUEVO
    private Color originalColor;

    private bool windowActive;
    private bool parrySucceeded;
    private bool isParrying;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stamina = GetComponent<PlayerStamina>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // NUEVO
        originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        bool parryPressed = Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(1);

        if (parryPressed && !isParrying && movement.canMove)
        {
            if (stamina.TryUse(parryStaminaCost))
            {
                StartCoroutine(Parry());
            }
        }
    }

    // NUEVO: si el script se desactiva (por ejemplo al morir), apagamos la animación de parry
    private void OnDisable()
    {
        if (animator != null)
        {
            animator.SetBool("IsParrying", false);
        }
    }

    // El enemigo llama a esta función justo antes de golpear.
    // Devuelve true si el golpe fue bloqueado.
    public bool TryParry()
    {
        if (!windowActive) return false;

        parrySucceeded = true;
        Debug.Log("¡PARRY!");
        return true;
    }

    private IEnumerator Parry()
    {
        isParrying = true;
        parrySucceeded = false;
        movement.canMove = false;

        // NUEVO
        if (animator != null)
        {
            animator.SetBool("IsParrying", true);
        }

        windowActive = true;
        spriteRenderer.color = parryColor;
        yield return new WaitForSeconds(parryWindow);
        windowActive = false;
        spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(parrySucceeded ? successRecovery : failRecovery);

        // NUEVO
        if (animator != null)
        {
            animator.SetBool("IsParrying", false);
        }

        movement.canMove = true;
        isParrying = false;
    }
}