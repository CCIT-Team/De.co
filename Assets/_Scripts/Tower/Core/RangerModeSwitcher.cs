using System.Collections;
using UnityEngine;

public class RangerModeSwitcher : MonoBehaviour
{
    public enum CombatMode { Melee, Ranged }

    public RangerSquadData data;

    private Tower tower;
    private CombatMode currentMode = CombatMode.Melee;
    private bool switching;

    public CombatMode CurrentMode => currentMode;

    void Awake()
    {
        tower = GetComponent<Tower>();
    }

    void Start()
    {
        ApplyModeStats();
    }

    void Update()
    {
        // TODO UI 연결 시 버튼 클릭으로도 ToggleMode() 호출
        if (tower == null || !tower.isSelected)
            return;

        if (Input.GetKeyDown(KeyCode.C))
            ToggleMode();
    }

    void ToggleMode()
    {
        if (switching || data == null)
            return;

        StartCoroutine(SwitchModeRoutine());
    }

    IEnumerator SwitchModeRoutine()
    {
        switching = true;
        tower.SetAttackLocked(true);

        currentMode = currentMode == CombatMode.Melee ? CombatMode.Ranged : CombatMode.Melee;
        ApplyModeStats();

        Debug.Log($"[{tower.name}] {(currentMode == CombatMode.Melee ? "근접" : "원거리")} 모드로 전환");

        yield return new WaitForSeconds(data.modeSwitchCooldown);

        tower.SetAttackLocked(false);
        switching = false;
    }

    void ApplyModeStats()
    {
        if (tower == null || tower.Data == null || data == null)
            return;

        if (currentMode == CombatMode.Melee)
        {
            tower.SetModeBaseStats(data.meleeDamage, data.meleeAttackSpeed, data.meleeRange);
            tower.SetCanAttackFlyingOverride(false);
        }
        else
        {
            tower.SetModeBaseStats(data.rangedDamage, data.rangedAttackSpeed, data.rangedRange);
            tower.SetCanAttackFlyingOverride(data.rangedCanAttackFlying);
        }
    }
}