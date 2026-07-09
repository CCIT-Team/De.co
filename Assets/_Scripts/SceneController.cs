using UnityEngine;

// 씬에 배치되어 버튼 onClick을 받아주는 중계 스크립트.
// 실제 씬 이동은 전부 GameSceneManager가 담당한다 (씬 이름도 거기서만 관리)
// 주의: 메서드 이름을 바꾸면 각 씬의 버튼 onClick 연결이 끊어진다!
public class SceneController : MonoBehaviour
{
    // 타이틀의 시작 버튼
    public void StartButton()
    {
        GameSceneManager.LoadLobby();
    }

    // 로비의 출격 준비 버튼 (난도 선택창으로)
    public void DifficultyButton()
    {
        GameSceneManager.LoadDifficulty();
    }

    // 로비의 발주실 버튼
    public void ShopButton()
    {
        GameSceneManager.LoadShop();
    }
}