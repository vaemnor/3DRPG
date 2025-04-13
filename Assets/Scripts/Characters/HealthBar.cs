using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Camera mainCamera;
    private Slider healthBarSlider;

    private int minHealth = 0;
    private int maxHealth = 0;
    
    public int CurrentHealth { get; private set; } = 0;

    private void Awake()
    {
        mainCamera = Camera.main;
        healthBarSlider = GetComponent<Slider>();

        minHealth = (int)healthBarSlider.minValue;
        maxHealth = (int)healthBarSlider.maxValue;
    }

    private void Start()
    {
        InitializeHealth();
    }

    private void LateUpdate()
    {
        transform.rotation = mainCamera.transform.rotation;
    }

    public void InitializeHealth()
    {
        CurrentHealth = maxHealth;
        healthBarSlider.value = CurrentHealth;
    }

    public void DecreaseHealth(int damage)
    {
        CurrentHealth = Mathf.Max(CurrentHealth - damage, minHealth);
        healthBarSlider.value = CurrentHealth;
    }
}
