using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class SceneController : MonoBehaviour
{
    

    // Start is called before the first frame update

    public void StartButton()
    {
        Debug.Log("[SceneController] StartButton 클릭됨 -> LobbyScene 로드 시도");
        SceneManager.LoadScene("LobbyScene");
    }

    public void DifficultyButton()
    {
        SceneManager.LoadScene("DifficultyScene");
    }

    public void SampleButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    // 로비의 발주실 버튼: 상점 씬으로 이동
    public void ShopButton()
    {
        SceneManager.LoadScene("Shop");
    }

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
