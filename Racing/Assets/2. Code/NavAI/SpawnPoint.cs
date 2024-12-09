using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] rasingCars;
    public MiniMapCamera miniMapCamera; // 미니맵 카메라
    // Start is called before the first frame update
    void Start()
    {
        SpawnCar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnCar()
    {
        // spawnPoints 배열 인덱스를 저장할 리스트 생성 후 셔플
        System.Collections.Generic.List<int> spawnIndices = new System.Collections.Generic.List<int>();
        for(int i = 0; i < spawnPoints.Length; i++)
        {
            spawnIndices.Add(i);
        }
        SufflePos(spawnIndices);

        // 셔플된 인덱스 순서에 따라 각 위치에 오브젝트를 하나씩 할당
        for (int i =0; i < rasingCars.Length; i++)
        {
            int randomSpawnIndex = spawnIndices[i];
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            GameObject spawnCar=Instantiate(rasingCars[i], spawnPoint.position, spawnPoint.rotation, spawnPoint);

            // 주행 중인 오브젝트 할당
            RankManager.Instance.playCar.Add(spawnCar);

            // 플레이어 소환 시
            if (rasingCars[i].tag == "Player")
            {
                GameManager.Instance.cameraControl.target = spawnCar.transform;
                GameManager.Instance.vehicleRigidbody=spawnCar.GetComponent<Rigidbody>();
                GameManager.Instance.playerSpeedDisplay.playerRigidbody=spawnCar.GetComponent<Rigidbody>();
                GameManager.Instance.countdownManager.vehicleController=spawnCar.GetComponent<MonoBehaviour>();
                GameManager.Instance.countdownManager.vehicleRigidbody = spawnCar.GetComponent<Rigidbody>();

                // 미니맵 카메라에 플레이어 자동차를 할당
                if (miniMapCamera != null)
                {
                    miniMapCamera.SetTarget(spawnCar.transform);
                }
            }
        }

    }
    void SufflePos(System.Collections.Generic.List<int> list)
    {
        for(int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(0, list.Count);
            int temp = list[rnd];
            list[rnd] = list[i];
            list[i] = temp;
        }
    }
}
