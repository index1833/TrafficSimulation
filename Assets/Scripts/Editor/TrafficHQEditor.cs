using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrafficHQEditor : Editor
{
    private TrafficHeadquarter headquarter;

    private Vector3 startPosition;
    private Vector3 lastPoint;
    private TrafficWaypoint lastWaypoint;

    [MenuItem("Component/TrafficTool/Create Traffic Sysytem")]
    private static void CreateTrafficSystem()
    {
        EditorHelper.SetUpdoGroup("Create Traffic System");

        GameObject headquarterObject = EditorHelper.CreateGameObject("Traffic Headquarter");
        EditorHelper.AddComponent<TrafficHeadquarter>(headquarterObject);

        GameObject segmentObject = EditorHelper.CreateGameObject("Segments", headquarterObject.transform);
        GameObject intersectionObejcet = EditorHelper.CreateGameObject("Intersections", headquarterObject.transform);

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());    

        
    }
    private void OnEnable()
    {
        headquarter = target as TrafficHeadquarter;
    }
    // ��������Ʈ �߰�
    private void AddWayPoint(Vector3 position)
    {
        GameObject go = EditorHelper.CreateGameObject("Waypoint-" + headquarter.curSegment.Waypoints.Count,
            headquarter.curSegment.transform);
        //��ġ�� ���� Ŭ���� ������ �մϴ�.
        go.transform.position = position;
        TrafficWaypoint waypoint = EditorHelper.AddComponent<TrafficWaypoint>(go);
        waypoint.Refresh(headquarter.curSegment.Waypoints.Count, headquarter.curSegment);
        Undo.RecordObject(headquarter.curSegment, "");
        //HQ�� ������ ��������Ʈ�� ���� �۾����� ���׸�Ʈ�� �߰��մϴ�.
        headquarter.curSegment.Waypoints.Add(waypoint);
    }
    //���׸�Ʈ �߰�
    private void AddSegment(Vector3 position)
    {
        int segID = headquarter.segments.Count;
        //Segments��� ���� �� ���ӿ�����Ʈ�� ���ϵ�� ���׸�Ʈ ���ӿ�����Ʈ�� �����մϴ�.
        GameObject segGameObject = EditorHelper.CreateGameObject("Segment-" + segID,
            headquarter.transform.GetChild(0).transform);
        //���� ���� Ŭ���� ��ġ�� ���׸�Ʈ�� �̵���Ŵ
        segGameObject.transform.position = position;
        //HQ�� ���� �۾����� ���׸�Ʈ�� ���� ���� ���׸�Ʈ ��ũ��Ʈ�� �������ݴϴ�.
        //���Ŀ� �߰��Ǵ� ��������Ʈ�� ���� �۾����� ���׸�Ʈ�� �߰��ǰ� �˴ϴ�.
        headquarter.curSegment = EditorHelper.AddComponent<TrafficSegment>(segGameObject);
        headquarter.curSegment.ID = segID;
        headquarter.curSegment.Waypoints = new List<TrafficWaypoint>();
        headquarter.curSegment.nextSegments = new List<TrafficSegment>();

        Undo.RecordObject(headquarter, "");
        headquarter.segments.Add(headquarter.curSegment);
    }
    //���ͼ��� �߰�
    private void AddIntersection(Vector3 position)
    {
        int intID = headquarter.intersections.Count;
        //���ο� �����α����� ���� Interections ���ӿ�����Ʈ ���ϵ�� �ٿ��ݴϴ�.
        GameObject intersection = EditorHelper.CreateGameObject
            ("Intersection-" + intID, headquarter.transform.GetChild(1).transform);
        intersection.transform.position = position;

        BoxCollider boxCollider = EditorHelper.AddComponent<BoxCollider>(intersection);
        boxCollider.isTrigger = true;
        TrafficIntersection trafficIntersection = EditorHelper.AddComponent<TrafficIntersection>(intersection);
        trafficIntersection.ID = intID;

        Undo.RecordObject(headquarter, "");
        headquarter.intersections.Add(trafficIntersection);
    }
}
