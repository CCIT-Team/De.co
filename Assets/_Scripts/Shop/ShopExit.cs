using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopExit : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "tlqkf"; // 실제 로비 씬 이름으로

    public void ExitShop()
    {
        SceneManager.LoadScene(lobbySceneName);
    }
}