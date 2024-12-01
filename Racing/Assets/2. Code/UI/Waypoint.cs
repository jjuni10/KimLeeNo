using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    public Transform[] waypoints; // Waypoints �迭
    public float radius = 1f; // ���� ������
    public Color waypointColor = Color.green; // �� ����
    public Color lineColor = Color.red; // Waypoint �� ���ἱ ����

    private void OnDrawGizmos()
    {
        // Waypoints �� ���ἱ�� �׷��ִ� ����� �ڵ�
        if (waypoints != null && waypoints.Length > 1)
        {
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position); // Waypoint �� �� �׸���
            }
        }

        // Waypoints�� ���� �׸���
        if (waypoints != null)
        {
            Gizmos.color = waypointColor;
            foreach (Transform waypoint in waypoints)
            {
                Gizmos.DrawWireSphere(waypoint.position, radius); // �� Waypoint�� �� �׸���
            }
        }
    }
}