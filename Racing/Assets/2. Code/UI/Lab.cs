using UnityEngine;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public TextMeshProUGUI lapText; // TMP 텍스트 연결
    public GameObject checkpoint;  // 체크포인트 오브젝트
    public int totalLaps = 3;      // 총 Lap 수

    private int currentLap = 0;    // 현재 Lap 수
    private bool canCountLap = true; // 중복 감지 방지 플래그

    private void Start()
    {
        UpdateLapText();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 차량이 체크포인트를 통과하고 중복 감지가 아닐 때
        if (other.gameObject.CompareTag("Checkpoint") && canCountLap)
        {
            canCountLap = false; // 중복 감지 방지
            currentLap++;
            UpdateLapText();

            // 레이스 종료 조건
            if (currentLap > totalLaps)
            {
                Debug.Log("Finish!");
                lapText.text = "Finish!";
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 차량이 체크포인트를 벗어나면 다시 감지 가능하도록 설정
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            canCountLap = true;
        }
    }

    private void UpdateLapText()
    {
        lapText.text = "Lap " + currentLap + " / " + totalLaps;
    }
}