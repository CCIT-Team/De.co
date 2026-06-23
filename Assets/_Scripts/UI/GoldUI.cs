using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용 필수

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    private void OnEnable()
    {
        // 골드 변경 신호 수신 대기
        GoldManager.OnGoldChanged += UpdateGoldText;
    }

    private void OnDisable()
    {
        // 오브젝트 꺼질 때 수신 해제 (메모리 누수 방지)
        GoldManager.OnGoldChanged -= UpdateGoldText;
    }

    private void UpdateGoldText(int newGold)
    {
        // :N0을 넣으면 세 자리마다 쉼표(,)가 자동으로 찍힙니다 (예: 1,500)
        goldText.text = $"{newGold:N0}";
    }
}