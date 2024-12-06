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
    public bool isBack;
    bool _canMove=false;

    [Header("Boost")]
    public BoostGauge boostGauge;
    public float boostPower;
    public float boostDuration;
    public bool isBoosting;
    public bool canBoost;
    float _boostEndTime=0f;
    public AudioSource audioSource;  // ����� �ҽ��� ������ ����
    public AudioClip boostSound;     // �ν�Ʈ �Ҹ��� ������ ����

    [Header("Drift")]
    public bool isDrifting;
    public float driftPower;
    public ParticleSystem leftTireParticle;
    public ParticleSystem rightTireParticle;
    ParticleSystem.EmissionModule leftEmissionModule;
    ParticleSystem.EmissionModule rightEmissionModule;
    public AudioSource audio; // AudioSource ����
    public AudioClip driftSound;    // �帮��Ʈ �Ҹ�


    [Header("Brake")]
    public float brakeForce;
    public bool isBraking;
    public Texture2D normalTexture;
    public Texture2D brakeTexture;
    public Renderer leftBrakeLight;
    public Renderer rightBrakeLight;
    public Renderer lamp;

    [Header("Camera")]
    public CameraControl cameraController;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        leftEmissionModule = leftTireParticle.emission;
        rightEmissionModule=rightTireParticle.emission;
    }

    void Update()
    {
        // ���� �ӵ� ǥ��
        curSpeed = _rb.velocity.magnitude;

        cameraController.OffsetChange(_rb.velocity.magnitude, isBoosting);

        PlayerBoost();

        PlayerDrift();

        PlayerBrake();
    }

    void LateUpdate()
    {
        // �÷��̾� ���� ��ȯ
        PlayerRotation();
    }

    void FixedUpdate()
    {
        // �÷��̾� ������
        if(_canMove)
        {
            PlayerMove();
        }

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
            // ���� ��ƼŬ
            leftEmissionModule.rateOverTime = 100;
            rightEmissionModule.rateOverTime = 100;

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
            if (!isBraking)
            {
                _rb.AddForce(driftForce, ForceMode.Acceleration);
            }
        }
        else
        {
            // ���� ��ƼŬ
            leftEmissionModule.rateOverTime = 0;
            rightEmissionModule.rateOverTime = 0;

            // �̵� (AddForce�� ����Ͽ� ���� ����)
            if (!isBraking)
            {
                _rb.AddForce(_moveVec * currentSpeed, ForceMode.Acceleration);
            }
        }

        if (isBraking)
        {
            // �극��ũ ���� ����
            _rb.velocity += -transform.forward * brakeForce * Time.deltaTime;

            // �ӵ��� �������� �̵����� �ʵ��� ����
            if (Vector3.Dot(_rb.velocity, -transform.forward) > 0) // �ڷ� �̵� ������ Ȯ��
            {
                _rb.velocity = Vector3.zero; // ������ ����
                _rb.angularVelocity = Vector3.zero;
            }
        }

        // ���� üũ
        isBack = (_v < 0);
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            // Z�� ȸ���� 0���� ����
            Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            _rb.MoveRotation(targetRotation);

            // Y ��ġ�� �������� 2�踸ŭ ���� �̵�
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + transform.localScale.y,
                transform.position.z
            );

            // �ܺ� �������� �� �ʱ�ȭ
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting)
        {
            if (canBoost)
            {
                // �Ҹ� ���
                PlayBoostSound();

                isBoosting = true;
                _boostEndTime = Time.time + boostDuration;

                boostGauge.BoostingGauge = 0;
                canBoost = false;
            }
        }

        if (isBoosting && Time.time > _boostEndTime)
        {
            isBoosting = false;
        }
    }

    // �ν�Ʈ �Ҹ��� ����ϴ� �Լ�
    void PlayBoostSound()
    {
        if (boostSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(boostSound);
        }
    }

    void PlayerDrift()
    {
        // �帮��Ʈ �Է��� üũ
        bool isDriftInput = Input.GetKey(KeyCode.LeftControl) && (_h == -1 || _h == 1);

        if (isDriftInput && !isDrifting)
        {
            StartDriftSound();
            isDrifting = true;
        }
        else if (!isDriftInput && isDrifting)
        {
            StopDriftSound();
            isDrifting = false;
        }
    }

    // �帮��Ʈ �Ҹ��� ����ϴ� �Լ�
    void StartDriftSound()
    {
        if (audioSource != null && driftSound != null)
        {
            audioSource.clip = driftSound; // �帮��Ʈ �Ҹ� ����
            if (!audioSource.isPlaying)   // �̹� ��� ���� �ƴϸ� ���
            {
                audioSource.Play();
            }
        }
    }

    // �帮��Ʈ �Ҹ� ����
    void StopDriftSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop(); // �Ҹ� ����
        }
    }

    void PlayerBrake()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            isBraking = true;

            leftBrakeLight.material.mainTexture = brakeTexture;
            rightBrakeLight.material.mainTexture = brakeTexture;
            lamp.material.mainTexture = brakeTexture;
        }
        else
        {
            isBraking = false;

            leftBrakeLight.material.mainTexture = normalTexture;
            rightBrakeLight.material.mainTexture = normalTexture;
            lamp.material.mainTexture = normalTexture;
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
