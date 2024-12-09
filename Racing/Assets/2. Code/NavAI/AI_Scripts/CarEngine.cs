using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public enum CarType { Aggressive, Defensive, SpeedFocused, Balanced }
    public CarType carType; // ���� Ÿ�� ����

    // �ڵ��� ������ ���� �ٸ� ������ �Ҵ�
    [Header("Car Objects")]
    public GameObject aggressiveCarPrefab;
    public GameObject defensiveCarPrefab;
    public GameObject speedFocusedCarPrefab;
    public GameObject balancedCarPrefab;


    public Transform path;
    public float maxSteerAngle = 50f;  // ������ �ִ� ȸ�� ����
    public float turnSpeed = 6f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    public GameObject skidPrefab; // ��Ű�� ��ũ ������
    private float skidTime; // ��Ű�� ��ũ ���� ���� Ÿ�̸�

    public float maxMotorTorque = 100f;  // ������ ȸ����
    public float maxBrakeTorque = 200f; // �극��ũ �Ǵ� ��
    public float currentSpeed;          // ���� �ӵ�
    public float maxSpeed = 120f;       // �ְ� �ӵ�

    public Vector3 centerOfMass;        // �ڵ����� ����

    public bool isBraking = false;      // �극��ũ ���� üũ

    [Header("Sensors")]
    public float sensorLength = 2.5f;
    public Vector3 frontSensorPosition = new Vector3(0, 0.65f, 0.5f);
    public float frontSideSensorPosition = 1f;
    public float frontSensorAngle = 30f;

    private List<Transform> nodes;
    private int currentNode = 0;
    private bool avoiding = false;
    private float targetSteerAngle = 0;

    private Transform targetCar; // ���� �ڵ����� �����ϱ� ���� ����
    public float detectionRange = 20f; // �⺻ Ž�� ����

    private int lastNodeIndex = 0; // ���������� ����ģ ��� �ε��� ����

    private bool isReadyToMove = false; // ������ ��� �غ� �Ϸ� �������� üũ
    private bool isBoosting;


    // Start is called before the first frame update
    private void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        SetTireFrictionByCarType(); // ������ ���� ������ �ʱ�ȭ

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
        StartCoroutine(WaitBeforeStarting(5f)); // 5�� ����� ���
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

        // ��� ������ ������ ����
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
        rb.velocity = Vector3.zero; // ���ӵ� �ʱ�ȭ
        rb.angularVelocity = Vector3.zero; // ���ӵ� �ʱ�ȭ
    }
        ApplyBrakes();
        yield return new WaitForSeconds(waitTime); // ������ �ð� ���
        ReleaseBrakes();
        isReadyToMove = true; // ������ ������ �غ� �Ϸ�
        StartCoroutine(InitialBoost(5f, 4f)); // 5�� ���� 2�� ���� �ν�Ʈ
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
        maxMotorTorque *= boostMultiplier; // ��ũ ����

        yield return new WaitForSeconds(boostDuration); // �ν�Ʈ ���� �ð�

        maxMotorTorque = originalMotorTorque; // ���� ��ũ�� ����
        isBoosting = false;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!isReadyToMove) return; // ��� �غ� �� ������ ������Ʈ �ߴ�

        DetectFrontCar();           // �� ���� Ž�� �� �߿� ó��
        CheckOvertakeCompletion();  // �߿� �Ϸ� ���� Ȯ��
        Sensors();                  // ��ֹ� ����
        AdjustSpeedForCurve();      // Ŀ�꿡 ���� �ӵ� ����
        ApplySteer();               // ���� ��θ� ���� ȸ��
        Drive();                    // �ڵ����� ������
        CheckWaypointDistance();    // ��ο��� �Ÿ� ����
        Braking();                  // �극��ũ
        LerpToSteerAngle();         // �ε巯�� ȸ��
        CreateSkidMarks();          // ��Ű�� ��ũ ���� �Լ� ȣ��
        CheckIfFlipped();           // ���� ������ ����
    }
    private void DetectFrontCar()
    {
        RaycastHit hit;
        Vector3 sensorStartPos = transform.position + transform.up * frontSensorPosition.y;
        if (Physics.Raycast(sensorStartPos, transform.forward, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Car"))
            {
                targetCar = hit.collider.transform; // ���� �ڵ��� ����
                HandleOvertaking(); // �߿� ó��
            }
        }
        else
        {
            targetCar = null; // Ž���� ������ ���� ���
        }
    }


    private void HandleOvertaking()
    {
        if (targetCar == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetCar.position);

        switch (carType)
        {
            case CarType.Aggressive:
                if (distanceToTarget < 10f) // ����� �Ÿ����� ��� �߿�
                {
                    PerformOvertake(1.5f);
                }
                else if (distanceToTarget < 15f && distanceToTarget > 5f) // �߰� �Ÿ����� �߿�
                {
                    PerformOvertake(1.2f);
                }
                break;

            case CarType.SpeedFocused:
                if (distanceToTarget < 10f) // ����� �Ÿ����� ��� �߿�
                {
                    PerformOvertake(1.5f);
                }
                else if (distanceToTarget < 20f && distanceToTarget > 10f) // �߰� �Ÿ����� �߿�
                {
                    PerformOvertake(1.2f);
                }
                break;

            case CarType.Defensive:
                // ����� ������ �߿����� ����
                break;

            case CarType.Balanced:
                if (distanceToTarget < 12f && distanceToTarget > 6f) // ������ �Ÿ����� �߿�
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

        // �߿� ��� ���
        Vector3 overtakePosition = targetCar.position + targetCar.right * direction * 3f;
        Vector3 relativeOvertakePosition = transform.InverseTransformPoint(overtakePosition);

        // �ε巯�� ����
        float desiredSteerAngle = (relativeOvertakePosition.x / relativeOvertakePosition.magnitude) * maxSteerAngle;
        targetSteerAngle = Mathf.Lerp(targetSteerAngle, desiredSteerAngle, Time.deltaTime * turnSpeed);

        // �ε巯�� �ӵ� ����
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

        // �߿� �Ϸ� ����
        if (distanceToTarget > 5f) // ����� �־��� ���
        {
            targetCar = null; // �߿� ��� ����
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

        if (turnAngle > 30f) // Ŀ�� ������ 30�� �̻��� ���
        {
            float slowdownFactor;
            float brakeFactor;

            if (turnAngle > 60f) // Ŀ�� ������ 60�� �̻��� ���
            {
                slowdownFactor = Mathf.Clamp((turnAngle - 60f) / 60f, 0f, 1f); // ���� ���� ����
                brakeFactor = 1.5f; // 60�� �̻󿡼� �극��ũ ���� ����

                float reducedSpeed = maxSpeed * 0.3f * (1f - slowdownFactor); // 30%�� �ӵ��� �߰��� ����
                currentSpeed = Mathf.Lerp(currentSpeed, reducedSpeed, Time.deltaTime * 5f); // �� ������ ����
            }
            else // 30�� �ʰ�, 60�� ������ ���
            {
                slowdownFactor = Mathf.Clamp((turnAngle - 30f) / 30f, 0f, 1f); // �⺻ ���� ����
                brakeFactor = 1.0f; // �⺻ �극��ũ ����

                float reducedSpeed = maxSpeed * (1f - slowdownFactor); // ���ӵ� �ӵ�
                currentSpeed = Mathf.Lerp(currentSpeed, reducedSpeed, Time.deltaTime * 2f); // �ε巴�� ����
            }

            // �극��ũ �߰� (�극��ũ ���� �ݿ�)
            wheelRL.brakeTorque = maxBrakeTorque * slowdownFactor * brakeFactor;
            wheelRR.brakeTorque = maxBrakeTorque * slowdownFactor * brakeFactor;
        }
        else
        {
            // �극��ũ ����
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
                adjustedMaxMotorTorque *= 1.8f; // ���� �� 1.5
                maxSpeed = 140f;
                break;
            case CarType.Defensive:
                adjustedMaxMotorTorque *= 1.5f; // ���� �� 0.8
                maxSpeed = 120f;
                break;
            case CarType.SpeedFocused:
                adjustedMaxMotorTorque *= 2.2f; // ���� �� 2.0
                maxSpeed = 160f;
                break;
            case CarType.Balanced:
                adjustedMaxMotorTorque *= 1.6f; // ���� �� 2.0
                maxSpeed = 130f;
                break;
        }

        // �ν�Ʈ ���¿��� �߰� ���� ����
        if (isBoosting)
        {
            adjustedMaxMotorTorque *= 1.5f; // �ν�Ʈ �� �߰� ����
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
            lastNodeIndex = currentNode; // ���� ��带 ���������� ����ģ ���� ����

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
                bool isDrifting = sidewaysSlip > 2.0f; // �帮��Ʈ ����
                bool isAccelerating = forwardSlip > 1.5f; // �ް��� ����
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

    private void Respawn()
    {
        if (nodes != null && nodes.Count > 0)
        {
            // ���������� ����ģ ����� ��ġ�� ������
            Transform lastNode = nodes[lastNodeIndex];
            transform.position = lastNode.position + Vector3.up * 1.5f; // ������ �ణ ���� ��� ��ġ
            transform.rotation = lastNode.rotation;

            // ���� �ӵ� �ʱ�ȭ
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // ���� ���� �ʱ�ȭ
            currentSpeed = 0;
            targetSteerAngle = 0;
            isBraking = false;

            // ���� ��带 ������ ������ �°� ����
            currentNode = lastNodeIndex + 1;

            // ������ ȸ���� ��� �������� ����
            if (nodes.Count > currentNode + 1)
            {
                Vector3 directionToNextNode = (nodes[currentNode + 1].position - nodes[currentNode].position).normalized;
                transform.forward = directionToNextNode; // ��� �������� ������ ���� ����
            }
        }
    }
    private void CheckIfFlipped()
    {
        // ������ ���������� ���� (X�� ���� ����)
        if (Vector3.Dot(transform.up, Vector3.up) < 0.1f) // ���� ���Ͱ� �Ʒ��� ���ϸ� ������ ����
        {
            Respawn(); // �������ٸ� ������
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Respawn(); // �� �浹 �� ������
        }
    }
}
