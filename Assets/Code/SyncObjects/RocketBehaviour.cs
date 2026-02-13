using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент управления снарядом RPG.
/// Получает обновления позиции от сервера и плавно интерполирует движение.
/// </summary>
public class RocketBehaviour : MonoBehaviour
{
    [Header("Идентификаторы")]
    public uint rocketId;
    public string ownerId;

    [Header("Интерполяция")]
    private Vector3 targetPosition;
    private Vector3 targetVelocity;
    private Vector3 currentVelocity;
    [SerializeField] private float smoothTime = 0.1f;

    [Header("Визуальные эффекты")]
    public ParticleSystem trailEffect;
    public Light rocketLight;

    private bool isInitialized = false;

    void Start()
    {
        // Инициализация эффектов
        if (trailEffect != null)
        {
            trailEffect.Play();
        }

        if (rocketLight != null)
        {
            rocketLight.enabled = true;
        }
    }

    /// <summary>
    /// Обновляет целевую позицию и скорость снаряда от сервера
    /// </summary>
    public void UpdateFromServer(Vector3 newPosition, Vector3 newVelocity)
    {
        if (!isInitialized)
        {
            // Первое обновление - телепортируем
            transform.position = newPosition;
            targetPosition = newPosition;
            targetVelocity = newVelocity;
            isInitialized = true;
        }
        else
        {
            // Последующие обновления - плавная интерполяция
            targetPosition = newPosition;
            targetVelocity = newVelocity;
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // Плавная интерполяция позиции
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );

        // Поворачиваем снаряд по направлению движения
        if (targetVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void OnDestroy()
    {
        // Останавливаем эффекты при уничтожении
        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }
}
