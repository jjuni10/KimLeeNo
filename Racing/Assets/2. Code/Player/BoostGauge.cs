using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostGauge : MonoBehaviour
{
    public Player player;

    [Header("Gauge")]
    public float gaugeValue;
    public float driftValue;
    [SerializeField] float boostingGauge;

    public float BoostingGauge
    {
        get { return boostingGauge; }
        set 
        { 
            boostingGauge = Mathf.Clamp(value, 0f, 100f);

            if (boostingGauge == 100)
            {
                player.canBoost = true;
            }
        }
    }

    void Update()
    {
        GaugeUp(player.curSpeed);
    }

    void GaugeUp(float velocity)
    {
        if (!player.isBack)
        {
            BoostingGauge += velocity / gaugeValue * (player.isDrifting ? driftValue : 1f) * Time.deltaTime;
        }
    }
}
