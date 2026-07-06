using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeed : MonoBehaviour

{
    private bool isDoubleSpeed = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isDoubleSpeed = !isDoubleSpeed;
            Time.timeScale = isDoubleSpeed ? 5f : 1f;
            Debug.Log(isDoubleSpeed ? "2���" : "1���");
        }
    }
}
