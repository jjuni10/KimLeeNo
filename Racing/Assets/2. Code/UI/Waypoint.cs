using UnityEngine;

public class WaypointsManager : MonoBehaviour
{
    public Transform[] waypoints; // Waypoints 배열
    public float radius = 1f; // 원의 반지름
    public Color waypointColor = Color.green; // 원 색상
    public Color lineColor = Color.red; // Waypoint 간 연결선 색상

    private void OnDrawGizmos()
    {
        // Waypoints 간 연결선을 그려주는 디버깅 코드
        if (waypoints != null && waypoints.Length > 1)
        {
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                Gizmos.color = lineColor;
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position); // Waypoint 간 선 그리기
            }
        }

        // Waypoints에 원을 그리기
        if (waypoints != null)
        {
            Gizmos.color = waypointColor;
            foreach (Transform waypoint in waypoints)
            {
                Gizmos.DrawWireSphere(waypoint.position, radius); // 각 Waypoint에 원 그리기
            }
        }
    }
}