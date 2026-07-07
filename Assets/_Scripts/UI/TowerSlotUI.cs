using UnityEngine;
using UnityEngine.UI;

public class TowerSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private int slotIndex;

    // TowerSummonBarUI가 슬롯을 생성할 때 호출해서 어떤 타워를 보여줄지 세팅
    public void Setup(int index, GameObject towerPrefab)
    {
        slotIndex = index;

        if (towerPrefab == null)
        {
            // 아직 배정된 타워가 없는 빈 슬롯
            iconImage.enabled = false;
            return;
        }

        SpriteRenderer sr = towerPrefab.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = sr.sprite;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    // 슬롯 버튼의 OnClick에 연결
    public void OnClickSlot()
    {
        Debug.Log($"[Slot] {slotIndex}번 슬롯 클릭됨");
        TowerPlacementManager.Instance.SelectSlot(slotIndex);
    }
}