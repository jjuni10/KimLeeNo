using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public Transform[] spawnPoints;
    public GameObject[] rasingCars;
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
            Instantiate(rasingCars[i], spawnPoint.position, spawnPoint.rotation, spawnPoint);
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
