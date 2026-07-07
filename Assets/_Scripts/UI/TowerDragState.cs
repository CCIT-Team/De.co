using UnityEngine;

// "지금 무엇을 드래그 중인지"를 기억하는 공유 메모장.
// 드래그를 시작한 쪽(인벤토리/장착 칸)이 여기에 기록하고,
// 드래그를 받은 쪽(장착 칸의 OnDrop)이 여기를 읽어서 처리한다
public static class TowerDragState
{
    // ── 타워 드래그 ──
    public static GameObject draggedTower;  // 드래그 중인 타워 프리팹 (null이면 타워 드래그 중 아님)
    public static int fromSlotIndex = -1;   // 장착 칸에서 드래그 시작했으면 그 칸 번호, 인벤토리에서 시작했으면 -1

    // ── 장비 드래그 ──
    public static EquipmentData draggedEquipment; // 드래그 중인 장비 (null이면 장비 드래그 중 아님)
    public static int fromEquipTowerSlot = -1;    // 장비 칸에서 드래그 시작했으면 그 타워 칸 번호, 인벤토리면 -1
    public static int fromEquipIndex = -1;        // 그 타워의 몇 번째 장비 칸(0/1)에서 시작했는지

    // ── 공통 ──
    public static bool dropHandled; // 어떤 칸이 이 드래그를 받아줬는지 여부 (못 받았으면 장착 해제 판단에 사용)

    public static bool IsDragging => draggedTower != null;

    public static void Clear()
    {
        draggedTower = null;
        fromSlotIndex = -1;
        draggedEquipment = null;
        fromEquipTowerSlot = -1;
        fromEquipIndex = -1;
        dropHandled = false;
    }
}