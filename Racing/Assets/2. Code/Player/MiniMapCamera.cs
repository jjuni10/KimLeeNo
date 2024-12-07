using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform target; // 따라갈 자동차
    public Vector3 offset = new Vector3(0, 10, 0); // 자동차와 카메라 간의 오프셋
    public bool rotateWithCar = false; // 자동차 회전 방향에 따라 카메라 회전 여부

    private void LateUpdate()
    {
        if (target == null) return;

        // 자동차의 위치 + 오프셋으로 카메라 위치 설정
        transform.position = target.position + offset;

        if (rotateWithCar)
        {
            // 자동차의 회전을 따라감
            transform.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0);
        }
        else
        {
            // 고정된 회전을 유지 (미니맵용)
            transform.rotation = Quaternion.Euler(90, 0, 0); // 위에서 내려다보는 고정 각도
        }
    }

    // 외부에서 호출해 자동차를 할당하는 메서드
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
