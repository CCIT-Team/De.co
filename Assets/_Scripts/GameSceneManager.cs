using UnityEngine.SceneManagement;

// 모든 씬 이동을 한곳에서 관리하는 정적 클래스.
// 씬 이름이 바뀌면 아래 상수만 고치면 된다 (다른 스크립트에 씬 이름 문자열을 직접 쓰지 말 것)
public static class GameSceneManager
{
    // ===== 씬 이름 상수 =====
    public const string Title = "TitleScene";
    public const string Lobby = "LobbyScene";
    public const string Difficulty = "DifficultyScene";
    public const string Shop = "Shop";
    public const string TowerSelect = "TowerSelect";

    // 난도별 게임 씬
    public const string EasyGame = "RealEasy";
    public const string NormalGame = "NormalStage";
    public const string HardGame = "HardStage";

    // ===== 선택된 난도 =====
    // static이라 씬이 바뀌어도 값이 유지된다 (난도 선택창 -> 타워 선택창 -> 게임 씬까지 전달됨)
    public enum GameDifficulty { Easy = 1, Normal = 2, Hard = 3 }
    public static GameDifficulty selectedDifficulty = GameDifficulty.Easy;

    // ===== 씬 이동 =====
    public static void LoadTitle() => SceneManager.LoadScene(Title);
    public static void LoadLobby() => SceneManager.LoadScene(Lobby);
    public static void LoadDifficulty() => SceneManager.LoadScene(Difficulty);
    public static void LoadShop() => SceneManager.LoadScene(Shop);
    public static void LoadTowerSelect() => SceneManager.LoadScene(TowerSelect);

    // 선택된 난도에 맞는 게임 씬으로 이동
    public static void LoadSelectedGame()
    {
        switch (selectedDifficulty)
        {
            case GameDifficulty.Normal: SceneManager.LoadScene(NormalGame); break;
            case GameDifficulty.Hard: SceneManager.LoadScene(HardGame); break;
            default: SceneManager.LoadScene(EasyGame); break;
        }
    }
}