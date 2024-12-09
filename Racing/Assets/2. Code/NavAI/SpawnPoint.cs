using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] rasingCars;
    public MiniMapCamera miniMapCamera; // �̴ϸ� ī�޶�
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
        // spawnPoints �迭 �ε����� ������ ����Ʈ ���� �� ����
        System.Collections.Generic.List<int> spawnIndices = new System.Collections.Generic.List<int>();
        for(int i = 0; i < spawnPoints.Length; i++)
        {
            spawnIndices.Add(i);
        }
        SufflePos(spawnIndices);

        // ���õ� �ε��� ������ ���� �� ��ġ�� ������Ʈ�� �ϳ��� �Ҵ�
        for (int i =0; i < rasingCars.Length; i++)
        {
            int randomSpawnIndex = spawnIndices[i];
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            GameObject spawnCar=Instantiate(rasingCars[i], spawnPoint.position, spawnPoint.rotation, spawnPoint);

            // ���� ���� ������Ʈ �Ҵ�
            RankManager.Instance.playCar.Add(spawnCar);

            // �÷��̾� ��ȯ ��
            if (rasingCars[i].tag == "Player")
            {
                GameManager.Instance.cameraControl.target = spawnCar.transform;
                GameManager.Instance.vehicleRigidbody=spawnCar.GetComponent<Rigidbody>();
                GameManager.Instance.playerSpeedDisplay.playerRigidbody=spawnCar.GetComponent<Rigidbody>();
                GameManager.Instance.countdownManager.vehicleController=spawnCar.GetComponent<MonoBehaviour>();
                GameManager.Instance.countdownManager.vehicleRigidbody = spawnCar.GetComponent<Rigidbody>();

                // �̴ϸ� ī�޶� �÷��̾� �ڵ����� �Ҵ�
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
