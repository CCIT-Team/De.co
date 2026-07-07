using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopPopup : MonoBehaviour
{
    public static ShopPopup Instance { get; private set; }

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action onConfirm;

    private void Awake()
    {
        Instance = this;
        confirmButton.onClick.AddListener(() =>
        {
            var action = onConfirm;
            Close();
            action?.Invoke();
        });
        cancelButton.onClick.AddListener(Close);
        gameObject.SetActive(false);
    }

    // 확인/취소 팝업 (구매 확인용)
    public void ShowConfirm(string message, Action confirmAction)
    {
        messageText.text = message;
        onConfirm = confirmAction;
        cancelButton.gameObject.SetActive(true);
        gameObject.SetActive(true);
    }

    // 확인만 있는 알림 팝업 (재화 부족용)
    public void ShowNotice(string message)
    {
        messageText.text = message;
        onConfirm = null;
        cancelButton.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void Close()
    {
        gameObject.SetActive(false);
    }
}