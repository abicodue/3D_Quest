using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPointManager : MonoBehaviour
{
    // 참조
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private GameObject visual;
    [SerializeField] private GameObject ui;

    // 이벤트
    public event Action OnDied;

    // 기타
    [SerializeField] private float maxHp = 100f;
    public float MaxHp => maxHp;

    private float currentHp;
    public float CurrentHp => currentHp;

    public bool IsDead => currentHp <= 0f;
   

    private void Awake()
    {
        if (hpSlider == null)
        {
            Debug.Log("hpSlider is missing");
        }     

        if (hpText == null)
        {
            Debug.Log("hpText is missing");
        }

        currentHp = maxHp;
    }

    private void Start()
    {
        UpdateHpUI();
    }

    private void RaiseOnDied()
    {
        OnDied?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        
        currentHp -= amount;
        currentHp = Mathf.Max(currentHp, 0);

        UpdateHpUI();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHp = Mathf.Min(maxHp, currentHp + amount);
        UpdateHpUI();
    }

    private void Die()
    {
        RaiseOnDied();
        SetAsDead();       
    }

    private void SetAsDead()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // 스크립트 끄기
            PlayerController3D controller = GetComponent<PlayerController3D>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            MeleeHitBoxAttack meleeAttack = GetComponent<MeleeHitBoxAttack>();
            if (meleeAttack != null)
            {
                meleeAttack.enabled = false;
            }

            // 물리 끄기
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider collider in colliders)
            {
                collider.enabled = false;
            }

            // visual, ui 끄기
            if (visual != null)
            {
                visual.SetActive(false);
            }

            if (ui != null)
            {
                ui.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
        Debug.Log($"<color=red>{gameObject.name}</color> has died");
    }

    private void UpdateHpUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHp;
            hpSlider.value = currentHp;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Ceil(currentHp)}";
        }
    }



}
