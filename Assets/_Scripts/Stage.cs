using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage : MonoBehaviour
{
    public int diffNum = 0;

    public void EasyButton()
    {
        diffNum = 1;
    }

    public void NormalButton()
    {
        diffNum = 2;
    }

    public void HardButton()
    {
        diffNum = 3;
    }

    public void DiffButton()
    {
        if (diffNum == 1)
        {
            SceneManager.LoadScene("EasyStage");
        }
        else if (diffNum == 2)
        {
            SceneManager.LoadScene("NormalStage");
        }
        else if (diffNum == 3)
        {
            SceneManager.LoadScene("HardStage");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
