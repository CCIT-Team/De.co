using UnityEngine;

// 타워 선택창에서 고른 8개 타워(+각 타워의 장비 2개)를 인게임 씬까지 들고 가는 데이터 보관소.
// static 클래스라서 씬이 바뀌어도 값이 사라지지 않는다 (게임 껐다 켜면 초기화됨).
public static class TowerLoadout
{
    public const int SlotCount = 8;
    public const int EquipPerTower = 2;

    // 장착된 타워 프리팹. 비어있는 칸은 null
    public static GameObject[] towerSlots = new GameObject[SlotCount];

    // 타워 칸마다 장비 2칸씩. equipSlots[타워 칸 번호][장비 칸 번호(0/1)]
    public static EquipmentData[][] equipSlots;

    static TowerLoadout()
    {
        equipSlots = new EquipmentData[SlotCount][];
        for (int i = 0; i < SlotCount; i++)
            equipSlots[i] = new EquipmentData[EquipPerTower];
    }

    // 슬롯이 하나라도 채워져 있는지 (인게임에서 로드아웃을 적용할지 판단용)
    public static bool HasAnyTower
    {
        get
        {
            foreach (GameObject tower in towerSlots)
                if (tower != null) return true;
            return false;
        }
    }

    // 이미 장착된 타워인지 확인 (중복 장착 방지용)
    public static bool Contains(GameObject towerPrefab)
    {
        foreach (GameObject tower in towerSlots)
            if (tower == towerPrefab) return true;
        return false;
    }

    // 칸에 타워를 새로 장착 (기존 타워를 교체하는 경우, 그 타워의 장비는 의미가 없어지므로 함께 비운다)
    public static void SetTower(int index, GameObject towerPrefab)
    {
        towerSlots[index] = towerPrefab;
        for (int j = 0; j < EquipPerTower; j++)
            equipSlots[index][j] = null;
    }

    // 칸 완전히 비우기 (타워 + 장비 모두)
    public static void ClearSlot(int index)
    {
        towerSlots[index] = null;
        for (int j = 0; j < EquipPerTower; j++)
            equipSlots[index][j] = null;
    }

    // 두 칸의 내용을 통째로 교환 (타워가 이동하면 장비도 같이 따라간다)
    public static void SwapSlots(int a, int b)
    {
        GameObject tempTower = towerSlots[a];
        towerSlots[a] = towerSlots[b];
        towerSlots[b] = tempTower;

        EquipmentData[] tempEquips = equipSlots[a];
        equipSlots[a] = equipSlots[b];
        equipSlots[b] = tempEquips;
    }

    // 해당 타워 칸에 같은 장비가 이미 붙어있는지 (한 타워에 같은 장비 중복 방지용)
    public static bool HasEquipment(int towerSlotIndex, EquipmentData equipment)
    {
        foreach (EquipmentData equip in equipSlots[towerSlotIndex])
            if (equip == equipment) return true;
        return false;
    }
}