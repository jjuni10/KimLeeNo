using System.Collections;
using UnityEngine;
using TMPro; // TextMeshPro ���
using UnityEngine.SceneManagement; // SceneManager �߰�
using System.Reflection.Emit;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    [Header("Object")]
    public CameraControl cameraControl;
    public PlayerSpeedDisplay playerSpeedDisplay;
    public CountdownManager countdownManager;
    public WaypointsManager waypointsManager;
    public Image reverseImage;
    public TextMeshProUGUI lapText;
    public TextMeshProUGUI lapTimeText;

    [Header ("NMS")]
    public GameObject pausePanel;          // �Ͻ����� �г� ����
    public MonoBehaviour vehicleController; // ���� ���� ��ũ��Ʈ
    public Rigidbody vehicleRigidbody;     // ���� Rigidbody

    private bool isPaused = false;         // ������ �Ͻ����� �������� ����

    public static GameManager Instance
    {
        get { return _instance; }
        set { }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            _instance = this;
        }
    }

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
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // ���� �ð� �簳
        pausePanel.SetActive(false); // �Ͻ����� �г� ��Ȱ��ȭ
        Cursor.lockState = CursorLockMode.Locked; // ���콺 Ŀ�� ���
        Cursor.visible = false; // ���콺 Ŀ�� �����
        isPaused = false; // �Ͻ����� ���� ����
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