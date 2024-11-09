using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    float _h;
    float _v;
    Vector3 _moveVec;
    Rigidbody _rb;

    [Header("Player")]
    public float speed;
    public float maxSpeed;
    public float turnSpeed;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // 플레이어 움직임
        PlayerMove();

        // 플레이어 각도 전환
        PlayerRotation();

        // 속도 제한
        LimitSpeed();
    }

    void PlayerMove()
    {
        _h = Input.GetAxis("Horizontal");
        _v = Input.GetAxis("Vertical");

        // 맵핑 적용
        float mappedX = _h * Mathf.Sqrt(1 - 0.5f * _v * _v);
        float mappedZ = _v * Mathf.Sqrt(1 - 0.5f * _h * _h);

        _moveVec = transform.forward*mappedZ;

        // 대각선 속도 일정하게 유지
        if (_moveVec.magnitude > 1)
        {
            _moveVec.Normalize();
        }

        // 이동 (AddForce를 사용하여 힘을 가함)
        _rb.AddForce(_moveVec * speed, ForceMode.Acceleration);
    }

    void PlayerRotation()
    {
        // 이동 방향으로 회전
        if (Mathf.Abs(_h)>0.1f)
        {
            float rotation = _h * turnSpeed * Time.deltaTime*(_v>=0?1:_v);
            Quaternion turnOffset = Quaternion.Euler(0, rotation, 0);
            _rb.MoveRotation(_rb.rotation * turnOffset);
        }
    }

    void LimitSpeed()
    {
        // 현재 속도를 가져와서 최대 속도를 넘지 않도록 제한
        if (_rb.velocity.magnitude > maxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * maxSpeed;
        }
    }
}
