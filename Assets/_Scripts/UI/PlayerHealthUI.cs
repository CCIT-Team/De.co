using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 필수

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void OnEnable()
    {
        // 체력 변경 신호 구독
        PlayerHealth.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        // 오브젝트 파괴 시 구독 해제 (메모리 누수 방지)
        PlayerHealth.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"{currentHealth:N0}/{maxHealth:N0}";
    }
}