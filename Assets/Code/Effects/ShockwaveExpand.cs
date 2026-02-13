using UnityEngine;

/// <summary>
/// Расширяет объект ударной волны при взрыве с затуханием
/// </summary>
public class ShockwaveExpand : MonoBehaviour
{
    [Header("Настройки расширения")]
    public float expandSpeed = 20f;
    public float maxScale = 10f;

    private float currentScale = 0.1f;

    void Start()
    {
        transform.localScale = Vector3.one * currentScale;
    }

    void Update()
    {
        if(currentScale < maxScale)
        {
            currentScale += expandSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * currentScale;

            // Затухание альфа-канала
            Renderer rend = GetComponent<Renderer>();
            if(rend != null && rend.material != null)
            {
                Color col = rend.material.color;
                float alpha = Mathf.Lerp(0.3f, 0f, currentScale / maxScale);
                col.a = alpha;
                rend.material.color = col;
            }
        }
    }
}
