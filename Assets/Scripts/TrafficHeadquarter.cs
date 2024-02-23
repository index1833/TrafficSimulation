using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficHeadquarter : MonoBehaviour
{
    //세그먼트와 세그먼트사이의 검출 간격.
    public float segDetectThresh = 0.1f;
    //웨이포인트의 크기.
    public float waypointSize = 0.5f;
    //충돌 레이어들.
    public string[] collisionLayers;
    /// <summary>
    ///  자율 주행차가 구간에서 구간을 이동할때 다음 구간을 검색하기 위한 최소 거리 조절 스크립트
    ///  웨이포인트 크기 조정할 수 있음, 충돌체 레이어 관리, 모든구간 포함, 모든 교차로 포함
    /// </summary>
    public List<TrafficSegment> segments = new List<TrafficSegment>();
    public TrafficSegment curSegment;

    public const string VehicleTagLayer = "AutonomousVehicle";//무인자동차.
    //교차로들
    public List<TrafficIntersection> intersections = new List<TrafficIntersection>();

    //에디터용, 기즈모 속성들, 본부에서 조정
    public enum ArrowDraw
    {
        FixedCount,
        ByLength,
        Off
    }
    

    public bool hideGizmos = false;
    public ArrowDraw arrowDrawTpye = ArrowDraw.ByLength;
    public int arrowCount = 1;
    public float arrowDistance = 5f;
    public float arrowSizeWaypoint = 1;
    public float arrowSizeIntersection = 0.5f;
    

    public List<TrafficWaypoint> GetAllWaypoints()
    {
        List<TrafficWaypoint> waypoints = new List<TrafficWaypoint>();
        foreach (var segment in segments)
        {
            waypoints.AddRange(segment.Waypoints);
        }

        return waypoints;
    }

    //data loading -> 속성들 정의
    [Serializable] // 순서가 데이터이기때문에 순서 바꾸기 X
    public class EmergencyData
    {
        public int ID = -1;
        public bool IsEmergency = false;
        public EmergencyData(string id, string emergency)
        {
            ID = int.Parse(id);
            IsEmergency = emergency.Contains("1");
        }
    }
    public class TraaficData
    {   //tsv 같은 형식으로 데이터 넣음
        public List<EmergencyData> datas = new List<EmergencyData>();
    }
    
    //data 출력한 UI 라벨
    public TMPro.TextMeshProUGUI stateLabel;
    //구글 스프레드 시트 읽어올 로더
    public SpreadSheetLoader dataLoader;
    //읽어온 데이터 클래스
    private TraaficData trafficData;

    private void Start()
    {
        // 링크를 따로 안걸어도 시작하자마자 바로 연결됌
        dataLoader = GetComponentInChildren<SpreadSheetLoader>();
        stateLabel = GameObject.FindWithTag("TrafficLabel").GetComponent<TMPro.TextMeshProUGUI>();
        // 일정 주기로 데이터 로딩을 시킬껀데, 너무 자주 빈번하게 부르면 URL막힘
        InvokeRepeating("CallLoaderAndCheck", 5f, 5f);

    }
    private void CallLoaderAndCheck()
    {
        string loadedData = dataLoader.StartLoader();
        stateLabel.text = "Traffic Status\n " + loadedData;
        if (string.IsNullOrEmpty(loadedData))
        {
            return;
        }
        //data -> Class 담음
        trafficData = new TraaficData();
        string[] AllRow = loadedData.Split('\n');
        foreach (string oneRow in AllRow)
        {
            string[] datas = oneRow.Split('\t'); // \t탭키 
            EmergencyData data = new EmergencyData(datas[0], datas[1]);
            trafficData.datas.Add(data);
        }
        //data 검사합니다. 응급상황 발생시 세팅하기
        CheckData();
    }
    private void CheckData()
    {
        for (int i = 0; i < trafficData.datas.Count; i++)
        {
            EmergencyData data = trafficData.datas[i];
            if (intersections.Count <= i || intersections[i] == null)
            {
                return;
            }
            if (data.IsEmergency)
            {
                intersections[data.ID].IntersectionType = IntersectionType.EMERGENCY;
            }
            else
            {
                intersections[data.ID].IntersectionType = IntersectionType.TRAFFIC_LIGHT;
            }
        }
    }
}
