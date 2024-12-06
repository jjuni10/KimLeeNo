using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText; // �� �ð� UI

    private void Start()
    {
        string lapTimeInfo = "";
        int totalLaps = 4; // �� �� ���� �����ϰ� ����

        for (int i = 1; i <= totalLaps; i++) // �� �� �ð� �ҷ�����
        {
            if (PlayerPrefs.HasKey($"LapTime_{i}"))
            {
                float lapTime = PlayerPrefs.GetFloat($"LapTime_{i}");
                int minutes = Mathf.FloorToInt(lapTime / 60f);
                int seconds = Mathf.FloorToInt(lapTime % 60f);

                lapTimeInfo += $"LAP {i} : {minutes:D2}:{seconds:D2} minutes\n";
            }
        }

        // UI�� ǥ��
        lapTimeText.text = lapTimeInfo;
    }
}
