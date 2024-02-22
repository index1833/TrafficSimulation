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
    // 웨이포인트 추가
    private void AddWayPoint(Vector3 position)
    {
        GameObject go = EditorHelper.CreateGameObject("Waypoint-" + headquarter.curSegment.Waypoints.Count,
            headquarter.curSegment.transform);
        //위치는 내가 클릭한 곳으로 합니다.
        go.transform.position = position;
        TrafficWaypoint waypoint = EditorHelper.AddComponent<TrafficWaypoint>(go);
        waypoint.Refresh(headquarter.curSegment.Waypoints.Count, headquarter.curSegment);
        Undo.RecordObject(headquarter.curSegment, "");
        //HQ에 생성한 웨이포인트를 현재 작업중인 세그먼트에 추가합니다.
        headquarter.curSegment.Waypoints.Add(waypoint);
    }
    //세그먼트 추가
    private void AddSegment(Vector3 position)
    {
        int segID = headquarter.segments.Count;
        //Segments라고 만든 빈 게임오브젝트의 차일드로 세그먼트 게임오브젝트를 생성합니다.
        GameObject segGameObject = EditorHelper.CreateGameObject("Segment-" + segID,
            headquarter.transform.GetChild(0).transform);
        //내가 지금 클릭한 위치에 세그먼트를 이동시킴
        segGameObject.transform.position = position;
        //HQ에 현재 작업중인 세그먼트에 새로 만든 세그먼트 스크립트를 연결해줍니다.
        //이후에 추가되는 웨이포인트는 현재 작업중인 세그먼트에 추가되게 됩니다.
        headquarter.curSegment = EditorHelper.AddComponent<TrafficSegment>(segGameObject);
        headquarter.curSegment.ID = segID;
        headquarter.curSegment.Waypoints = new List<TrafficWaypoint>();
        headquarter.curSegment.nextSegments = new List<TrafficSegment>();

        Undo.RecordObject(headquarter, "");
        headquarter.segments.Add(headquarter.curSegment);
    }
    //인터섹션 추가
    private void AddIntersection(Vector3 position)
    {
        int intID = headquarter.intersections.Count;
        //새로운 교차로구간을 만들어서 Interections 게임오브젝트 차일드로 붙여줍니다.
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
    //씬에서 직접 설치해보도록 함
    //Shift 
    //Control
    //Alt
    private void OnSceneGUI()
    {
        //마우스 클릭 조작이 있었는지를 얻어옴
        Event @event = Event.current;
        if (@event == null)
        {
            return;
        }
        //마우스포지션 위치로 레이를 만들어줍니다.
        Ray ray = HandleUtility.GUIPointToWorldRay(@event.mousePosition);
        RaycastHit hit;
        //마우스 위치로 충돌체 검출이 되었고, 마우스 클릭으로 인해 발생하였다.
        //0 왼쪽 클릭 1 오른쪽 클릭 2는 휠버튼
        if (Physics.Raycast(ray, out hit) &&
            @event.type == EventType.MouseDown &&
            @event.button == 0)
        {
            //마우스 왼쪽 클릭 + Shift -> 웨이포인트 추가.
            if (@event.shift)
            {
                //구간없는 웨이포인트는 존재할 수 없습니다.
                if (headquarter.curSegment == null)
                {
                    Debug.LogWarning("세그먼트 먼저 만들어주세요");
                    return;
                }
                EditorHelper.BeginUndoGruop("Add WayPoint", headquarter);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            //마우스 왼족 클릭 + Control -> 세그먼트 추가
            //첫번째 웨이포인트도 같이 추가됌
            else if (@event.control)
            {
                EditorHelper.BeginUndoGruop("Add Segment", headquarter);
                AddSegment(hit.point);
                AddWayPoint(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
            //마우스 왼쪽 클릭 + Alt -> 인터섹션 추가
            else if (@event.alt)
            {
                EditorHelper.BeginUndoGruop("Add Intersection", headquarter);
                AddIntersection(hit.point);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            }
        }
        //웨이포인트 시스템을 히어라키뷰에서 선택한 게임 객체로 설정
        Selection.activeGameObject = headquarter.gameObject;
        //선택한 웨이포인트를 처리함
        if (lastWaypoint != null)
        {
            //레이가 충돌할 수 있도록 Plane을 사용함
            Plane plane = new Plane(Vector3.up, lastWaypoint.GetVisualPos());
            plane.Raycast(ray, out float dst);
            Vector3 hitpoint = ray.GetPoint(dst);
            //마우스 버튼을 처음 눌렀을때, LastPoint 재설정
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
            //선택한 웨이포인트를 해제
            if (@event.type == EventType.MouseUp && @event.button == 0)
            {
                Vector3 curPos = lastWaypoint.transform.position;
                lastWaypoint.transform.position = startPosition;
                Undo.RegisterFullObjectHierarchyUndo(lastWaypoint, "Move WayPoint");
                lastWaypoint.transform.position = curPos;
            }
            //구 하나 그리기
            Handles.SphereHandleCap(0, lastWaypoint.GetVisualPos(), Quaternion.identity,
                headquarter.waypointSize * 2f, EventType.Repaint);
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            SceneView.RepaintAll();
        }
        // 모든 웨이포인트로 부터 판별식을 통해 충돌되는 웨이포인트를 LastWayPoint로 세팅
        if (lastWaypoint == null)
        {
            lastWaypoint = headquarter.GetAllWaypoints().FirstOrDefault
                (i => EditorHelper.SphereHit(i.GetVisualPos(), headquarter.waypointSize, ray));
        }
        //HQ의 현재 수정중인 세그먼트를 현재 세그먼트를 현재 선택한 세그먼트로 대체함
        if(lastWaypoint != null && @event.type == EventType.MouseDown)
        {
            headquarter.curSegment = lastWaypoint.segment;
        }
        //현재 웨이포인트를 재설정. 마우스를 이동하면 선택이 풀리도록
        else if(lastWaypoint != null && @event.type == EventType.MouseMove)
        {
            lastWaypoint = null;
        }
    }
}
