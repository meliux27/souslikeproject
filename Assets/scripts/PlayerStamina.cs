using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [Header("Estamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float regenPerSecond = 25f;
    [SerializeField] private float regenDelay = 1f; 

    [Header("UI")]
    [SerializeField] private Slider staminaBar;

    private float currentStamina;
    private float regenTimer;

    private void Awake()
    {
        currentStamina = maxStamina;
        UpdateBar();
    }

    private void Update()
    {
        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
        }
        else if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(currentStamina + regenPerSecond * Time.deltaTime, maxStamina);
            UpdateBar();
        }
    }

    public bool TryUse(float amount)
    {
        if (currentStamina < amount)
        {
            return false;
        }

        currentStamina -= amount;
        regenTimer = regenDelay;
        UpdateBar();
        return true;
    }

    private void UpdateBar()
    {
        if (staminaBar != null)
        {
            staminaBar.value = currentStamina / maxStamina;
        }
    }
}