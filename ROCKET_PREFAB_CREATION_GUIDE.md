# Инструкция по созданию префабов для RPG

## 1. Создание префаба RocketHead

### Шаг 1: Открыть Unity проект
1. Откройте проект `UnityServerGameRep` в Unity Editor
2. Перейдите в сцену Game (Assets/Scenes/Game.unity)

### Шаг 2: Создать GameObject для снаряда
1. Hierarchy → ПКМ → Create Empty → Назвать "RocketHead"
2. В Inspector установить Position (0, 0, 0)

### Шаг 3: Добавить 3D модель
1. Найти в Project: `Assets/Models/WeaponObjects/RGP_Head.blend`
2. Перетащить модель как дочерний объект в RocketHead
3. Настроить масштаб модели: Scale (0.1, 0.1, 0.1) или по необходимости

### Шаг 4: Добавить компонент RocketBehaviour
1. Выбрать RocketHead в Hierarchy
2. Inspector → Add Component → Scripts → RocketBehaviour
3. Проверить, что скрипт прикрепился

### Шаг 5: Добавить Trail Effect (дымовой след)
1. RocketHead → ПКМ → Effects → Particle System → Назвать "Trail"
2. Настроить Particle System:
   - **Duration**: 5.0
   - **Looping**: ✓
   - **Start Lifetime**: 2-3
   - **Start Speed**: 0
   - **Start Size**: 0.5-1.0
   - **Start Color**: Серый с альфа-каналом (градиент)
   - **Emission → Rate over Time**: 30
   - **Shape**: Cone, Angle: 5, Radius: 0.1
   - **Color over Lifetime**: Градиент от белого к прозрачному серому
   - **Size over Lifetime**: Увеличение от 0.5 до 1.5
3. Переместить Trail назад по оси Z: Position (0, 0, -0.5)

### Шаг 6: Добавить Point Light
1. RocketHead → ПКМ → Light → Point Light → Назвать "RocketLight"
2. Настроить Light:
   - **Color**: Оранжевый/Желтый (#FF8800)
   - **Range**: 5
   - **Intensity**: 2
   - **Render Mode**: Important

### Шаг 7: Связать компоненты с RocketBehaviour
1. Выбрать RocketHead
2. В Inspector найти RocketBehaviour
3. Перетащить Trail (ParticleSystem) в поле "Trail Effect"
4. Перетащить RocketLight в поле "Rocket Light"

### Шаг 8: Создать префаб
1. Перетащить RocketHead из Hierarchy в папку `Assets/Prefabs/`
2. Сохранить как "RocketHead.prefab"
3. Удалить RocketHead из сцены (Delete)

---

## 2. Создание префаба ExplosionEffect

### Шаг 1: Создать пустой GameObject
1. Hierarchy → ПКМ → Create Empty → Назвать "ExplosionEffect"

### Шаг 2: Добавить Fireball (огненный шар)
1. ExplosionEffect → ПКМ → Effects → Particle System → Назвать "Fireball"
2. Настроить:
   - **Duration**: 0.5
   - **Looping**: ✗
   - **Start Lifetime**: 0.3-0.5
   - **Start Speed**: 5-10
   - **Start Size**: 1-2
   - **Start Color**: Градиент от желтого к красному
   - **Emission → Bursts**: Add Burst (Time: 0, Count: 50)
   - **Shape**: Sphere, Radius: 0.1
   - **Color over Lifetime**: Желтый → Оранжевый → Прозрачный
   - **Size over Lifetime**: Увеличение от 1 до 3

### Шаг 3: Добавить Smoke (дым)
1. ExplosionEffect → ПКМ → Effects → Particle System → Назвать "Smoke"
2. Настроить:
   - **Duration**: 2.0
   - **Looping**: ✗
   - **Start Lifetime**: 1-2
   - **Start Speed**: 2-5
   - **Start Size**: 2-4
   - **Start Color**: Темно-серый
   - **Emission → Bursts**: Add Burst (Time: 0, Count: 30)
   - **Shape**: Sphere, Radius: 0.5
   - **Color over Lifetime**: Серый → Прозрачный
   - **Size over Lifetime**: Увеличение от 2 до 6

### Шаг 4: Добавить Shockwave (ударная волна)
1. ExplosionEffect → ПКМ → 3D Object → Sphere → Назвать "Shockwave"
2. Удалить Sphere Collider
3. Создать Material: `Assets/Materials/ShockwaveMaterial.mat`
   - Shader: Unlit/Transparent
   - Color: Белый с низкой альфой (0.3)
4. Применить материал к Shockwave
5. Добавить скрипт ShockwaveExpand.cs (создать ниже)

### Шаг 5: Добавить Point Light
1. ExplosionEffect → ПКМ → Light → Point Light → Назвать "ExplosionLight"
2. Настроить:
   - **Color**: Оранжевый
   - **Range**: 15
   - **Intensity**: 8
3. Добавить скрипт LightFade.cs (создать ниже)

### Шаг 6: Создать префаб
1. Перетащить ExplosionEffect в `Assets/Prefabs/`
2. Сохранить как "ExplosionEffect.prefab"
3. Удалить из сцены

---

## 3. Создать вспомогательные скрипты

### ShockwaveExpand.cs
Создать: `Assets/Code/Effects/ShockwaveExpand.cs`

```csharp
using UnityEngine;

public class ShockwaveExpand : MonoBehaviour
{
    public float expandSpeed = 20f;
    public float maxScale = 10f;
    
    void Start()
    {
        transform.localScale = Vector3.one * 0.1f;
    }
    
    void Update()
    {
        if(transform.localScale.x < maxScale)
        {
            float scale = transform.localScale.x + expandSpeed * Time.deltaTime;
            transform.localScale = Vector3.one * scale;
            
            // Затухание альфа-канала
            Renderer rend = GetComponent<Renderer>();
            if(rend != null && rend.material != null)
            {
                Color col = rend.material.color;
                col.a = Mathf.Lerp(0.3f, 0f, transform.localScale.x / maxScale);
                rend.material.color = col;
            }
        }
    }
}
```

### LightFade.cs
Создать: `Assets/Code/Effects/LightFade.cs`

```csharp
using UnityEngine;

public class LightFade : MonoBehaviour
{
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
        if(lightComponent != null)
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
```

---

## 4. Назначить префабы в Client

1. Открыть сцену Game
2. Найти GameObject "Client" в Hierarchy
3. В Inspector найти компонент Client (Script)
4. Перетащить префабы:
   - **Rocket Prefab** → RocketHead.prefab
   - **Explosion Effect Prefab** → ExplosionEffect.prefab
5. Для звука взрыва:
   - Найти/скачать звук взрыва (RPG_Explosion.wav)
   - Поместить в `Assets/Audio/Weapons/RPG/`
   - Перетащить в поле **Explosion Sound**

---

## 5. Проверка

### Checklist:
- [ ] RocketHead.prefab существует в Assets/Prefabs/
- [ ] RocketHead содержит компонент RocketBehaviour
- [ ] RocketHead содержит дочерний TrailEffect (ParticleSystem)
- [ ] RocketHead содержит дочерний RocketLight (Light)
- [ ] ExplosionEffect.prefab существует в Assets/Prefabs/
- [ ] ExplosionEffect содержит Fireball, Smoke, Shockwave, ExplosionLight
- [ ] Client имеет назначенные префабы
- [ ] Проект компилируется без ошибок

---

## Альтернатива: Временные заглушки для быстрого тестирования

Если нет времени создавать полноценные эффекты:

### Простой RocketHead
1. Create Empty → RocketHead
2. Add Component → Mesh Filter → Mesh: Cylinder
3. Add Component → Mesh Renderer → Material: Default
4. Add Component → RocketBehaviour
5. Сохранить как префаб

### Простой ExplosionEffect
1. Create Empty → ExplosionEffect
2. Add → Particle System
3. Настроить: Burst, Duration 1s, Start Color: Orange
4. Сохранить как префаб

Этого достаточно для тестирования логики синхронизации!
