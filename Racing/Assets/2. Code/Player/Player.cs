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
    public AudioSource audioSource;  // 오디오 소스를 연결할 변수
    public AudioClip boostSound;     // 부스트 소리를 연결할 변수

    [Header("Drift")]
    public bool isDrifting;
    public float driftPower;
    public float particleOverTime;
    public ParticleSystem leftTireParticle;
    public ParticleSystem rightTireParticle;
    ParticleSystem.EmissionModule leftEmissionModule;
    ParticleSystem.EmissionModule rightEmissionModule;
    public AudioSource audio; // AudioSource 연결
    public AudioClip driftSound;    // 드리프트 소리


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

    [Header("Respawn")]
    public ReverseDetection respawnPos;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        leftEmissionModule = leftTireParticle.emission;
        rightEmissionModule=rightTireParticle.emission;
    }

    void Start()
    {
        cameraController = GameManager.Instance.cameraControl;
        this.GetComponent<ReverseDetection>().carName = "Player";
    }

    void Update()
    {
        // 현재 속도 표시
        curSpeed = _rb.velocity.magnitude;

        cameraController.OffsetChange(_rb.velocity.magnitude, isBoosting);

        PlayerBoost();

        PlayerDrift();

        PlayerBrake();
    }

    void LateUpdate()
    {
        // 플레이어 각도 전환
        PlayerRotation();

        // 속도 제한
        LimitSpeed();
    }

    void FixedUpdate()
    {
        // 플레이어 움직임
        if(_canMove)
        {
            PlayerMove();
        }
        else
        {
            //PlayerNotOnGround();
        }
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
            // 바퀴 파티클
            leftEmissionModule.rateOverTime = particleOverTime*curSpeed;
            rightEmissionModule.rateOverTime = particleOverTime*curSpeed;

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
            if (!isBraking)
            {
                _rb.AddForce(driftForce, ForceMode.Acceleration);
            }
        }
        else
        {
            // 바퀴 파티클
            leftEmissionModule.rateOverTime = 0;
            rightEmissionModule.rateOverTime = 0;

            // 이동 (AddForce를 사용하여 힘을 가함)
            if (!isBraking)
            {
                _rb.AddForce(_moveVec * currentSpeed, ForceMode.Acceleration);
            }
        }

        if (isBraking)
        {
            // 브레이크 감속 적용
            _rb.velocity += -transform.forward * brakeForce * Time.deltaTime;

            // 속도가 뒤쪽으로 이동하지 않도록 제어
            if (Vector3.Dot(_rb.velocity, -transform.forward) > 0) // 뒤로 이동 중인지 확인
            {
                _rb.velocity = Vector3.zero; // 완전히 정지
                _rb.angularVelocity = Vector3.zero;
            }
        }

        // 후진 체크
        isBack = (_v < 0);
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Vector3 respawnVec = respawnPos.waypoints[respawnPos.currentWaypointIndex].position;

            // Z축 회전을 0도로 설정
            Quaternion targetRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            _rb.MoveRotation(targetRotation);

            // Y 위치를 스케일의 2배만큼 위로 이동
            transform.position = new Vector3(
                respawnVec.x,
                respawnVec.y + transform.localScale.y * 2,
                respawnVec.z
            ) ;

            // 외부 가해지는 힘 초기화
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
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
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isBoosting)
        {
            if (canBoost)
            {
                // 소리 재생
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

    // 부스트 소리를 재생하는 함수
    void PlayBoostSound()
    {
        if (boostSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(boostSound);
        }
    }

    void PlayerDrift()
    {
        // 드리프트 입력을 체크
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

    // 드리프트 소리를 재생하는 함수
    void StartDriftSound()
    {
        if (audioSource != null && driftSound != null)
        {
            audioSource.clip = driftSound; // 드리프트 소리 설정
            if (!audioSource.isPlaying)   // 이미 재생 중이 아니면 재생
            {
                audioSource.Play();
            }
        }
    }

    // 드리프트 소리 정지
    void StopDriftSound()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop(); // 소리 정지
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

    // 공중에 떴을 때 회전 제어
    void PlayerNotOnGround()
    {
        Vector3 stabilizationTorque = new Vector3(-_rb.angularVelocity.x, 0, -_rb.angularVelocity.z);
        _rb.AddTorque(stabilizationTorque, ForceMode.VelocityChange);
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
