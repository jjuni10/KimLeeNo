using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class PlayerSpeedDisplay : MonoBehaviour
{
    public Rigidbody playerRigidbody; // Player의 Rigidbody
    public TMP_Text speedTextTMP; // TMP로 속도를 표시할 텍스트

    void Update()
    {
        // Rigidbody의 속도를 가져와 크기를 계산
        float speed = playerRigidbody.velocity.magnitude;

        // 속도를 두 배로 계산
        float doubledSpeed = speed * 4;

        // 속도를 int로 변환 (반올림)
        int intSpeed = Mathf.RoundToInt(doubledSpeed);

        // TMP 텍스트에 속도를 업데이트
        speedTextTMP.text = intSpeed.ToString(); // 두 배 속도를 int로 표시
    }
}