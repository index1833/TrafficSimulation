using Codice.Client.Common.GameUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    //������ ���� ��ġ�غ����� ��
    //Shift 
    //Control
    //Alt
    private void OnSceneGUI()
    {
        //���콺 Ŭ�� ������ �־������� ����
        Event @event = Event.current;
        if (@event == null)
        {
            return;
        }
        //���콺������ ��ġ�� ���̸� ������ݴϴ�.
        Ray ray = HandleUtility.GUIPointToWorldRay(@event.mousePosition);
        RaycastHit hit;
        //���콺 ��ġ�� �浹ü ������ �Ǿ���, ���콺 Ŭ������ ���� �߻��Ͽ���.
        //0 ���� Ŭ�� 1 ������ Ŭ�� 2�� �ٹ�ư
        if (Physics.Raycast(ray, out hit) &&
            @event.type == EventType.MouseDown &&
            @event.button == 0)
        {
            //���콺 ���� Ŭ�� + Shift -> ��������Ʈ �߰�.
            if (@event.shift)
            {
                //�������� ��������Ʈ�� ������ �� �����ϴ�.
                if (headquarter.curSegment == null)
                {
                    Debug.LogWarning("���׸�Ʈ ���� ������ּ���");
                    return;
                }
                EditorHelper.BeginUndoGruop("Add WayPoint", headquarter);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            //���콺 ���� Ŭ�� + Control -> ���׸�Ʈ �߰�
            //ù��° ��������Ʈ�� ���� �߰���
            else if (@event.control)
            {
                EditorHelper.BeginUndoGruop("Add Segment", headquarter);
                AddSegment(hit.point);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            //���콺 ���� Ŭ�� + Alt -> ���ͼ��� �߰�
            else if (@event.alt)
            {
                EditorHelper.BeginUndoGruop("Add Intersection", headquarter);
                AddIntersection(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
        //��������Ʈ �ý����� �����Ű�信�� ������ ���� ��ü�� ����
        Selection.activeGameObject = headquarter.gameObject;
        //������ ��������Ʈ�� ó����
        if (lastWaypoint != null)
        {
            //���̰� �浹�� �� �ֵ��� Plane�� �����
            Plane plane = new Plane(Vector3.up, lastWaypoint.GetVisualPos());
            plane.Raycast(ray, out float dst);
            Vector3 hitpoint = ray.GetPoint(dst);
            //���콺 ��ư�� ó�� ��������, LastPoint �缳��
            if (@event.type == EventType.MouseDown && @event.button == 0)
            {
                lastPoint = hitpoint;
                startPosition = lastWaypoint.transform.position;
            }
            if (@event.type == EventType.MouseDrag && @event.button == 0)
            {
                Vector3 realPos = new Vector3(hitpoint.x - lastPoint.x, 0, hitpoint.z - lastPoint.z);
                lastWaypoint.transform.position += realPos;
                lastPoint = hitpoint;

            }
            //������ ��������Ʈ�� ����
            if (@event.type == EventType.MouseUp && @event.button == 0)
            {
                Vector3 curPos = lastWaypoint.transform.position;
                lastWaypoint.transform.position = startPosition;
                Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move WayPoint");
                lastWaypoint.transform.position = curPos;
            }
            //�� �ϳ� �׸���
            Handles.SphereHandleCap(0, lastWaypoint.GetVisualPos(), Quaternion.identity,
                headquarter.waypointSize * 2f, EventType.Repaint);
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            SceneView.RepaintAll();
        }
        // ��� ��������Ʈ�� ���� �Ǻ����� ���� �浹�Ǵ� ��������Ʈ�� LastWayPoint�� ����
        if (lastWaypoint == null)
        {
            lastWaypoint = headquarter.GetAllWaypoints().FirstOrDefault
                (i => EditorHelper.SphereHit(i.GetVisualPos(), headquarter.waypointSize, ray));
        }
        //HQ�� ���� �������� ���׸�Ʈ�� ���� ���׸�Ʈ�� ���� ������ ���׸�Ʈ�� ��ü��
        if(lastWaypoint != null && @event.type == EventType.MouseDown)
        {
            headquarter.curSegment = lastWaypoint.segment;
        }
        //���� ��������Ʈ�� �缳��. ���콺�� �̵��ϸ� ������ Ǯ������
        else if(lastWaypoint != null && @event.type == EventType.MouseMove)
        {
            lastWaypoint = null;
        }
    }
}
