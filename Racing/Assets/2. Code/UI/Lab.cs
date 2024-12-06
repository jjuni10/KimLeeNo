using UnityEngine;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public TextMeshProUGUI lapText;       // 현재 랩 UI
    public TextMeshProUGUI lapTimeText;   // 랩 시간 UI
    public GameObject checkpoint;         // 체크포인트 오브젝트
    public int totalLaps = 3;             // 총 Lap 수

    public int currentLap = 0;           // 현재 Lap 수
    private bool canCountLap = true;      // 중복 감지 방지 플래그
    private bool timerStarted = false;    // 타이머 시작 여부
    private float lapStartTime;           // 현재 랩 시작 시간
    private float[] lapTimes;             // 각 랩 시간 기록

    private void Start()
    {
        lapText = GameManager.Instance.lapText;
        lapTimeText = GameManager.Instance.lapTimeText;
        checkpoint = GameManager.Instance.checkPoint;
        
        lapTimes = new float[totalLaps-1];  // 총 랩 수만큼 배열 생성
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
            if (this.gameObject.tag == "Player")
            {
                UpdateLapText();
                UpdateLapTimeText();
            }

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
            lapText.text = "3 / 3"; // 랩이 끝났으면 "End"로 표시
        }
        else
        {
            lapText.text = currentLap + " / " + "3"; // 현재 랩과 총 랩 수 표시
        }
    }

    private void UpdateLapTimeText()
    {
        // 시간 출력은 분:초 형식으로 표시
        string lapTimeInfo = "";
        for (int i = 1; i < currentLap; i++) // i = 1부터 시작
        {
            // 초를 분과 초로 나누기
            int minutes = Mathf.FloorToInt(lapTimes[i - 1] / 60f);
            int seconds = Mathf.FloorToInt(lapTimes[i - 1] % 60f);

            lapTimeInfo += $"LAP {i} : {minutes:D2}:{seconds:D2} minutes\n"; // 랩 번호와 시간 표시
        }
        lapTimeText.text = lapTimeInfo; // 시간 정보를 UI에 표시
    }
}
