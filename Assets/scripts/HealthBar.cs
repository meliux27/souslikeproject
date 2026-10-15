using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health target; 

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (target != null)
        {
            target.OnHealthChanged += UpdateBar;
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.OnHealthChanged -= UpdateBar;
        }
    }

    private void Start()
    {
        if (target != null)
        {
            UpdateBar(target.HealthNormalized);
        }
    }

    private void UpdateBar(float normalized)
    {
        slider.value = normalized;
    }
}