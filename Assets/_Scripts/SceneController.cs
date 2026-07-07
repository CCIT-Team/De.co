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

    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
