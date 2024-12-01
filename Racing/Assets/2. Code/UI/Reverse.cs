using UnityEngine;
using UnityEngine.UI; // Image ����� ���� ���ӽ����̽�
using System.Collections;

public class ReverseDetection : MonoBehaviour
{
    public Transform[] waypoints; // Waypoints �迭
    private int currentWaypointIndex = 0; // ���� Waypoint �ε���
    private Transform car; // ���� Transform
    private bool isReversing = false; // ������ ���¸� �����ϴ� ����
    private Coroutine reverseCoroutine; // ������ �ؽ�Ʈ�� ������Ű�� �ڷ�ƾ ����

    public Image reverseImage; // ������ �˸� �̹����� ���� Image UI ��ü

    [Range(-1f, 0f)]
    public float reverseThreshold = -0.2f; // ���������� ������ Dot �Ӱ谪

    public float waypointThreshold = 5f; // Waypoint �Ÿ� �Ӱ谪

    private float carSpeed; // ������ �ӵ�

    private void Start()
    {
        car = transform; // ������ Transform ����
        if (reverseImage != null)
        {
            reverseImage.gameObject.SetActive(false); // �ʱ⿡�� �̹��� ��Ȱ��ȭ
        }
    }

    private void Update()
    {
        DetectReverseDriving();
    }

    private void DetectReverseDriving()
    {
        // ���� Waypoint�� ���� Waypoint�� ������
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        Transform nextWaypoint = waypoints[(currentWaypointIndex + 1) % waypoints.Length];

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
        yield return new WaitForSeconds(1f); // 1�� ���
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