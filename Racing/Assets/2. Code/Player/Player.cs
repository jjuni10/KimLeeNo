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
    public float basicSpeed;
    public float maxSpeed;
    public float turnSpeed;
    bool _canMove=false;

    [Header("Boost")]
    public float boostPower;
    public float boostDuration;
    public bool isBoosting;
    float _boostEndTime=0f;

    [Header("Drift")]
    public float driftFactor;
    public float driftGrip;
    public float normalDrag;
    public float driftDrag;
    public float angularDragValue;
    public float angularDragNormal;
    public bool isDrifting;

    [Header("Camera")]
    public CameraControl cameraController;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        PlayerBoost();

        // �÷��̾� ������
        if(_canMove)
        {
            PlayerMove();
        }

        // �÷��̾� ���� ��ȯ
        PlayerRotation();

        // �÷��̾� �帮��Ʈ
        PlayerDrift();

        // �ӵ� ����
        LimitSpeed();

        cameraController.OffsetChange(_rb.velocity.magnitude,isBoosting);
    }

    void PlayerMove()
    {
        _h = Input.GetAxis("Horizontal");
        _v = Input.GetAxis("Vertical");

        // ���� ����
        float mappedX = _h * Mathf.Sqrt(1 - 0.5f * _v * _v);
        float mappedZ = _v * Mathf.Sqrt(1 - 0.5f * _h * _h);

        _moveVec = transform.forward*mappedZ;

        // �밢�� �ӵ� �����ϰ� ����
        if (_moveVec.magnitude > 1)
        {
            _moveVec.Normalize();
        }

        // �̵� (AddForce�� ����Ͽ� ���� ����)
        float currentSpeed = isBoosting ? speed * boostPower : speed;
        _rb.AddForce(_moveVec * currentSpeed, ForceMode.Acceleration);
    }

    void PlayerRotation()
    {
        // �̵� �������� ȸ��
        if (Mathf.Abs(_h)>0.1f)
        {
            float rotation = _h * turnSpeed * Time.deltaTime*(_v>=0?1:_v);
            Quaternion turnOffset = Quaternion.Euler(0, rotation, 0);
            _rb.MoveRotation(_rb.rotation * turnOffset);
        }
    }

    void LimitSpeed()
    {
        // ���� �ӵ��� �����ͼ� �ִ� �ӵ��� ���� �ʵ��� ����
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

            Vector3 forwardVelocity=Vector3.Project(_rb.velocity,transform.forward);
            Vector3 rightVelocity=Vector3.Project(_rb.velocity,transform.right);

            _rb.velocity = forwardVelocity + rightVelocity * driftFactor;

            _rb.drag = driftDrag;
            _rb.angularDrag = angularDragValue;

            speed *= driftGrip;
        }
        else
        {
            _rb.drag = normalDrag;
            _rb.angularDrag = angularDragNormal;

            speed = basicSpeed;
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