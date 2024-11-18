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

    [Header("Zoom")]
    public float zoomSpeed;
    public float downZoom;
    public float upZoom;
    public float minZoom;
    public float maxZoom;
    public float boostZoom;

    [SerializeField] float _previousSpeed = 0f;

    void Update()
    {
        // 카메라 위치 이동 처리
        CameraMove();

        // 카메라 각도 변경 처리
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

    public void OffsetChange(float playerSpeed,bool isBoosting)
    {
        float basicTargetZoom=Mathf.Lerp(minZoom,maxZoom, playerSpeed/maxZoom);

        if (isBoosting)
        {
            basicTargetZoom += boostZoom;
        }

        float realTargetZoom = basicTargetZoom;

        if (playerSpeed > _previousSpeed)
        {
            zoomSpeed = upZoom;
        }
        else
        {
            zoomSpeed = downZoom;
        }

        float zoomFactor = Mathf.MoveTowards(offset.magnitude, realTargetZoom, zoomSpeed * Time.deltaTime);

        offset =offset.normalized*zoomFactor;

        _previousSpeed = playerSpeed;
    }
}
