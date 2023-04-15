using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMenuShake : MonoBehaviour
{
    public float shakeSpeed = 2.0f; // скорость качания камеры
    public float shakeAmount = 5.0f; // максимальный угол отклонения камеры
    public float smoothness = 0.1f; // плавность движения камеры
    private Quaternion initialRotation; // начальная ротация камеры
    private Vector3 initialPosition; // начальная позиция камеры

    void Start()
    {
        initialRotation = transform.rotation; // сохраняем начальную ротацию камеры
        initialPosition = transform.position; // сохраняем начальную позицию камеры
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * shakeSpeed) * shakeAmount; // вычисляем отклонение по оси X
        float y = Mathf.Sin(Time.time * shakeSpeed * 2) * shakeAmount; // вычисляем отклонение по оси Y
        Quaternion targetRotation = initialRotation * Quaternion.Euler(x, y, 0); // новая ротация камеры
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothness * Time.deltaTime); // плавно двигаем камеру к новой ротации
        transform.position = initialPosition + new Vector3(x / 50f, y / 50f, 0); // добавляем небольшое смещение, чтобы создать эффект качания
    }
}