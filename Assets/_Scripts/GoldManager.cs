using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    // �̱������� ��𼭵� ���� �����ϰ� ����
    public static GoldManager Instance { get; private set; }

    // ��尡 ����� �� UI�� ��ȣ�� ���� �̺�Ʈ
    public static event Action<int> OnGoldChanged;

    [SerializeField] private int startGold = 500; // �ʱ� ���� ���
    private int currentGold;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // ���� ���� �� �ʱ� ��� ���� �� UI ����
        currentGold = startGold;
        OnGoldChanged?.Invoke(currentGold);
    }

    // ���Ͱ� �׾��� �� ȣ��� �Լ�
    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold); // UI ������Ʈ ��ȣ �߻�!
    }

    // Ÿ�� �Ǽ� ������ ��带 �� �� ȣ��� �Լ�
    // 차감하지 않고 골드가 충분한지만 확인 (설치 미리보기 등에서 사용)
    public bool HasGold(int amount)
    {
        return currentGold >= amount;
    }

    public bool ConsumeGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
        return false; // �� ����
    }
}