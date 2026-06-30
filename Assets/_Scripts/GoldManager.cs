using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    // 싱글톤으로 어디서든 접근 가능하게 설정
    public static GoldManager Instance { get; private set; }

    // 골드가 변경될 때 UI에 신호를 보낼 이벤트
    public static event Action<int> OnGoldChanged;

    [SerializeField] private int startGold = 500; // 초기 지급 골드
    private int currentGold;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 게임 시작 시 초기 골드 세팅 및 UI 갱신
        currentGold = startGold;
        OnGoldChanged?.Invoke(currentGold);
    }

    // 몬스터가 죽었을 때 호출될 함수
    public void AddGold(int amount)
    {
        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold); // UI 업데이트 신호 발사!
    }

    // 타워 건설 등으로 골드를 쓸 때 호출될 함수
    public bool ConsumeGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
        return false; // 돈 부족
    }
}