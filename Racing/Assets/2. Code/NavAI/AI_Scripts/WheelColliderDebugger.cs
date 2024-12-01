using UnityEngine;

public class WheelColliderDebugger : MonoBehaviour
{
    void OnDrawGizmos()
    {
        WheelCollider wheel = GetComponent<WheelCollider>();
        if (wheel != null)
        {
            // Wheel Collider의 월드 좌표와 회전 정보 가져오기
            Vector3 position;
            Quaternion rotation;
            wheel.GetWorldPose(out position, out rotation);

            // Wheel Collider 반지름을 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, wheel.radius);
        }
    }
}
