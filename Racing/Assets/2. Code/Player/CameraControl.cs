using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class CameraControl : MonoBehaviour
{
    [Header("Position")]
    public Transform target;
    public Vector3 offset;
    public Quaternion rotation;

    void Update()
    {
        // ī�޶� ��ġ �̵� ó��
        CameraMove();

        // ī�޶� ���� ���� ó��
        CameraRotation();
    }

    void CameraMove()
    {
        Vector3 cameraPos = target.position + target.rotation * offset;

        transform.position = cameraPos;
    }

    void CameraRotation()
    {
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);

        transform.rotation = targetRotation;
    }
}
