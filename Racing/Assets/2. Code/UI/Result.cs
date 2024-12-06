using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    public TextMeshProUGUI lapTimeText; // 랩 시간 UI

    private void Start()
    {
        string lapTimeInfo = "";
        int totalLaps = 4; // 총 랩 수와 동일하게 설정

        for (int i = 1; i <= totalLaps; i++) // 각 랩 시간 불러오기
        {
            if (PlayerPrefs.HasKey($"LapTime_{i}"))
            {
                float lapTime = PlayerPrefs.GetFloat($"LapTime_{i}");
                int minutes = Mathf.FloorToInt(lapTime / 60f);
                int seconds = Mathf.FloorToInt(lapTime % 60f);

                lapTimeInfo += $"LAP {i} : {minutes:D2}:{seconds:D2} minutes\n";
            }
        }

        // UI에 표시
        lapTimeText.text = lapTimeInfo;
    }
}
