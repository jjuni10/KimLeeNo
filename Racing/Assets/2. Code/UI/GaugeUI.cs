using UnityEngine;
using UnityEngine.UI;

public class BoostGaugeUI : MonoBehaviour
{
    public static BoostGaugeUI Instance;

    [SerializeField] private Slider boostSlider; // Boost �������� ǥ���ϴ� �����̴�
    [SerializeField] public BoostGauge boostGauge; // BoostGauge ������Ʈ ����

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
            boostSlider.value = boostGauge.BoostingGauge; // BoostGauge�� ���� �����̴��� �ݿ�
        }
    }
}