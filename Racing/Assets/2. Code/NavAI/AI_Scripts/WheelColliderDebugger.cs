using UnityEngine;

public class WheelColliderDebugger : MonoBehaviour
{
    void OnDrawGizmos()
    {
        WheelCollider wheel = GetComponent<WheelCollider>();
        if (wheel != null)
        {
            // Wheel Collider�� ���� ��ǥ�� ȸ�� ���� ��������
            Vector3 position;
            Quaternion rotation;
            wheel.GetWorldPose(out position, out rotation);

            // Wheel Collider �������� ǥ��
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(position, wheel.radius);
        }
    }
}
