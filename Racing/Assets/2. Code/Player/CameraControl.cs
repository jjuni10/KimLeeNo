using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Position")]
    public Transform target;
    public Vector3 offset;
    public Quaternion rotation;

    void Update()
    {
        // 카메라 위치 이동 처리
        CameraMove();

        // 카메라 각도 변경 처리
        CameraRotation();
    }

    void CameraMove()
    {
        Vector3 cameraPos = target.position+offset;

        transform.position = cameraPos;
    }

    void CameraRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = targetRotation*rotation;
    }
}
