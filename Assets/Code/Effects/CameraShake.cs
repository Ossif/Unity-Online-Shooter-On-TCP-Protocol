using System.Collections;
using UnityEngine;

/// <summary>
/// Компонент для тряски камеры при взрывах и других событиях
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;

    [Header("Настройки тряски")]
    [Tooltip("Множитель силы тряски")]
    public float shakeMultiplier = 1.0f;

    [Tooltip("Минимальная интенсивность для запуска тряски")]
    public float minimumIntensity = 0.01f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isShaking = false;

    void Awake()
    {
        // Синглтон для глобального доступа
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Несколько экземпляров CameraShake! Используется первый.");
        }
    }

    void Start()
    {
        // Сохраняем изначальную позицию и поворот камеры с небольшой задержкой
        StartCoroutine(InitializeWithDelay());
    }

    /// <summary>
    /// Инициализация с задержкой для гарантии что камера готова
    /// </summary>
    private IEnumerator InitializeWithDelay()
    {
        // Ждём один кадр чтобы все трансформации были готовы
        yield return null;
        UpdateOriginalTransform();
    }

    /// <summary>
    /// Безопасное обновление исходной трансформации
    /// </summary>
    private void UpdateOriginalTransform()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        // Проверка на NaN с логированием для диагностики
        if (float.IsNaN(originalPosition.x) || float.IsNaN(originalPosition.y) || float.IsNaN(originalPosition.z))
        {
            Debug.LogError($"CameraShake [{gameObject.name}]: transform.localPosition содержит NaN! " +
                          $"Position={transform.localPosition}, Parent={transform.parent?.name ?? "null"}");
            originalPosition = Vector3.zero;
            transform.localPosition = Vector3.zero;
        }

        if (float.IsNaN(originalRotation.x) || float.IsNaN(originalRotation.y) ||
            float.IsNaN(originalRotation.z) || float.IsNaN(originalRotation.w))
        {
            Debug.LogError($"CameraShake [{gameObject.name}]: transform.localRotation содержит NaN! " +
                          $"Rotation={transform.localRotation}, Parent={transform.parent?.name ?? "null"}");
            originalRotation = Quaternion.identity;
            transform.localRotation = Quaternion.identity;
        }

        // Дополнительное логирование для отладки
        Debug.Log($"CameraShake [{gameObject.name}]: Инициализирован. Pos={originalPosition}, Rot={originalRotation.eulerAngles}");
    }

    /// <summary>
    /// Статический метод для запуска тряски камеры из любого места
    /// </summary>
    /// <param name="intensity">Сила тряски (0-1)</param>
    /// <param name="duration">Длительность тряски в секундах</param>
    public static void Shake(float intensity, float duration)
    {
        if (instance != null)
        {
            instance.StartShake(intensity, duration);
        }
        else
        {
            Debug.LogWarning("CameraShake instance не найден! Добавьте компонент CameraShake на камеру.");
        }
    }

    /// <summary>
    /// Запускает тряску камеры (внутренний метод)
    /// </summary>
    private void StartShake(float intensity, float duration)
    {
        // Проверяем минимальную интенсивность
        if (intensity < minimumIntensity)
        {
            return;
        }

        // Проверка на NaN в параметрах
        if (float.IsNaN(intensity) || float.IsNaN(duration))
        {
            Debug.LogWarning("CameraShake: параметры содержат NaN!");
            return;
        }

        // Применяем множитель
        intensity *= shakeMultiplier;

        // Обновляем исходное положение перед тряской
        if (!isShaking)
        {
            UpdateOriginalTransform();
        }

        // Если уже трясём - запускаем новую корутину (более сильная тряска прерывает слабую)
        if (isShaking)
        {
            StopAllCoroutines();
        }

        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    /// <summary>
    /// Корутина тряски камеры
    /// </summary>
    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        float elapsed = 0f;

        // Максимальное смещение позиции и поворота
        float maxPositionOffset = intensity * 0.15f; // До 15 см при intensity=1.0
        float maxRotationOffset = intensity * 2.0f;   // До 2 градусов при intensity=1.0

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Вычисляем затухание (сильнее в начале, слабее к концу)
            float damper = 1.0f - (elapsed / duration);
            damper = Mathf.Pow(damper, 0.5f); // Квадратный корень для более плавного затухания

            // Генерируем случайное смещение с использованием Perlin Noise для плавности
            float offsetX = (Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f) * 2f;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f) * 2f;
            float offsetZ = (Mathf.PerlinNoise(Time.time * 25f, Time.time * 25f) - 0.5f) * 2f;

            // Вычисляем новую позицию
            Vector3 newPosition = originalPosition + new Vector3(
                offsetX * maxPositionOffset * damper,
                offsetY * maxPositionOffset * damper,
                offsetZ * maxPositionOffset * damper * 0.5f // Меньше смещения по Z
            );

            // Проверка на NaN перед применением позиции
            if (!float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z))
            {
                transform.localPosition = newPosition;
            }

            // Генерируем случайное вращение
            float rotX = (Mathf.PerlinNoise(Time.time * 20f, 10f) - 0.5f) * 2f;
            float rotY = (Mathf.PerlinNoise(10f, Time.time * 20f) - 0.5f) * 2f;
            float rotZ = (Mathf.PerlinNoise(Time.time * 20f, Time.time * 20f + 10f) - 0.5f) * 2f;

            // Вычисляем новое вращение
            Vector3 rotationEuler = new Vector3(
                rotX * maxRotationOffset * damper,
                rotY * maxRotationOffset * damper,
                rotZ * maxRotationOffset * damper * 0.3f // Меньше крена по Z
            );

            // Проверка на NaN перед применением вращения
            if (!float.IsNaN(rotationEuler.x) && !float.IsNaN(rotationEuler.y) && !float.IsNaN(rotationEuler.z))
            {
                Quaternion deltaRotation = Quaternion.Euler(rotationEuler);
                if (!float.IsNaN(deltaRotation.x) && !float.IsNaN(deltaRotation.y) &&
                    !float.IsNaN(deltaRotation.z) && !float.IsNaN(deltaRotation.w))
                {
                    transform.localRotation = originalRotation * deltaRotation;
                }
            }

            yield return null;
        }

        // Возвращаем камеру в исходное положение
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isShaking = false;
    }

    /// <summary>
    /// Обновляет исходную позицию камеры (полезно если камера двигается)
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            UpdateOriginalTransform();
        }
    }

    /// <summary>
    /// Немедленно останавливает тряску
    /// </summary>
    public static void StopShake()
    {
        if (instance != null)
        {
            instance.StopAllCoroutines();
            instance.transform.localPosition = instance.originalPosition;
            instance.transform.localRotation = instance.originalRotation;
            instance.isShaking = false;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
