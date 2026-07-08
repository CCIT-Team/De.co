using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // 싱글턴으로 어디서든 쉽게 접근하게 만듬
    public static PlayerHealth Instance { get; private set; }

    // 체력이 변경될 때 UI에 신호를 보낼 이벤트 (currentHealth, maxHealth)
    public static event Action<float, float> OnHealthChanged;

    // 체력이 0 이하가 됐을 때 신호를 보낼 이벤트
    public static event Action OnPlayerDead;

    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 적이 끝까지 도달했을 때 호출할 함수 (적의 atk만큼 체력 감소)
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            OnPlayerDead?.Invoke();
        }
    }
}