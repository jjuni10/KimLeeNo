using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectDestroy : MonoBehaviour
{
    private float destroyTime;

    void Update()
    {
        if (destroyTime >= 1.5f)
        {
            Destroy(gameObject);
        }
        else
        {
            destroyTime += Time.deltaTime;
        }
    }
}