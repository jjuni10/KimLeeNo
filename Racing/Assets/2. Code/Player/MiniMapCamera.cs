using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    public Transform target; // ���� �ڵ���
    public Vector3 offset = new Vector3(0, 10, 0); // �ڵ����� ī�޶� ���� ������
    public bool rotateWithCar = false; // �ڵ��� ȸ�� ���⿡ ���� ī�޶� ȸ�� ����

    private void LateUpdate()
    {
        if (target == null) return;

        // �ڵ����� ��ġ + ���������� ī�޶� ��ġ ����
        transform.position = target.position + offset;

        if (rotateWithCar)
        {
            // �ڵ����� ȸ���� ����
            transform.rotation = Quaternion.Euler(90, target.eulerAngles.y, 0);
        }
        else
        {
            // ������ ȸ���� ���� (�̴ϸʿ�)
            transform.rotation = Quaternion.Euler(90, 0, 0); // ������ �����ٺ��� ���� ����
        }
    }

    // �ܺο��� ȣ���� �ڵ����� �Ҵ��ϴ� �޼���
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
