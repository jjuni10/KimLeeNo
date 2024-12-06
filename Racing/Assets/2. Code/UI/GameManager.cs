using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro ���
using UnityEngine.SceneManagement; // SceneManager �߰�

public class GameManager : MonoBehaviour
{
    public GameObject pausePanel;          // �Ͻ����� �г� ����
    public MonoBehaviour vehicleController; // ���� ���� ��ũ��Ʈ
    public Rigidbody vehicleRigidbody;     // ���� Rigidbody

    private AudioSource[] audioSources;    // ��� AudioSource ����
    private bool isPaused = false;         // ������ �Ͻ����� �������� ����

    private void Start()
    {
        // ���� ���� �� �Ͻ����� �г� ��Ȱ��ȭ
        pausePanel.SetActive(false);

        // ���� ���� ��ũ��Ʈ ��Ȱ��ȭ
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        // Rigidbody ���� ���� ����
        if (vehicleRigidbody != null)
        {
            vehicleRigidbody.isKinematic = true;
        }

        // ��� AudioSource ��������
        audioSources = FindObjectsOfType<AudioSource>();
    }

    private void Update()
    {
        // ESC Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(); // �Ͻ����� ���¸� ������Ŵ
        }
    }

    public void TogglePause()
    {
        if (isPaused) // �Ͻ����� ���¶��
        {
            ResumeGame(); // ���� �簳
        }
        else // �Ͻ������� �ƴϸ�
        {
            PauseGame(); // ���� �Ͻ�����
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // ���� �ð� ����
        pausePanel.SetActive(true); // �Ͻ����� �г� Ȱ��ȭ
        Cursor.lockState = CursorLockMode.None; // ���콺 Ŀ�� ����
        Cursor.visible = true; // ���콺 Ŀ�� ���̱�
        isPaused = true; // �Ͻ����� ���� ����

        // ��� ����� ����
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
        Time.timeScale = 1f; // ���� �ð� �簳
        pausePanel.SetActive(false); // �Ͻ����� �г� ��Ȱ��ȭ
        Cursor.lockState = CursorLockMode.Locked; // ���콺 Ŀ�� ���
        Cursor.visible = false; // ���콺 Ŀ�� �����
        isPaused = false; // �Ͻ����� ���� ����

        // ��� ����� �簳
        foreach (var source in audioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }
    }

    // Continue ��ư Ŭ�� �� ȣ��� �Լ�
    public void OnContinueButton()
    {
        ResumeGame(); // ������ �簳
    }

    // Restart ��ư Ŭ�� �� ȣ��� �Լ�
    public void OnRestartButton()
    {
        RestartGame(); // ���� �����
    }

    // ������ �ٽ� �����ϴ� �Լ�
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // ���� ���� �ٽ� �ε��Ͽ� ������ �ʱ�ȭ
        Time.timeScale = 1f; // �ð� �������� �������� �ǵ�����
        isPaused = false; // �Ͻ����� ���� ����
    }

    public void QuitGame()
    {
        // ���� ���� ��ư
        Application.Quit();
    }
}