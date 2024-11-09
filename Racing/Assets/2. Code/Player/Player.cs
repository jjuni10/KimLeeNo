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
        // �÷��̾� ������
        PlayerMove();

        // �÷��̾� ���� ��ȯ
        PlayerRotation();

        // �ӵ� ����
        LimitSpeed();
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
        _rb.AddForce(_moveVec * speed, ForceMode.Acceleration);
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
        if (_rb.velocity.magnitude > maxSpeed)
        {
            _rb.velocity = _rb.velocity.normalized * maxSpeed;
        }
    }
}
