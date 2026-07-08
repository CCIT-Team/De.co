using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int currency = 1000; // 시작 재화, Inspector에서 수정 가능
    public int Currency => currency;

    private const string SaveKey = "Currency";

    public event Action<int> OnCurrencyChanged; // UI 갱신용

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        currency = PlayerPrefs.GetInt(SaveKey, currency);  
    }

    public bool TrySpend(int amount)
    {
        if (currency < amount) return false;
        currency -= amount;
        Save();
        OnCurrencyChanged?.Invoke(currency);
        return true;
    }

    public void Add(int amount)
    {
        currency += amount;
        Save();
        OnCurrencyChanged?.Invoke(currency);
    }
    private void Save()                          // ← 메서드 통째로 추가 (클래스 안 아무 위치)
    {
        PlayerPrefs.SetInt(SaveKey, currency);
        PlayerPrefs.Save();
    }

}


