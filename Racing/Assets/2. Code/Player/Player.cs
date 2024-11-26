using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    float _h;
    float _v;
    Vector3 _moveVec;
    Rigidbody _rb;

    public float curSpeed;

    [Header("Player")]
    public float speed;
    public float basicSpeed;
    public float maxSpeed;
    public float turnSpeed;
    bool _canMove=false;

    [Header("Boost")]
    public float boostPower;
    public float boostDuration;
    public bool isBoosting;
    public bool canBoost;
    float _boostEndTime=0f;

    [Header("Drift")]
    public bool isDrifting;
    public float driftPower;

    [Header("Camera")]
    public CameraControl cameraController;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 현재 속도 표시
        curSpeed = _rb.velocity.magnitude;

        cameraController.OffsetChange(_rb.velocity.magnitude, isBoosting);

        PlayerBoost();

        PlayerDrift();
    }

    void FixedUpdate()
    {
        // 플레이어 움직임
        if(_canMove)
        {
            PlayerMove();
        }

        // 플레이어 각도 전환
        PlayerRotation();

        // 속도 제한
        LimitSpeed();
    }

    void PlayerMove()
    {
        speed = basicSpeed;

        _h = Input.GetAxis("Horizontal");
        _v = Input.GetAxis("Vertical");

        // 맵핑 적용
        float mappedZ = _v * Mathf.Sqrt(1 - 0.5f * _h * _h);

        _moveVec = transform.forward*mappedZ;

        // 이동 (AddForce를 사용하여 힘을 가함)
        float currentSpeed = isBoosting ? speed * boostPower : speed;

        if (isDrifting)
        {
            Vector3 driftForce = _moveVec * currentSpeed; // 기본 이동 방향 유지

            if (_h < 0) // 왼쪽 방향키 입력
            {
                // 오른쪽으로 힘 추가
                driftForce += transform.right*driftPower * (currentSpeed * Mathf.Abs(_h));
            }
            else if (_h > 0) // 오른쪽 방향키 입력
            {
                // 왼쪽으로 힘 추가
                driftForce += transform.right* driftPower * (currentSpeed * -Mathf.Abs(_h));
            }

            // 드리프트 힘을 추가
            _rb.AddForce(driftForce, ForceMode.Acceleration);
        }
        else
        {
            // 이동 (AddForce를 사용하여 힘을 가함)
            _rb.AddForce(_moveVec * currentSpeed, ForceMode.Acceleration);
        }
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
        float currentMaxSpeed=isBoosting?maxSpeed*boostPower:maxSpeed;

        if (_rb.velocity.magnitude > maxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * currentMaxSpeed;
        }
    }

    void PlayerBoost()
    {
        if (Input.GetKeyDown(KeyCode.F)&&!isBoosting)
        {
            isBoosting = true;
            _boostEndTime = Time.time + boostDuration;
        }

        if(isBoosting&&Time.time>_boostEndTime)
        {
            isBoosting=false;
        }
    }

    void PlayerDrift()
    {
        if (Input.GetKey(KeyCode.H))
        {
            isDrifting = true;
        }
        else
        {
            isDrifting=false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        _canMove = true;
    }

    void OnTriggerExit(Collider other)
    {
        _canMove=false;
    }
}
