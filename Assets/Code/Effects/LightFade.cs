using UnityEngine;

/// <summary>
/// Плавно гасит свет от взрыва
/// </summary>
public class LightFade : MonoBehaviour
{
    [Header("Настройки затухания")]
    public float fadeDuration = 0.5f;

    private Light lightComponent;
    private float initialIntensity;
    private float timer = 0f;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        if(lightComponent != null)
        {
            initialIntensity = lightComponent.intensity;
        }
    }

    void Update()
    {
        if(lightComponent != null && timer < fadeDuration)
        {
            timer += Time.deltaTime;
            lightComponent.intensity = Mathf.Lerp(
                initialIntensity,
                0f,
                timer / fadeDuration
            );
        }
    }
}
