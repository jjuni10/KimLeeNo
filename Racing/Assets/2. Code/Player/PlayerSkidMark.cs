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
        // 스키드 마크를 그릴 MeshCollider가 있는 바퀴들
        MeshCollider[] meshColliders = GetComponentsInChildren<MeshCollider>();
        bool shouldCreateSkidMark = Input.GetKey(KeyCode.LeftControl); // 조건 체크
        Vector3[] skidPositions = new Vector3[meshColliders.Length]; // 스키드 마크 위치 저장

        if (shouldCreateSkidMark)
        {
            for (int i = 0; i < meshColliders.Length; i++)
            {
                MeshCollider wheelMeshCollider = meshColliders[i];

                // Raycast를 통해 MeshCollider의 접지 정보를 확인
                Ray ray = new Ray(wheelMeshCollider.transform.position, -transform.up);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 1.0f)) // 바퀴 아래로 Ray를 쏴 접지 확인
                {
                    skidPositions[i] = hit.point + hit.normal * 0.01f; // 접지 위치 저장 (약간 위로 올림)
                    Quaternion rot = Quaternion.LookRotation(transform.forward);

                    // 스키드 마크 생성
                    GameObject skidInstance = Instantiate(skidPrefab, skidPositions[i], rot);
                    skidInstance.AddComponent<GameObjectDestroy>(); // 일정 시간 후 삭제
                }
            }
        }
    }
}
