using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ShopItemData> shopItems; // 판매 목록
    [SerializeField] private ShopSlot slotPrefab;
    [SerializeField] private Transform contentParent;      // ScrollView의 Content
    [SerializeField] private TMP_Text currencyText;

    private void Start()
    {
        foreach (var item in shopItems)
        {
            ShopSlot slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(item);
        }

        currencyText.text = CurrencyManager.Instance.Currency.ToString();
        CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyText;
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyText;
    }

    private void UpdateCurrencyText(int value)
    {
        currencyText.text = value.ToString();
    }
}