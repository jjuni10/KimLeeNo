using UnityEngine;
using UnityEngine.UI; // Image ����� ���� ���ӽ����̽�
using System.Collections;

public class ReverseDetection : MonoBehaviour
{
    [Header("Rank)")]
    public float rankData;
    public LapCounter curLap;
    public float distanceWaypoint;
    public string carName;

    [Header("NMS")]
    public Transform[] waypoints; // Waypoints �迭
    public int currentWaypointIndex = 0; // ���� Waypoint �ε���
    private Transform car; // ���� Transform
    private bool isReversing = false; // ������ ���¸� �����ϴ� ����
    private Coroutine reverseCoroutine; // ������ �ؽ�Ʈ�� ������Ű�� �ڷ�ƾ ����

    public Image reverseImage; // ������ �˸� �̹����� ���� Image UI ��ü

    [Range(-1f, 0f)]
    public float reverseThreshold = -0.2f; // ���������� ������ Dot �Ӱ谪

    public float waypointThreshold = 15f; // Waypoint �Ÿ� �Ӱ谪

    private float carSpeed; // ������ �ӵ�

    private void Start()
    {
        waypoints = GameManager.Instance.waypointsManager.waypoints;
        reverseImage = GameManager.Instance.reverseImage;

        car = transform; // ������ Transform ����
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false); // �ʱ⿡�� �̹��� ��Ȱ��ȭ
        }
    }

    private void Update()
    {
        DetectReverseDriving();

        rankData = currentWaypointIndex * 100+curLap.currentLap*100000+distanceWaypoint;
    }

    private void DetectReverseDriving()
    {
        // ���� Waypoint�� ���� Waypoint�� ������
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Transform nextWaypoint = waypoints[(currentWaypointIndex + 1) % waypoints.Length];
        Transform previousWaypoint = waypoints[(currentWaypointIndex-1+waypoints.Length)%waypoints.Length];

        float curDistance=Vector3.Distance(this.gameObject.transform.position,currentWaypoint.transform.position);
        float nextDistance=Vector3.Distance(this.gameObject.transform.position,nextWaypoint.transform.position);
        float nextWaypointDistance=Vector3.Distance(currentWaypoint.transform.position,nextWaypoint.transform.position);

        distanceWaypoint=(nextDistance>nextWaypointDistance)?-curDistance:curDistance;

        // ������ ���� ���� ����
        Vector3 carDirection = car.forward;

        // ���� Waypoint���� ���� Waypoint���� ����
        Vector3 waypointDirection = (nextWaypoint.position - currentWaypoint.position).normalized;

        // ���� ����� Waypoint ������ ���� ���
        float dot = Vector3.Dot(carDirection, waypointDirection);

        // ������ �ӵ� (�ڷ� ���� ����)
        carSpeed = Vector3.Dot(car.forward, car.GetComponent<Rigidbody>().velocity);

        // ������ �Ǵ� (dot < reverseThreshold) �Ǵ� �ӵ��� ������ ���
        if (dot < reverseThreshold || carSpeed < 0)
        {
            if (!isReversing) // ������ ���°� �ƴϸ�
            {
                isReversing = true; // ������ ���·� ����
                if (reverseCoroutine == null) // �ڷ�ƾ�� ���� ���� �ƴϸ�
                {
                    reverseCoroutine = StartCoroutine(ShowReverseImageWithDelay());
                }
            }
        }
        else // �������� ���
        {
            if (isReversing) // ������ ���¿�����
            {
                isReversing = false; // ������ ���� ����
                if (reverseCoroutine != null) // �ڷ�ƾ�� ���� ���̸� ����
                {
                    StopCoroutine(reverseCoroutine);
                    reverseCoroutine = null;
                }
                HideReverseImage(); // ��� �̹��� �����
            }
        }

        // Waypoint ���� ���� Ȯ�� (���� ������ ��츸 ����)
        if (!isReversing && Vector3.Distance(car.position, nextWaypoint.position) < waypointThreshold) 
        {
            // ���� Waypoint�� �̵�
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    private IEnumerator ShowReverseImageWithDelay()
    {
        yield return new WaitForSeconds(2f); // 1�� ���
        if (isReversing) // ������ ������ �������� Ȯ��
        {
            reverseImage.gameObject.SetActive(true); // �̹��� Ȱ��ȭ
        }
    }

    private void HideReverseImage()
    {
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false); // �̹��� ��Ȱ��ȭ
        }
    }
}