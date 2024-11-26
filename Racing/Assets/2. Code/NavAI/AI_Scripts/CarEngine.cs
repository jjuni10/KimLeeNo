using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public Transform path;
    public float maxSteerAngle = 45f;  // ������ �ִ� ȸ�� ����
    public float turnSpeed = 3f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public GameObject skidPrefab; // ��Ű�� ��ũ ������
    private Vector3 prevSkidPos = Vector3.zero; // ���� ��Ű�� ��ũ ��ġ
    private float skidTime; // ��Ű�� ��ũ ���� ���� Ÿ�̸�

    public float maxMotorTorque = 80f;  // ������ ȸ����
    public float maxBrakeTorque = 150f; // �극��ũ �Ǵ� ��
    public float currentSpeed;          // ���� �ӵ�
    public float maxSpeed = 100f;       // �ְ� �ӵ�

    public Vector3 centerOfMass;        // �ڵ����� ����

    public bool isBraking = false;      // �극��ũ ���� üũ

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
        Sensors();                  // ��ֹ� ����
        ApplySteer();               // ���� ��θ� ���� ȸ��
        Drive();                    // �ڵ����� ������
        CheckWaypointDistance();    // ��ο��� �Ÿ� ����
        Braking();                  // �극��ũ
        LerpToSteerAngle();         // �ε巯�� ȸ��
        CreateSkidMarks();          // ��Ű�� ��ũ ���� �Լ� ȣ��
    }
    private void Sensors()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position;
        sensorStartPos += transform.forward * frontSensorPosition.z;
        sensorStartPos += transform.up * frontSensorPosition.y;
        float avoidMultiplier = 0;
        avoiding = false;


        // front right ����
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
        // front right angle ����
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier -= 0.5f;
            }
        }

        // front left ����
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

        // front left angle ����
        else if (Physics.Raycast(sensorStartPos, Quaternion.AngleAxis(-frontSensorAngle, transform.up) * transform.forward, out hit, sensorLength))
        {
            if (!hit.collider.CompareTag("Terrain"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoiding = true;
                avoidMultiplier += 0.5f;
            }
        }

        // front center ����
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
        WheelCollider[] wheels = { wheelFL, wheelFR, wheelRL, wheelRR }; // �� ����
        bool shouldCreateSkidMark = false; // ��Ű�帶ũ ���� ����
        Vector3[] skidPositions = new Vector3[wheels.Length]; // ��Ű�帶ũ ��ġ

        // �� ������ ������ �����ϴ��� Ȯ��
        foreach (WheelCollider wheel in wheels)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit)) // ���� ���� Ȯ��
            {
                float sidewaysSlip = Mathf.Abs(hit.sidewaysSlip); // ���� ����
                float forwardSlip = Mathf.Abs(hit.forwardSlip);   // ���� ����

                // ��Ű�� ��ũ ���� ����
                bool isDrifting = sidewaysSlip > 0.8f; // �帮��Ʈ ����
                bool isAccelerating = forwardSlip > 0.5f; // �ް��� ����
                bool isBrakingNow = isBraking; // �극��ũ ����
                float minSpeedForSkid = 5.0f; // �ּ� �ӵ�

                if ((isDrifting || isAccelerating || isBrakingNow) && currentSpeed > minSpeedForSkid)
                {
                    shouldCreateSkidMark = true; // ������ �����ϸ� ��Ű�� ��ũ ���� �÷��� ����
                    skidPositions[System.Array.IndexOf(wheels, wheel)] = hit.point + hit.normal * 0.01f; // ��ġ ����
                }
            }
        }

        // ������ �������� ���, ��� ������ ��Ű�� ��ũ ����
        if (shouldCreateSkidMark && skidTime > 0.02f)
        {
            foreach (WheelCollider wheel in wheels)
            {
                WheelHit hit;
                if (wheel.GetGroundHit(out hit)) // ���� ���� Ȯ��
                {
                    int wheelIndex = System.Array.IndexOf(wheels, wheel); // ���� ������ �ε���
                    Vector3 skidPosition = skidPositions[wheelIndex];    // ����� ��ġ ��������
                    Quaternion rot = Quaternion.LookRotation(transform.forward);

                    GameObject skidInstance = Instantiate(skidPrefab, skidPosition, rot);
                    skidInstance.AddComponent<GameObjectDestroy>(); // ���� �ð� �� ����
                }
            }
            skidTime = 0; // Ÿ�̸� �ʱ�ȭ
        }

        skidTime += Time.deltaTime; // ���� ����
    }
}
