using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TMP_Text buyButtonText;

    private ShopItemData data;

    public void Setup(ShopItemData itemData)
    {
        data = itemData;
        icon.sprite = data.icon;
        nameText.text = data.displayName;
        priceText.text = data.price.ToString();

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);

        RefreshState();
    }

    private void OnBuyClicked()
    {
        if (PlayerInventory.Instance.Owns(data.itemId)) return;

        ShopPopup.Instance.ShowConfirm(
            $"{data.displayName}을(를) {data.price} 재화에 구매하시겠습니까?",
            ExecutePurchase);
    }

    private void ExecutePurchase()
    {
        if (CurrencyManager.Instance.TrySpend(data.price))
        {
            PlayerInventory.Instance.AddItem(data.itemId);
            RefreshState();
        }
        else
        {
            ShopPopup.Instance.ShowNotice("Not enough money");
        }
    }

    private void RefreshState()
    {
        bool owned = PlayerInventory.Instance.Owns(data.itemId);
        buyButton.interactable = !owned;
        if (buyButtonText != null)
            buyButtonText.text = owned ? "Held" : "Buy";
    }
}