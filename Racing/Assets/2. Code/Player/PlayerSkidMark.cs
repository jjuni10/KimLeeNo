using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkidMark : MonoBehaviour
{
    public GameObject skidPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CreateSkidMarks();
    }
    private void CreateSkidMarks()
    {
        // ��Ű�� ��ũ�� �׸� MeshCollider�� �ִ� ������
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>();
        bool shouldCreateSkidMark = Input.GetKey(KeyCode.LeftControl); // ���� üũ
        Vector3[] skidPositions = new Vector3[meshColliders.Length]; // ��Ű�� ��ũ ��ġ ����

        if (shouldCreateSkidMark)
        {
            for (int i = 0; i < meshColliders.Length; i++)
            {
                MeshCollider wheelMeshCollider = meshColliders[i];

                // Raycast�� ���� MeshCollider�� ���� ������ Ȯ��
                Ray ray = new Ray(wheelMeshCollider.transform.position, -transform.up);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1.0f)) // ���� �Ʒ��� Ray�� �� ���� Ȯ��
                {
                    skidPositions[i] = hit.point + hit.normal * 0.01f; // ���� ��ġ ���� (�ణ ���� �ø�)
                    Quaternion rot = Quaternion.LookRotation(transform.forward);

                    // ��Ű�� ��ũ ����
                    GameObject skidInstance = Instantiate(skidPrefab, skidPositions[i], rot);
                    skidInstance.AddComponent<GameObjectDestroy>(); // ���� �ð� �� ����
                }
            }
        }
    }
}
