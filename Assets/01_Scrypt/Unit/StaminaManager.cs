using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StaminaManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private TMP_Text staminaText;    

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 15f;
    public float MaxStamina => maxStamina;    
    
    private float currentStamina;
    public float CurrentStamina => currentStamina;
    
    [SerializeField] private float staminaRecovery = 5f;
    public float StaminaRecovery => staminaRecovery;


    private void Awake()
    {
        if (staminaSlider == null)
        {
            Debug.Log("staminaSlider is missing");
        }

        if (staminaText == null)
        {
            Debug.Log("staminaText is missing");
        }

        currentStamina = maxStamina;
    }

    private void Start()
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = maxStamina;
        }
    }

    private void Update()
    {
        if (staminaSlider != null && staminaText != null)
        {
            staminaSlider.value = currentStamina;
            staminaText.text = $"{Mathf.Ceil(currentStamina)}";
        }        
    }

    public bool UseStamina(float amount)
    {
        if (amount <= 0f)
        {
            return false;
        }

        if (amount > currentStamina)
        {
            return false;
        }        

        currentStamina -= amount;
        return true;
    }

    public void RecoverStamina(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }







}
