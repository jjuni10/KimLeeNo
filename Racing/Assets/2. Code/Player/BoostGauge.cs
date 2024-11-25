using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostGauge : MonoBehaviour
{
    [Header("Gauge")]
    [SerializeField] float boostingGauge;

    public float BoostingGauge
    {
        get { return boostingGauge; }
        set { boostingGauge = Mathf.Clamp(value, 0f, 100f); }
    }
}
