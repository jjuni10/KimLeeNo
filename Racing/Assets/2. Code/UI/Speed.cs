using UnityEngine;
using TMPro; // TextMeshPro ���ӽ����̽� �߰�

public class PlayerSpeedDisplay : MonoBehaviour
{
    public Rigidbody playerRigidbody; // Player�� Rigidbody
    public TMP_Text speedTextTMP; // TMP�� �ӵ��� ǥ���� �ؽ�Ʈ

    void Update()
    {
        // Rigidbody�� �ӵ��� ������ ũ�⸦ ���
        float speed = playerRigidbody.velocity.magnitude;

        // �ӵ��� �� ��� ���
        float doubledSpeed = speed * 4;

        // �ӵ��� int�� ��ȯ (�ݿø�)
        int intSpeed = Mathf.RoundToInt(doubledSpeed);

        // TMP �ؽ�Ʈ�� �ӵ��� ������Ʈ
        speedTextTMP.text = intSpeed.ToString(); // �� �� �ӵ��� int�� ǥ��
    }
}