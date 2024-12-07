using UnityEngine;
using UnityEngine.UI; // Image 사용을 위한 네임스페이스
using System.Collections;

public class ReverseDetection : MonoBehaviour
{
    [Header("Rank)")]
    public float rankData;
    public LapCounter curLap;
    public float distanceWaypoint;
    public string carName;

    [Header("NMS")]
    public Transform[] waypoints; // Waypoints 배열
    public int currentWaypointIndex = 0; // 현재 Waypoint 인덱스
    private Transform car; // 차량 Transform
    private bool isReversing = false; // 역주행 상태를 추적하는 변수
    private Coroutine reverseCoroutine; // 역주행 텍스트를 지연시키는 코루틴 참조

    public Image reverseImage; // 역주행 알림 이미지를 위한 Image UI 객체

    [Range(-1f, 0f)]
    public float reverseThreshold = -0.2f; // 역주행으로 간주할 Dot 임계값

    public float waypointThreshold = 15f; // Waypoint 거리 임계값

    private float carSpeed; // 차량의 속도

    private void Start()
    {
        waypoints = GameManager.Instance.waypointsManager.waypoints;
        reverseImage = GameManager.Instance.reverseImage;

        car = transform; // 차량의 Transform 설정
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false); // 초기에는 이미지 비활성화
        }
    }

    private void Update()
    {
        DetectReverseDriving();

        rankData = currentWaypointIndex * 100+curLap.currentLap*100000+distanceWaypoint;
    }

    private void DetectReverseDriving()
    {
        // 현재 Waypoint와 다음 Waypoint를 가져옴
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Transform nextWaypoint = waypoints[(currentWaypointIndex + 1) % waypoints.Length];
        Transform previousWaypoint = waypoints[(currentWaypointIndex-1+waypoints.Length)%waypoints.Length];

        float curDistance=Vector3.Distance(this.gameObject.transform.position,currentWaypoint.transform.position);
        float nextDistance=Vector3.Distance(this.gameObject.transform.position,nextWaypoint.transform.position);
        float nextWaypointDistance=Vector3.Distance(currentWaypoint.transform.position,nextWaypoint.transform.position);

        distanceWaypoint=(nextDistance>nextWaypointDistance)?-curDistance:curDistance;

        // 차량의 진행 방향 벡터
        Vector3 carDirection = car.forward;

        // 현재 Waypoint에서 다음 Waypoint로의 벡터
        Vector3 waypointDirection = (nextWaypoint.position - currentWaypoint.position).normalized;

        // 차량 방향과 Waypoint 방향의 내적 계산
        float dot = Vector3.Dot(carDirection, waypointDirection);

        // 차량의 속도 (뒤로 가면 음수)
        carSpeed = Vector3.Dot(car.forward, car.GetComponent<Rigidbody>().velocity);

        // 역주행 판단 (dot < reverseThreshold) 또는 속도가 음수일 경우
        if (dot < reverseThreshold || carSpeed < 0)
        {
            if (!isReversing) // 역주행 상태가 아니면
            {
                isReversing = true; // 역주행 상태로 설정
                if (reverseCoroutine == null) // 코루틴이 실행 중이 아니면
                {
                    reverseCoroutine = StartCoroutine(ShowReverseImageWithDelay());
                }
            }
        }
        else // 정방향일 경우
        {
            if (isReversing) // 역주행 상태였으면
            {
                isReversing = false; // 역주행 상태 해제
                if (reverseCoroutine != null) // 코루틴이 실행 중이면 중지
                {
                    StopCoroutine(reverseCoroutine);
                    reverseCoroutine = null;
                }
                HideReverseImage(); // 즉시 이미지 숨기기
            }
        }

        // Waypoint 도달 여부 확인 (정상 주행일 경우만 갱신)
        if (!isReversing && Vector3.Distance(car.position, nextWaypoint.position) < waypointThreshold) 
        {
            // 다음 Waypoint로 이동
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private IEnumerator ShowReverseImageWithDelay()
    {
        yield return new WaitForSeconds(2f); // 1초 대기
        if (isReversing) // 여전히 역주행 상태인지 확인
        {
            reverseImage.gameObject.SetActive(true); // 이미지 활성화
        }
    }

    private void HideReverseImage()
    {
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false); // 이미지 비활성화
        }
    }
}