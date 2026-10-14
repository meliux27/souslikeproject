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
    private Color originalColor;

    private bool windowActive;
    private bool parrySucceeded;
    private bool isParrying;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        stamina = GetComponent<PlayerStamina>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        windowActive = true;
        spriteRenderer.color = parryColor;
        yield return new WaitForSeconds(parryWindow);
        windowActive = false;
        spriteRenderer.color = originalColor;

        yield return new WaitForSeconds(parrySucceeded ? successRecovery : failRecovery);

        movement.canMove = true;
        isParrying = false;
    }
}