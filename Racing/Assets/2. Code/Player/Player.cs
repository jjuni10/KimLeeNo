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
        // ���� �ӵ� ǥ��
        curSpeed = _rb.velocity.magnitude;

        cameraController.OffsetChange(_rb.velocity.magnitude, isBoosting);

        PlayerBoost();

        PlayerDrift();
    }

    void FixedUpdate()
    {
        // �÷��̾� ������
        if(_canMove)
        {
            PlayerMove();
        }

        // �÷��̾� ���� ��ȯ
        PlayerRotation();

        // �ӵ� ����
        LimitSpeed();
    }

    void PlayerMove()
    {
        speed = basicSpeed;

        _h = Input.GetAxis("Horizontal");
        _v = Input.GetAxis("Vertical");

        // ���� ����
        float mappedZ = _v * Mathf.Sqrt(1 - 0.5f * _h * _h);

        _moveVec = transform.forward*mappedZ;

        // �̵� (AddForce�� ����Ͽ� ���� ����)
        float currentSpeed = isBoosting ? speed * boostPower : speed;

        if (isDrifting)
        {
            Vector3 driftForce = _moveVec * currentSpeed; // �⺻ �̵� ���� ����

            if (_h < 0) // ���� ����Ű �Է�
            {
                // ���������� �� �߰�
                driftForce += transform.right*driftPower * (currentSpeed * Mathf.Abs(_h));
            }
            else if (_h > 0) // ������ ����Ű �Է�
            {
                // �������� �� �߰�
                driftForce += transform.right* driftPower * (currentSpeed * -Mathf.Abs(_h));
            }

            // �帮��Ʈ ���� �߰�
            _rb.AddForce(driftForce, ForceMode.Acceleration);
        }
        else
        {
            // �̵� (AddForce�� ����Ͽ� ���� ����)
            _rb.AddForce(_moveVec * currentSpeed, ForceMode.Acceleration);
        }
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
