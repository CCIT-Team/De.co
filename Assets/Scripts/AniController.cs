using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anicontroller : MonoBehaviour
{
    // Start is called before the first frame update
    private Animator animator;
    public float Interval = 5f;

    float timer = 0f;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= Interval)
        {
            animator.SetTrigger("NewTrigger");
            timer = 0f;
        }
    }
}
