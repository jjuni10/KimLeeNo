using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro 사용
using UnityEngine.SceneManagement; // SceneManager 추가

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;          // 일시정지 패널 연결
    public MonoBehaviour vehicleController; // 차량 제어 스크립트
    public Rigidbody vehicleRigidbody;     // 차량 Rigidbody

    private AudioSource[] audioSources;    // 모든 AudioSource 저장
    private bool isPaused = false;         // 게임이 일시정지 상태인지 추적

    private void Start()
    {
        // 게임 시작 시 일시정지 패널 비활성화
        pausePanel.SetActive(false);

        // 차량 제어 스크립트 비활성화
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        // Rigidbody 물리 동작 정지
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        // 모든 AudioSource 가져오기
        audioSources = FindObjectsOfType<AudioSource>();
    }

    private void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(); // 일시정지 상태를 반전시킴
        }
    }

    public void TogglePause()
    {
        if (isPaused) // 일시정지 상태라면
        {
            ResumeGame(); // 게임 재개
        }
        else // 일시정지가 아니면
        {
            PauseGame(); // 게임 일시정지
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // 게임 시간 멈춤
        pausePanel.SetActive(true); // 일시정지 패널 활성화
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 해제
        Cursor.visible = true; // 마우스 커서 보이기
        isPaused = true; // 일시정지 상태 설정

        // 모든 오디오 정지
        foreach (var source in audioSources)
        {
            if (source.isPlaying)
            {
                source.Pause();
            }
        }
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // 게임 시간 재개
        pausePanel.SetActive(false); // 일시정지 패널 비활성화
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 잠금
        Cursor.visible = false; // 마우스 커서 숨기기
        isPaused = false; // 일시정지 상태 해제

        // 모든 오디오 재개
        foreach (var source in audioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }

    // Continue 버튼 클릭 시 호출될 함수
    public void OnContinueButton()
    {
        ResumeGame(); // 게임을 재개
    }

    // Restart 버튼 클릭 시 호출될 함수
    public void OnRestartButton()
    {
        RestartGame(); // 게임 재시작
    }

    // 게임을 다시 시작하는 함수
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 씬을 다시 로드하여 게임을 초기화
        Time.timeScale = 1f; // 시간 스케일을 정상으로 되돌리기
        isPaused = false; // 일시정지 상태 해제
    }

    public void QuitGame()
    {
        // 게임 종료 버튼
        Application.Quit();
    }
}