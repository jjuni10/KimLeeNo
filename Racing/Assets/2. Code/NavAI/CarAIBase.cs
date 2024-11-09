using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

class CarAIStats
{
    public float speed { get; set; }
    public float angularSpeed { get; set; }
    public float acceleration { get; set; }
    public float autoBraking { get; set; }
    public float radius { get; set; }
    public float priority { get; set; }

    public void InitalizeStats(float speed, float angularSpeed, float acceleration, float autoBraking, float radius, float priority)
    {

    }
}
public class CarAIBase : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
