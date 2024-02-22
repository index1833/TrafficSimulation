using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficWaypoint : MonoBehaviour
{
    /// <summary>
    /// 부모 세그먼트
    /// 스스로 충돌체 제거
    /// 리프레쉬 가능
    /// </summary>


    public TrafficSegment segment;

    public void RemoveCollider()
    {
        if (GetComponent<SphereCollider>())
        {
            Debug.Log("Remove Collider");
            DestroyImmediate(gameObject.GetComponent<SphereCollider>());
        }
    }

    public void Refresh(int newID, TrafficSegment newSegment)
    {
        segment = newSegment;
        //WayPoint-1 , WayPoint-10
        name = "WayPoint-" + newID.ToString();
        tag = "Waypoint";
        gameObject.layer = LayerMask.NameToLayer("Default");
        RemoveCollider();
    }

    public Vector3 GetVisualPos()
    {
        return transform.position + new Vector3(0.0f, 0.5f, 0.0f);
    }

}
