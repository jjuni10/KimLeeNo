using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class LapCounter : MonoBehaviour
{
    public TextMeshProUGUI lapText;       // 현재 랩 UI
    public TextMeshProUGUI lapTimeText;   // 랩 시간 UI
    public GameObject checkpoint;         // 체크포인트 오브젝트
    public int totalLaps = 4;             // 총 Lap 수

    private int currentLap = 0;           // 현재 Lap 수
    private bool canCountLap = true;      // 중복 감지 방지 플래그
    private bool timerStarted = false;    // 타이머 시작 여부
    private float lapStartTime;           // 현재 랩 시작 시간
    private float[] lapTimes;             // 각 랩 시간 기록

    private void Start()
    {
        lapTimes = new float[totalLaps];  // 총 랩 수만큼 배열 생성
        lapStartTime = Time.time;         // 첫 랩 시작 시간 초기화
        UpdateLapText();
        UpdateLapTimeText();

        // 5초 후 타이머 시작
        Invoke(nameof(StartTimer), 5f);
    }

    private void StartTimer()
    {
        timerStarted = true;              // 타이머 활성화
        lapStartTime = Time.time;         // 타이머 시작 시간 설정
    }

    private void OnTriggerEnter(Collider other)
    {
        // 차량이 체크포인트를 통과하고 타이머가 활성화되었으며 중복 감지가 아니며, Lap 수가 한도를 넘지 않을 때
        if (timerStarted && other.gameObject == checkpoint && canCountLap && currentLap < totalLaps)
        {
            canCountLap = false; // 중복 감지 방지
            if (currentLap > 0)  // 두 번째 랩부터 시간 기록
            {
                float lapEndTime = Time.time; // 현재 시간 기록
                lapTimes[currentLap - 1] = lapEndTime - lapStartTime; // 걸린 시간 계산
                lapStartTime = lapEndTime; // 다음 랩 시작 시간 갱신
            }

            currentLap++; // Lap 수 증가
            UpdateLapText();
            UpdateLapTimeText();

            // 30초 동안 다시 감지되지 않도록 설정
            StartCoroutine(DisableCheckpointForOneMinute());
        }
    }

    private System.Collections.IEnumerator DisableCheckpointForOneMinute()
    {
        yield return new WaitForSeconds(30); // 30초 대기
        canCountLap = true; // 다시 감지 가능
    }

    private void UpdateLapText()
    {
        if (currentLap >= totalLaps)
        {
            lapText.text = $"{totalLaps} / {totalLaps}"; // 완료 표시
            LoadNextScene(); // 다음 씬 로드
        }
        else
        {
            lapText.text = currentLap + " / " + "3"; // 현재 랩과 총 랩 수 표시
        }
    }

    private void UpdateLapTimeText()
    {
        string lapTimeInfo = "";
        for (int i = 1; i < currentLap; i++) // i = 1부터 시작
        {
            int minutes = Mathf.FloorToInt(lapTimes[i - 1] / 60f);
            int seconds = Mathf.FloorToInt(lapTimes[i - 1] % 60f);

            // 랩 시간 정보를 UI에 표시
            lapTimeInfo += $"LAP {i} : {minutes:D2}:{seconds:D2} minutes\n";

            // 각 랩 시간을 PlayerPrefs에 저장
            PlayerPrefs.SetFloat($"LapTime_{i}", lapTimes[i - 1]);
        }
        lapTimeText.text = lapTimeInfo;

        // 저장 내용을 즉시 적용
        PlayerPrefs.Save();
    }
    private void LoadNextScene()
    {
        SceneManager.LoadScene("Result"); // 다음 씬 이름으로 변경
    }
}
