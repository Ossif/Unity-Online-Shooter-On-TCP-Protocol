using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMenuShake : MonoBehaviour
{
    public float shakeSpeed = 2.0f; // �������� ������� ������
    public float shakeAmount = 5.0f; // ������������ ���� ���������� ������
    public float smoothness = 0.1f; // ��������� �������� ������
    private Quaternion initialRotation; // ��������� ������� ������
    private Vector3 initialPosition; // ��������� ������� ������

    void Start()
    {
        initialRotation = transform.rotation; // ��������� ��������� ������� ������
        initialPosition = transform.position; // ��������� ��������� ������� ������
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount; // ��������� ���������� �� ��� X
        float y = Mathf.Sin(Time.time * shakeSpeed * 2) * shakeAmount; // ��������� ���������� �� ��� Y
        Quaternion targetRotation = initialRotation * Quaternion.Euler(x, y, 0); // ����� ������� ������
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothness * Time.deltaTime); // ������ ������� ������ � ����� �������
        transform.position = initialPosition + new Vector3(x / 50f, y / 50f, 0); // ��������� ��������� ��������, ����� ������� ������ �������
    }
}