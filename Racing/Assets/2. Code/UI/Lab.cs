using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class LapCounter : MonoBehaviour
{
    public TextMeshProUGUI lapText;       // ���� �� UI
    public TextMeshProUGUI lapTimeText;   // �� �ð� UI
    public GameObject checkpoint;         // üũ����Ʈ ������Ʈ
    public int totalLaps = 4;             // �� Lap ��

    private int currentLap = 0;           // ���� Lap ��
    private bool canCountLap = true;      // �ߺ� ���� ���� �÷���
    private bool timerStarted = false;    // Ÿ�̸� ���� ����
    private float lapStartTime;           // ���� �� ���� �ð�
    private float[] lapTimes;             // �� �� �ð� ���

    private void Start()
    {
        lapTimes = new float[totalLaps];  // �� �� ����ŭ �迭 ����
        lapStartTime = Time.time;         // ù �� ���� �ð� �ʱ�ȭ
        UpdateLapText();
        UpdateLapTimeText();

        // 5�� �� Ÿ�̸� ����
        Invoke(nameof(StartTimer), 5f);
    }

    private void StartTimer()
    {
        timerStarted = true;              // Ÿ�̸� Ȱ��ȭ
        lapStartTime = Time.time;         // Ÿ�̸� ���� �ð� ����
    }

    private void OnTriggerEnter(Collider other)
    {
        // ������ üũ����Ʈ�� ����ϰ� Ÿ�̸Ӱ� Ȱ��ȭ�Ǿ����� �ߺ� ������ �ƴϸ�, Lap ���� �ѵ��� ���� ���� ��
        if (timerStarted && other.gameObject == checkpoint && canCountLap && currentLap < totalLaps)
        {
            canCountLap = false; // �ߺ� ���� ����
            if (currentLap > 0)  // �� ��° ������ �ð� ���
            {
                float lapEndTime = Time.time; // ���� �ð� ���
                lapTimes[currentLap - 1] = lapEndTime - lapStartTime; // �ɸ� �ð� ���
                lapStartTime = lapEndTime; // ���� �� ���� �ð� ����
            }

            currentLap++; // Lap �� ����
            UpdateLapText();
            UpdateLapTimeText();

            // 30�� ���� �ٽ� �������� �ʵ��� ����
            StartCoroutine(DisableCheckpointForOneMinute());
        }
    }

    private System.Collections.IEnumerator DisableCheckpointForOneMinute()
    {
        yield return new WaitForSeconds(30); // 30�� ���
        canCountLap = true; // �ٽ� ���� ����
    }

    private void UpdateLapText()
    {
        if (currentLap >= totalLaps)
        {
            lapText.text = $"{totalLaps} / {totalLaps}"; // �Ϸ� ǥ��
            LoadNextScene(); // ���� �� �ε�
        }
        else
        {
            lapText.text = currentLap + " / " + "3"; // ���� ���� �� �� �� ǥ��
        }
    }

    private void UpdateLapTimeText()
    {
        string lapTimeInfo = "";
        for (int i = 1; i < currentLap; i++) // i = 1���� ����
        {
            int minutes = Mathf.FloorToInt(lapTimes[i - 1] / 60f);
            int seconds = Mathf.FloorToInt(lapTimes[i - 1] % 60f);

            // �� �ð� ������ UI�� ǥ��
            lapTimeInfo += $"LAP {i} : {minutes:D2}:{seconds:D2} minutes\n";

            // �� �� �ð��� PlayerPrefs�� ����
            PlayerPrefs.SetFloat($"LapTime_{i}", lapTimes[i - 1]);
        }
        lapTimeText.text = lapTimeInfo;

        // ���� ������ ��� ����
        PlayerPrefs.Save();
    }
    private void LoadNextScene()
    {
        SceneManager.LoadScene("Result"); // ���� �� �̸����� ����
    }
}
