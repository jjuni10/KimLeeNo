using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public enum CarType { Aggressive, Defensive, SpeedFocused, Balanced }
    public CarType carType; // 차량 타입 지정

    // 자동차 종류에 따라 다른 프리팹 할당
    [Header("Car Objects")]
    public GameObject aggressiveCarPrefab;
    public GameObject defensiveCarPrefab;
    public GameObject speedFocusedCarPrefab;
    public GameObject balancedCarPrefab;


    public Transform path;
    public float maxSteerAngle = 50f;  // 바퀴의 최대 회전 각도
    public float turnSpeed = 6f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public GameObject skidPrefab; // 스키드 마크 프리팹
    private float skidTime; // 스키드 마크 생성 간격 타이머

    public float maxMotorTorque = 100f;  // 바퀴의 회전력
    public float maxBrakeTorque = 200f; // 브레이크 되는 값
    public float currentSpeed;          // 현재 속도
    public float maxSpeed = 120f;       // 최고 속도

    public Vector3 centerOfMass;        // 자동차의 중점

    public bool isBraking = false;      // 브레이크 상태 체크

    [Header("Sensors")]
    public float sensorLength = 2.5f;
    public Vector3 frontSensorPosition = new Vector3(0, 0.65f, 0.5f);
    public float frontSideSensorPosition = 1f;
    public float frontSensorAngle = 30f;

    private List<Transform> nodes;
    private int currentNode = 0;
    private bool avoiding = false;
    private float targetSteerAngle = 0;

    private Transform targetCar; // 앞의 자동차를 감지하기 위한 변수
    public float detectionRange = 20f; // 기본 탐지 범위

    private int lastNodeIndex = 0; // 마지막으로 지나친 노드 인덱스 저장

    private bool isReadyToMove = false; // 차량이 출발 준비 완료 상태인지 체크
    private bool isBoosting;


    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        SetTireFrictionByCarType(); // 차량의 바퀴 마찰력 초기화

        GameObject pathObject = GameObject.FindWithTag("Path");
        if (pathObject != null)
        {
            path = pathObject.transform;

            Transform[] pathTransforms = path.GetComponentsInChildren<Transform>();
            nodes = new List<Transform>();

            for (int i = 0; i < pathTransforms.Length; i++)
            {
                if (pathTransforms[i] != path.transform)
                {
                    nodes.Add(pathTransforms[i]);
                }
            }
        }

        this.GetComponent<ReverseDetection>().carName=carType.ToString();
        StartCoroutine(WaitBeforeStarting(5f)); // 5초 대기후 출발
    }
    private void SetTireFrictionByCarType()
    {
        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();

        switch (carType)
        {
            case CarType.Aggressive:
                forwardFriction.extremumSlip = 0.25f;
                forwardFriction.extremumValue = 2.0f;
                forwardFriction.asymptoteSlip = 0.5f;
                forwardFriction.asymptoteValue = 1.5f;
                forwardFriction.stiffness = 2.5f;

                sidewaysFriction.extremumSlip = 0.2f;
                sidewaysFriction.extremumValue = 2.2f;
                sidewaysFriction.asymptoteSlip = 0.4f;
                sidewaysFriction.asymptoteValue = 1.7f;
                sidewaysFriction.stiffness = 3.0f;
                break;

            case CarType.Defensive:
                forwardFriction.extremumSlip = 0.35f;
                forwardFriction.extremumValue = 1.8f;
                forwardFriction.asymptoteSlip = 0.6f;
                forwardFriction.asymptoteValue = 1.2f;
                forwardFriction.stiffness = 2.0f;

                sidewaysFriction.extremumSlip = 0.3f;
                sidewaysFriction.extremumValue = 1.9f;
                sidewaysFriction.asymptoteSlip = 0.5f;
                sidewaysFriction.asymptoteValue = 1.3f;
                sidewaysFriction.stiffness = 2.5f;
                break;

            case CarType.SpeedFocused:
                forwardFriction.extremumSlip = 0.3f;
                forwardFriction.extremumValue = 2.1f;
                forwardFriction.asymptoteSlip = 0.5f;
                forwardFriction.asymptoteValue = 1.4f;
                forwardFriction.stiffness = 2.8f;

                sidewaysFriction.extremumSlip = 0.25f;
                sidewaysFriction.extremumValue = 2.0f;
                sidewaysFriction.asymptoteSlip = 0.45f;
                sidewaysFriction.asymptoteValue = 1.6f;
                sidewaysFriction.stiffness = 2.9f;
                break;

            case CarType.Balanced:
                forwardFriction.extremumSlip = 0.3f;
                forwardFriction.extremumValue = 2.0f;
                forwardFriction.asymptoteSlip = 0.5f;
                forwardFriction.asymptoteValue = 1.3f;
                forwardFriction.stiffness = 2.5f;

                sidewaysFriction.extremumSlip = 0.25f;
                sidewaysFriction.extremumValue = 2.0f;
                sidewaysFriction.asymptoteSlip = 0.4f;
                sidewaysFriction.asymptoteValue = 1.5f;
                sidewaysFriction.stiffness = 2.7f;
                break;
        }

        // 모든 바퀴에 마찰력 적용
        SetWheelFriction(wheelFL, forwardFriction, sidewaysFriction);
        SetWheelFriction(wheelFR, forwardFriction, sidewaysFriction);
        SetWheelFriction(wheelRL, forwardFriction, sidewaysFriction);
        SetWheelFriction(wheelRR, forwardFriction, sidewaysFriction);
    }

    private void SetWheelFriction(WheelCollider wheel, WheelFrictionCurve forward, WheelFrictionCurve sideways)
    {
        wheel.forwardFriction = forward;
        wheel.sidewaysFriction = sideways;
    }

    private IEnumerator WaitBeforeStarting(float waitTime)
    {
            Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.velocity = Vector3.zero; // 선속도 초기화
        rb.angularVelocity = Vector3.zero; // 각속도 초기화
    }
        ApplyBrakes();
        yield return new WaitForSeconds(waitTime); // 지정된 시간 대기
        ReleaseBrakes();
        isReadyToMove = true; // 차량을 움직일 준비 완료
        StartCoroutine(InitialBoost(5f, 4f)); // 5초 동안 2배 가속 부스트
    }
    private void ApplyBrakes()
    {
        wheelFL.brakeTorque = maxBrakeTorque;
        wheelFR.brakeTorque = maxBrakeTorque;
        wheelRL.brakeTorque = maxBrakeTorque;
        wheelRR.brakeTorque = maxBrakeTorque;
    }

    private void ReleaseBrakes()
    {
        wheelFL.brakeTorque = 0;
        wheelFR.brakeTorque = 0;
        wheelRL.brakeTorque = 0;
        wheelRR.brakeTorque = 0;
    }
    private IEnumerator InitialBoost(float boostDuration, float boostMultiplier)
    {
        isBoosting = true;

        float originalMotorTorque = maxMotorTorque;
        maxMotorTorque *= boostMultiplier; // 토크 증가

        yield return new WaitForSeconds(boostDuration); // 부스트 지속 시간

        maxMotorTorque = originalMotorTorque; // 원래 토크로 복구
        isBoosting = false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!isReadyToMove) return; // 출발 준비가 안 됐으면 업데이트 중단

        DetectFrontCar();           // 앞 차량 탐지 및 추월 처리
        CheckOvertakeCompletion();  // 추월 완료 여부 확인
        Sensors();                  // 장애물 감지
        AdjustSpeedForCurve();      // 커브에 따른 속도 조정
        ApplySteer();               // 다음 경로를 향해 회전
        Drive();                    // 자동차의 움직임
        CheckWaypointDistance();    // 경로와의 거리 측정
        Braking();                  // 브레이크
        LerpToSteerAngle();         // 부드러운 회전
        CreateSkidMarks();          // 스키드 마크 생성 함수 호출
        CheckIfFlipped();           // 차량 뒤집힘 감지
    }
    private void DetectFrontCar()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position + transform.up * frontSensorPosition.y;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Car"))
            {
                targetCar = hit.collider.transform; // 앞의 자동차 감지
                HandleOvertaking(); // 추월 처리
            }
        }
        else
        {
            targetCar = null; // 탐지된 차량이 없을 경우
        }
    }


    private void HandleOvertaking()
    {
        if (targetCar == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetCar.position);

        switch (carType)
        {
            case CarType.Aggressive:
                if (distanceToTarget < 10f) // 가까운 거리에서 즉시 추월
                {
                    PerformOvertake(1.5f);
                }
                else if (distanceToTarget < 15f && distanceToTarget > 5f) // 중간 거리에서 추월
                {
                    PerformOvertake(1.2f);
                }
                break;

            case CarType.SpeedFocused:
                if (distanceToTarget < 10f) // 가까운 거리에서 즉시 추월
                {
                    PerformOvertake(1.5f);
                }
                else if (distanceToTarget < 20f && distanceToTarget > 10f) // 중간 거리에서 추월
                {
                    PerformOvertake(1.2f);
                }
                break;

            case CarType.Defensive:
                // 방어형 차량은 추월하지 않음
                break;

            case CarType.Balanced:
                if (distanceToTarget < 12f && distanceToTarget > 6f) // 적당한 거리에서 추월
                {
                    PerformOvertake(1.0f);
                }
                break;
        }
    }

    private void PerformOvertake(float speedMultiplier)
    {
        if (targetCar == null) return;

        Vector3 relativePosition = transform.InverseTransformPoint(targetCar.position);
        float direction = relativePosition.x > 0 ? 1f : -1f;

        // 추월 경로 계산
        Vector3 overtakePosition = targetCar.position + targetCar.right * direction * 3f;
        Vector3 relativeOvertakePosition = transform.InverseTransformPoint(overtakePosition);

        // 부드러운 조향
        float desiredSteerAngle = (relativeOvertakePosition.x / relativeOvertakePosition.magnitude) * maxSteerAngle;
        targetSteerAngle = Mathf.Lerp(targetSteerAngle, desiredSteerAngle, Time.deltaTime * turnSpeed);

        // 부드러운 속도 증가
        float desiredSpeed = Mathf.Min(currentSpeed + (speedMultiplier * maxMotorTorque * Time.deltaTime), maxSpeed);
        currentSpeed = Mathf.Lerp(currentSpeed, desiredSpeed, Time.deltaTime * 2f);

        switch (carType)
        {
            case CarType.Aggressive:
                currentSpeed *= 3.5f;
                break;
            case CarType.Defensive:
                break;
            case CarType.SpeedFocused:
                currentSpeed *= 3.0f;
                break;
            case CarType.Balanced:
                currentSpeed *= 2.0f;
                break;
        }
    }

        private void CheckOvertakeCompletion()
    {
        if (targetCar == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetCar.position);

        // 추월 완료 조건
        if (distanceToTarget > 5f) // 충분히 멀어진 경우
        {
            targetCar = null; // 추월 대상 해제
        }
    }


    private void Sensors()
    {

        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos += transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0;
        avoiding = false;


        // front right 센서
        sensorStartPos += transform.right * frontSideSensorPosition;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 1f;
            }
        }
        // front right angle 센서
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 0.5f;
            }
        }

        // front left 센서
        sensorStartPos -= transform.right * frontSideSensorPosition * 2;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 1f;
            }
        }

        // front left angle 센서
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 0.5f;
            }
        }

        // front center 센서
        if(avoidMultiplier == 0)
        {
            if (Physics.Raycast(sensorStartPos, transform.forward, out hit, sensorLength))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    Debug.DrawLine(sensorStartPos, hit.point);
                    avoiding = true;
                    if(hit.normal.x < 0)
                    {
                        avoidMultiplier = -1;
                    }
                    else
                    {
                        avoidMultiplier = 1;
                    }
                }
            }
        }


        if (avoiding)
        {
            targetSteerAngle = maxSteerAngle * avoidMultiplier;
        }
    }

    private void ApplySteer()
    {
        if (avoiding) return;
        Vector3 relativeVector = transform.InverseTransformPoint(nodes[currentNode].position);
        float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;

        switch (carType)
        {
            case CarType.Aggressive:
                targetSteerAngle = newSteer * 0.5f;
                break;
            case CarType.Defensive:
                targetSteerAngle = newSteer * 0.3f;
                break;
            case CarType.SpeedFocused:
                targetSteerAngle = newSteer;
                break;
            case CarType.Balanced:
                targetSteerAngle = Mathf.Lerp(newSteer, targetSteerAngle, 0.3f);
                break;
        }
    }
    private float GetTurnAngle()
    {
        if (nodes.Count < 2 || currentNode >= nodes.Count - 1) return 0f;

        Vector3 currentWaypoint = nodes[currentNode].position;
        Vector3 nextWaypoint = nodes[(currentNode + 1) % nodes.Count].position;

        Vector3 currentDirection = (currentWaypoint - transform.position).normalized;
        Vector3 nextDirection = (nextWaypoint - currentWaypoint).normalized;

        float angle = Vector3.Angle(currentDirection, nextDirection);
        return angle;
    }

    private void AdjustSpeedForCurve()
    {
        float turnAngle = GetTurnAngle();

        if (turnAngle > 30f) // 커브 각도가 30도 이상인 경우
        {
            float slowdownFactor;
            float brakeFactor;

            if (turnAngle > 60f) // 커브 각도가 60도 이상인 경우
            {
                slowdownFactor = Mathf.Clamp((turnAngle - 60f) / 60f, 0f, 1f); // 강한 감속 비율
                brakeFactor = 1.5f; // 60도 이상에서 브레이크 강도 증가

                float reducedSpeed = maxSpeed * 0.3f * (1f - slowdownFactor); // 30%의 속도를 추가로 줄임
                currentSpeed = Mathf.Lerp(currentSpeed, reducedSpeed, Time.deltaTime * 5f); // 더 빠르게 감속
            }
            else // 30도 초과, 60도 이하인 경우
            {
                slowdownFactor = Mathf.Clamp((turnAngle - 30f) / 30f, 0f, 1f); // 기본 감속 비율
                brakeFactor = 1.0f; // 기본 브레이크 강도

                float reducedSpeed = maxSpeed * (1f - slowdownFactor); // 감속된 속도
                currentSpeed = Mathf.Lerp(currentSpeed, reducedSpeed, Time.deltaTime * 2f); // 부드럽게 감속
            }

            // 브레이크 추가 (브레이크 강도 반영)
            wheelRL.brakeTorque = maxBrakeTorque * slowdownFactor * brakeFactor;
            wheelRR.brakeTorque = maxBrakeTorque * slowdownFactor * brakeFactor;
        }
        else
        {
            // 브레이크 해제
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }
    }



    private void Drive()
    {
        if (!isReadyToMove)
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
            return;
        }
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;

        float adjustedMaxMotorTorque = maxMotorTorque;

        switch (carType)
        {
            case CarType.Aggressive:
                adjustedMaxMotorTorque *= 1.8f; // 원래 값 1.5
                maxSpeed = 140f;
                break;
            case CarType.Defensive:
                adjustedMaxMotorTorque *= 1.5f; // 원래 값 0.8
                maxSpeed = 120f;
                break;
            case CarType.SpeedFocused:
                adjustedMaxMotorTorque *= 2.2f; // 원래 값 2.0
                maxSpeed = 160f;
                break;
            case CarType.Balanced:
                adjustedMaxMotorTorque *= 1.6f; // 원래 값 2.0
                maxSpeed = 130f;
                break;
        }

        // 부스트 상태에서 추가 가속 적용
        if (isBoosting)
        {
            adjustedMaxMotorTorque *= 1.5f; // 부스트 중 추가 가속
        }
        if (currentSpeed < maxSpeed && !isBraking)
        {
            wheelFL.motorTorque = adjustedMaxMotorTorque;
            wheelFR.motorTorque = adjustedMaxMotorTorque;
        }
        else
        {
            wheelFL.motorTorque = 0;
            wheelFR.motorTorque = 0;
        }
    }
    private void CheckWaypointDistance()
    {
        if(Vector3.Distance(transform.position, nodes[currentNode].position) < 15.0f)
        {
            lastNodeIndex = currentNode; // 현재 노드를 마지막으로 지나친 노드로 저장

            if (currentNode == nodes.Count - 1)
            {
                currentNode = 0;
            }
            else
            {
                currentNode++;
            }
        }
    }
    private void Braking()
    {
        if (isBraking)
        {
            float adjustedBrakeTorque = maxBrakeTorque;

            switch (carType)
            {
                case CarType.Aggressive:
                    adjustedBrakeTorque *= 0.8f;
                    break;
                case CarType.Defensive:
                    adjustedBrakeTorque *= 1.2f;
                    break;
                case CarType.SpeedFocused:
                    adjustedBrakeTorque *= 0.6f;
                    break;
                case CarType.Balanced:
                    break;
            }

            wheelRL.brakeTorque = adjustedBrakeTorque;
            wheelRR.brakeTorque = adjustedBrakeTorque;
        }
        else
        {
            wheelRL.brakeTorque = 0;
            wheelRR.brakeTorque = 0;
        }
    }
    private void LerpToSteerAngle()
    {
        wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);

    }

    private void CreateSkidMarks()
    {
        WheelCollider[] wheels = { wheelFL, wheelFR, wheelRL, wheelRR }; // 네 바퀴
        bool shouldCreateSkidMark = false; // 스키드마크 생성 여부
        Vector3[] skidPositions = new Vector3[wheels.Length]; // 스키드마크 위치

        // 한 바퀴라도 조건을 만족하는지 확인
        foreach (WheelCollider wheel in wheels)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit)) // 접지 정보 확인
            {
                float sidewaysSlip = Mathf.Abs(hit.sidewaysSlip); // 측면 슬립
                float forwardSlip = Mathf.Abs(hit.forwardSlip);   // 전방 슬립

                // 스키드 마크 생성 조건
                bool isDrifting = sidewaysSlip > 2.0f; // 드리프트 조건
                bool isAccelerating = forwardSlip > 1.5f; // 급가속 조건
                bool isBrakingNow = isBraking; // 브레이크 조건
                float minSpeedForSkid = 5.0f; // 최소 속도

                if ((isDrifting || isAccelerating || isBrakingNow) && currentSpeed > minSpeedForSkid)
                {
                    shouldCreateSkidMark = true; // 조건을 만족하면 스키드 마크 생성 플래그 설정
                    skidPositions[System.Array.IndexOf(wheels, wheel)] = hit.point + hit.normal * 0.01f; // 위치 저장
                }
            }
        }

        // 조건을 만족했을 경우, 모든 바퀴에 스키드 마크 생성
        if (shouldCreateSkidMark && skidTime > 0.02f)
        {
            foreach (WheelCollider wheel in wheels)
            {
                WheelHit hit;
                if (wheel.GetGroundHit(out hit)) // 접지 정보 확인
                {
                    int wheelIndex = System.Array.IndexOf(wheels, wheel); // 현재 바퀴의 인덱스
                    Vector3 skidPosition = skidPositions[wheelIndex];    // 저장된 위치 가져오기
                    Quaternion rot = Quaternion.LookRotation(transform.forward);

                    GameObject skidInstance = Instantiate(skidPrefab, skidPosition, rot);
                    skidInstance.AddComponent<GameObjectDestroy>(); // 일정 시간 후 삭제
                }
            }
            skidTime = 0; // 타이머 초기화
        }

        skidTime += Time.deltaTime; // 간격 증가
    }

    private void Respawn()
    {
        if (nodes != null && nodes.Count > 0)
        {
            // 마지막으로 지나친 노드의 위치로 리스폰
            Transform lastNode = nodes[lastNodeIndex];
            transform.position = lastNode.position + Vector3.up * 1.5f; // 차량을 약간 위로 띄워 배치
            transform.rotation = lastNode.rotation;

            // 물리 속도 초기화
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 차량 상태 초기화
            currentSpeed = 0;
            targetSteerAngle = 0;
            isBraking = false;

            // 현재 노드를 리스폰 지점에 맞게 설정
            currentNode = lastNodeIndex + 1;

            // 차량의 회전을 경로 방향으로 조정
            if (nodes.Count > currentNode + 1)
            {
                Vector3 directionToNextNode = (nodes[currentNode + 1].position - nodes[currentNode].position).normalized;
                transform.forward = directionToNextNode; // 경로 방향으로 차량의 방향 설정
            }
        }
    }
    private void CheckIfFlipped()
    {
        // 차량이 뒤집혔는지 감지 (X축 기울기 기준)
        if (Vector3.Dot(transform.up, Vector3.up) < 0.1f) // 위쪽 벡터가 아래로 향하면 뒤집힌 상태
        {
            Respawn(); // 뒤집혔다면 리스폰
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Respawn(); // 벽 충돌 시 리스폰
        }
    }
}
