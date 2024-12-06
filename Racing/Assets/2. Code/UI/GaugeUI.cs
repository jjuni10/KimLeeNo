using UnityEngine;
using UnityEngine.UI;

public class BoostGaugeUI : MonoBehaviour
{
    public static BoostGaugeUI Instance;

    [SerializeField] private Slider boostSlider; // Boost 게이지를 표시하는 슬라이더
    [SerializeField] public BoostGauge boostGauge; // BoostGauge 컴포넌트 참조

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (boostGauge != null && boostSlider != null)
        {
            boostSlider.value = boostGauge.BoostingGauge; // BoostGauge의 값을 슬라이더에 반영
        }
    }
}