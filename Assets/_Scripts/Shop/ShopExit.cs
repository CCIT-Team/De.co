using UnityEngine;

public class ShopExit : MonoBehaviour
{
    // 상점 나가기 버튼: 로비로 복귀
    public void ExitShop()
    {
        GameSceneManager.LoadLobby();
    }
}