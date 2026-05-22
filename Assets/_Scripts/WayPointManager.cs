using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    public Transform[] waypoints; // Inspector에서 체크포인트들을 순서대로 연결

    void Awake()
    {
        Instance = this;
    }

    public Transform GetWaypoint(int index)
    {
        return waypoints[index];
    }

    public int GetWaypointCount()
    {
        return waypoints.Length;
    }
}