using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance;

    [SerializeField] private GameObject hitEffectPrefab;

    void Awake ()
    {
        Instance = this;
    }

    public void PlayHit(Vector3 position)
    {
        if (hitEffectPrefab == null) return;

        GameObject fx = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Destroy(fx, 1f);
    }
}
