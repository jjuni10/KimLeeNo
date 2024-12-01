using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 사용

public class CountdownManager : MonoBehaviour
{
    public TextMeshProUGUI countdownText; // TMP 텍스트 연결
    public MonoBehaviour vehicleController; // 차량 제어 스크립트
    public Rigidbody vehicleRigidbody;     // 차량 Rigidbody
    public int countdownTime = 5;          // 카운트다운 시간

    private void Start()
    {
        // 차량의 제어 스크립트를 비활성화
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        // Rigidbody의 물리 동작을 멈춤
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        // 카운트다운 시작
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        int timeRemaining = countdownTime;

        // 카운트다운 루프
        while (timeRemaining > 0)
        {
            countdownText.text = timeRemaining.ToString(); // 텍스트 갱신
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        // 마지막 텍스트 "Go!" 표시 및 즉시 출발 준비
        countdownText.text = "Go!";

        // 차량 제어 스크립트 활성화
        if (vehicleController != null)
        {
            vehicleController.enabled = true;
        }

        // Rigidbody 물리 동작 재개
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = false;
        }

        // "Go!" 텍스트를 잠시 보여준 후 제거
        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);
    }
}