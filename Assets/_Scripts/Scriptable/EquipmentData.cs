using UnityEngine;

// 장비 하나의 데이터. 지금은 UI 표시용 껍데기(이름+아이콘)만 있고,
// 실제 효과(공격력 증가 등)는 나중에 장비 시스템을 만들 때 여기에 추가하면 된다
[CreateAssetMenu(fileName = "EquipmentData", menuName = "Data/Equipment")]
public class EquipmentData : ScriptableObject
{
    public string equipmentName;
    public Sprite icon;
}