using System.Collections;
using UnityEngine;
using PacketHeaders;

/// <summary>
/// Debug инструмент для тестирования коллизий с сервером
/// Отправляет raycast запросы на сервер и визуализирует результат
/// </summary>
public class DebugRaycast : MonoBehaviour
{
    [Header("Настройки")]
    public bool enableDebug = true;
    public float raycastInterval = 0.3f;
    public float raycastDistance = 100f;

    [Header("Визуализация")]
    public GameObject debugMarkerPrefab;
    private GameObject currentMarker;

    [Header("Ссылки")]
    public Camera playerCamera;
    private Client client;

    private float nextRaycastTime = 0f;

    void Start()
    {
        // Находим клиента
        client = FindObjectOfType<Client>();

        // Находим камеру если не указана
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Создаём простой кубик-маркер если префаб не указан
        if (debugMarkerPrefab == null && currentMarker == null)
        {
            currentMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            currentMarker.name = "DebugRaycastMarker";
            currentMarker.transform.localScale = Vector3.one * 0.3f;

            // Делаем яркий красный материал
            Renderer rend = currentMarker.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = Color.red;
            }

            // Убираем коллайдер чтобы не мешал
            Collider col = currentMarker.GetComponent<Collider>();
            if (col != null)
            {
                Destroy(col);
            }

            currentMarker.SetActive(false);
        }
    }

    void Update()
    {
        if (!enableDebug || client == null || !client.socketReady)
            return;

        // Отправляем raycast с интервалом
        if (Time.time >= nextRaycastTime)
        {
            SendDebugRaycast();
            nextRaycastTime = Time.time + raycastInterval;
        }

        // Отключение debug режима по клавише
        if (Input.GetKeyDown(KeyCode.F3))
        {
            enableDebug = !enableDebug;
            if (currentMarker != null)
            {
                currentMarker.SetActive(enableDebug);
            }
            Debug.Log($"[DEBUG RAYCAST] {(enableDebug ? "Включен" : "Выключен")}");
        }
    }

    void SendDebugRaycast()
    {
        if (playerCamera == null)
            return;

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        // Отправляем пакет на сервер
        Packet packet = new Packet((int)WorldCommand.CMSG_DEBUG_RAYCAST);

        // Позиция начала луча
        packet.Write(origin.x);
        packet.Write(origin.y);
        packet.Write(origin.z);

        // Направление луча
        packet.Write(direction.x);
        packet.Write(direction.y);
        packet.Write(direction.z);

        // Максимальная дистанция
        packet.Write(raycastDistance);

        client.Send(packet);
    }

    public void OnRaycastResult(bool hit, Vector3 hitPoint)
    {
        if (currentMarker == null)
            return;

        if (hit)
        {
            // Перемещаем маркер в точку попадания
            currentMarker.transform.position = hitPoint;
            currentMarker.SetActive(true);

            // Вычисляем расстояние
            float distance = Vector3.Distance(playerCamera.transform.position, hitPoint);
            Debug.Log($"[DEBUG RAYCAST] Попадание! Dist: {distance:F2}m, Pos: {hitPoint}");
        }
        else
        {
            // Скрываем маркер если ничего не нашли
            currentMarker.SetActive(false);
            Debug.Log("[DEBUG RAYCAST] Нет попадания");
        }
    }

    void OnDestroy()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }
    }
}
