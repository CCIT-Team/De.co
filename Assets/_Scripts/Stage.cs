using UnityEngine;

// 난도 선택창: 난도 버튼 3개가 선택값을 기록하고, 선택 버튼이 실제 이동을 실행한다.
// 선택된 난도는 GameSceneManager.selectedDifficulty에 저장되어 다음 씬들까지 유지된다
public class Stage : MonoBehaviour
{
    public void EasyButton()
    {
        GameSceneManager.selectedDifficulty = GameSceneManager.GameDifficulty.Easy;
    }

    public void NormalButton()
    {
        GameSceneManager.selectedDifficulty = GameSceneManager.GameDifficulty.Normal;
    }

    public void HardButton()
    {
        GameSceneManager.selectedDifficulty = GameSceneManager.GameDifficulty.Hard;
    }

    public void DiffButton()
    {
        // 쉬움 난도는 타워 선택창을 거쳐서 게임 씬으로, 나머지는 바로 스테이지로
        if (GameSceneManager.selectedDifficulty == GameSceneManager.GameDifficulty.Easy)
            GameSceneManager.LoadTowerSelect();
        else
            GameSceneManager.LoadSelectedGame();
    }
}