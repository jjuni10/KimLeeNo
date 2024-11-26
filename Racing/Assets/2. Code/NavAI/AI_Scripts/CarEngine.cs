using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle = 45f;  // 바퀴의 최대 회전 각도
    public float turnSpeed = 3f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public GameObject skidPrefab; // 스키드 마크 프리팹
    private Vector3 prevSkidPos = Vector3.zero; // 이전 스키드 마크 위치
    private float skidTime; // 스키드 마크 생성 간격 타이머

    public float maxMotorTorque = 80f;  // 바퀴의 회전력
    public float maxBrakeTorque = 150f; // 브레이크 되는 값
    public float currentSpeed;          // 현재 속도
    public float maxSpeed = 100f;       // 최고 속도

    public Vector3 centerOfMass;        // 자동차의 중점

    public bool isBraking = false;      // 브레이크 상태 체크

    [Header("Sensors")]
    public float sensorLength = 2f;
    public Vector3 frontSensorPosition = new Vector3(0, 0.65f, 0.5f);
    public float frontSideSensorPosition = 1f;
    public float frontSensorAngle = 30f;

    private List<Transform> nodes;
    private int currentNode = 0;
    private bool avoiding = false;
    private float targetSteerAngle = 0;

    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

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
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Sensors();                  // 장애물 감지
        ApplySteer();               // 다음 경로를 향해 회전
        Drive();                    // 자동차의 움직임
        CheckWaypointDistance();    // 경로와의 거리 측정
        Braking();                  // 브레이크
        LerpToSteerAngle();         // 부드러운 회전
        CreateSkidMarks();          // 스키드 마크 생성 함수 호출
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
        targetSteerAngle = newSteer;
    }

    private void Drive()
    {
        currentSpeed = 2 * Mathf.PI * wheelFL.radius * wheelFL.rpm * 60 / 1000;

        if(currentSpeed < maxSpeed && !isBraking)
        {
            wheelFL.motorTorque = maxMotorTorque;
            wheelFR.motorTorque = maxMotorTorque;
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
            if(currentNode == nodes.Count - 1)
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
            wheelRL.brakeTorque = maxBrakeTorque;
            wheelRR.brakeTorque = maxBrakeTorque;
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
                bool isDrifting = sidewaysSlip > 0.8f; // 드리프트 조건
                bool isAccelerating = forwardSlip > 0.5f; // 급가속 조건
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
}
