# 🚀 Детальный план доработки RPG (Гранатомёт)

## 📊 Анализ замысла автора

### Что уже сделано:

#### **Клиент (Unity):**
1. ✅ Добавлен enum `GRENADE_LAUNCHER = 5` в WeaponEnum.cs
2. ✅ Создана 3D модель Bazooka (bazooka.blend)
3. ✅ Создан префаб BazookaModel с иерархией:
   - `BazookaModel` (корень)
     - `model` (меш bazooки + Animator)
       - `Bone` (точка крепления)
         - `Body` (тело оружия)
           - `MuzzleFlash` (эффект выстрела)
4. ✅ Созданы анимации RPG:
   - `idle.anim` - анимация покоя
   - `shot.anim` - анимация выстрела
   - `model.controller` - аниматор с триггером "shot"
5. ✅ Создана 3D модель снаряда (RGP_Head.blend)
6. ✅ Добавлен слот [3] для гранатомёта в WeaponSystem
7. ✅ Реализована логика отправки CMSG_PLAYER_WEAPON_SHOT с weaponId
8. ⚠️ Параметры скопированы с Sawned-Off (временно)

#### **Сервер (Python):**
1. ✅ Добавлен enum `GRENADE_LAUNCHER = 5` в WeaponId
2. ✅ Создана система SyncObjectSystem с классом RocketHeadObject
3. ✅ Реализована физика снаряда:
   - Гравитация (-9.81)
   - Начальная скорость (30.0)
   - Время жизни (5 секунд)
4. ✅ Реализована проверка коллизий с картой (ray-triangle intersection)
5. ✅ Реализован взрыв с радиусом урона (5 метров, 50 HP)
6. ✅ Добавлена обработка CMSG_PLAYER_WEAPON_SHOT в Features/Weapon.py
7. ✅ Добавлены пакеты синхронизации:
   - SMSG_CREATE_SYNC_OBJECT (42)
   - SMSG_SYNC_OBJECT_UPDATE (43)
   - SMSG_DESTROY_SYNC_OBJECT (44)

### Что НЕ сделано:

#### **Критические проблемы:**
1. ❌ **Клиент не обрабатывает пакеты синхронизации (42-44)**
   - Нет визуализации снаряда
   - Нет обновления траектории
   - Нет эффектов взрыва

2. ❌ **Нет префаба снаряда на клиенте**
   - Модель RGP_Head.blend не интегрирована в префаб
   - Нет компонента Rigidbody/визуализации

3. ❌ **Неправильные параметры оружия**
   - Копипаста параметров обреза
   - Неправильное название ("Sawned-Off")

4. ❌ **Отсутствуют звуки и анимации**
   - Используются звуки обреза
   - Нет уникальных аудио для RPG

5. ❌ **Нет эффектов взрыва**
   - Particle System для взрыва
   - Звук взрыва
   - Camera shake

---

## 🎯 Замысел автора (реконструкция)

### **Концепция оружия:**
- **Тип:** Снарядное оружие с баллистической физикой
- **Механика:** 
  - Клиент отправляет запрос на выстрел
  - Сервер создаёт снаряд и симулирует физику
  - Клиенты получают синхронизацию позиции снаряда
  - При столкновении/таймауте происходит взрыв
  - Урон наносится в радиусе со спадом

### **Архитектурное решение:**
- **Server-authoritative** - сервер контролирует всю логику снаряда
- **Client prediction отсутствует** - клиент только визуализирует
- **Причина:** Предотвращение читерства, единая физика для всех

### **Отличия от hitscan оружия:**
| Hitscan (AK, Pistol) | Projectile (RPG) |
|----------------------|------------------|
| Мгновенное попадание | Полёт снаряда |
| Клиент рассчитывает raycast | Сервер симулирует физику |
| Урон по одной цели | Урон по площади (AoE) |
| Трейл-эффект на клиенте | Полноценный GameObject |

---

## 📋 План доработки (поэтапный)

### **Этап 1: Критические исправления на клиенте** ⚡

#### **1.1 Добавить пакеты в PacketHeaders.cs**
```csharp
// В enum WorldCommand добавить:
SMSG_CREATE_SYNC_OBJECT = 42,
SMSG_SYNC_OBJECT_UPDATE = 43,
SMSG_DESTROY_SYNC_OBJECT = 44
```

#### **1.2 Создать префаб снаряда RocketHead**
**Структура:**
```
RocketHead.prefab
├── Model (MeshFilter + MeshRenderer)
│   └── RGP_Head.blend меш
├── Trail (ParticleSystem) - дымовой след
└── Light (точечный свет для подсветки)
```

**Компоненты:**
- `RocketBehaviour.cs` - скрипт управления снарядом
- НЕТ Rigidbody (позиция управляется сервером)
- НЕТ Collider (коллизии на сервере)

#### **1.3 Создать скрипт RocketBehaviour.cs**
```csharp
public class RocketBehaviour : MonoBehaviour {
    public uint rocketId;
    public string ownerId;
    
    // Интерполяция позиции для плавности
    private Vector3 targetPosition;
    private Vector3 targetVelocity;
    private float smoothTime = 0.1f;
    
    public void UpdatePosition(Vector3 newPos, Vector3 newVel) {
        targetPosition = newPos;
        targetVelocity = newVel;
    }
    
    void Update() {
        // Плавная интерполяция
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime
        );
        
        // Поворот по направлению движения
        if(targetVelocity.magnitude > 0.1f) {
            transform.rotation = Quaternion.LookRotation(targetVelocity);
        }
    }
}
```

#### **1.4 Добавить обработку пакетов в Client.cs**

**В switch (WorldCommand) добавить:**

```csharp
case WorldCommand.SMSG_CREATE_SYNC_OBJECT: {
    uint objectId = InComePacket.ReadUInt32();
    string objectType = InComePacket.ReadString();
    
    Vector3 position = new Vector3(
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat()
    );
    
    Vector3 velocity = new Vector3(
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat()
    );
    
    if(objectType == "rocket") {
        GameObject rocket = Instantiate(
            RocketPrefab, 
            position, 
            Quaternion.identity
        );
        
        RocketBehaviour rb = rocket.GetComponent<RocketBehaviour>();
        rb.rocketId = objectId;
        rb.UpdatePosition(position, velocity);
        
        syncObjects.Add(objectId, rocket);
    }
    break;
}

case WorldCommand.SMSG_SYNC_OBJECT_UPDATE: {
    uint objectId = InComePacket.ReadUInt32();
    
    Vector3 position = new Vector3(
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat()
    );
    
    Vector3 velocity = new Vector3(
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat(),
        InComePacket.ReadFloat()
    );
    
    if(syncObjects.ContainsKey(objectId)) {
        GameObject obj = syncObjects[objectId];
        obj.GetComponent<RocketBehaviour>().UpdatePosition(position, velocity);
    }
    break;
}

case WorldCommand.SMSG_DESTROY_SYNC_OBJECT: {
    uint objectId = InComePacket.ReadUInt32();
    
    if(syncObjects.ContainsKey(objectId)) {
        GameObject obj = syncObjects[objectId];
        
        // Проигрываем эффект взрыва
        Vector3 explosionPos = obj.transform.position;
        Instantiate(ExplosionEffectPrefab, explosionPos, Quaternion.identity);
        
        // Звук взрыва
        AudioSource.PlayClipAtPoint(ExplosionSound, explosionPos);
        
        // Удаляем снаряд
        Destroy(obj);
        syncObjects.Remove(objectId);
    }
    break;
}
```

#### **1.5 Добавить поле в Client.cs**
```csharp
public GameObject RocketPrefab;
public GameObject ExplosionEffectPrefab;
public AudioClip ExplosionSound;
private Dictionary<uint, GameObject> syncObjects = new Dictionary<uint, GameObject>();
```

---

### **Этап 2: Исправление параметров оружия** 🔧

#### **2.1 Исправить WeaponEnum.cs**
```csharp
// Строка 47 - изменить параметры:
weaponList.Add(new Weapon(
    WeaponId.GRENADE_LAUNCHER, 
    GrenadeLauncherObject, 
    "Grenade Launcher",  // ❌ было "Sawned-Off"
    1,                   // ❌ было 2 - патронов в обойме
    3,                   // ❌ было 30 - общий запас
    100.0f,              // ❌ было 40.0f - урон (не используется, на сервере)
    false,               // автоматический режим
    1.5f,                // ❌ было 0.5f - задержка между выстрелами
    "RPG_take",          // ❌ было "SO_take" - анимация доставания
    "RPG_shot",          // ❌ было "SO_shot" - анимация выстрела
    "RPG_reload",        // ❌ было "SO_reload" - анимация перезарядки
    "RPG_walk"           // ❌ было "SO_walk" - анимация ходьбы с оружием
));
```

#### **2.2 Создать анимации для рук**
**Файлы для создания:**
- `H_RPG_take.anim` - анимация доставания RPG для рук
- `H_RPG_shot.anim` - анимация отдачи рук при выстреле
- `H_RPG_reload.anim` - анимация перезарядки для рук
- `H_RPG_walk.anim` - анимация ходьбы с RPG в руках

**Где:** `Assets/Models/FPSAnim/FBX/animations/hands/`

#### **2.3 Обновить WeaponSystem.cs**
```csharp
// В методе Start() изменить:
weaponSlots[3] = WeaponId.GRENADE_LAUNCHER;
slotAmmo[3] = 1;      // ❌ было 1 - стартовые патроны
maxAmmo[3] = 3;       // ❌ было 3 - стартовый запас

// В методе ChangeWeapon() добавить:
case WeaponId.GRENADE_LAUNCHER: { 
    AS.PlayOneShot(RPGTakeClip);  // Новый звук
    break;
}

// В методе Update() (секция reload) добавить:
case WeaponId.GRENADE_LAUNCHER: { 
    AS.PlayOneShot(RPGReloadClip);  // Новый звук
    break;
}
```

#### **2.4 Обновить Shoot.cs**
```csharp
// В switch звуков выстрела добавить:
case WeaponId.GRENADE_LAUNCHER: {
    ws.AS.PlayOneShot(ws.RPGShotClip);
    break;
}
```

---

### **Этап 3: Аудио и визуальные эффекты** 🎨

#### **3.1 Добавить звуки**
**Требуемые аудиофайлы:**
- `RPG_Take.wav` - звук доставания RPG
- `RPG_Shot.wav` - звук выстрела (свист)
- `RPG_Reload.wav` - звук перезарядки
- `RPG_Explosion.wav` - звук взрыва

**Где разместить:** `Assets/Audio/Weapons/RPG/`

#### **3.2 Создать эффект взрыва**
**Префаб ExplosionEffect.prefab:**
```
ExplosionEffect
├── FireBall (ParticleSystem) - огненный шар
├── Smoke (ParticleSystem) - дым
├── Sparks (ParticleSystem) - искры
├── Light (убывающий свет)
└── Shockwave (расширяющийся меш-эффект)
```

**Параметры:**
- Время жизни: 2 секунды
- Auto-destroy после проигрывания
- Prefab должен быть в Resources для Instantiate

#### **3.3 Добавить Camera Shake**
**Создать скрипт CameraShake.cs:**
```csharp
public class CameraShake : MonoBehaviour {
    public static void Shake(float intensity, float duration) {
        // Трясём камеру при взрыве рядом
        // Intensity зависит от расстояния до взрыва
    }
}
```

**В Client.cs при SMSG_DESTROY_SYNC_OBJECT:**
```csharp
// Вычисляем расстояние до игрока
float distance = Vector3.Distance(
    explosionPos, 
    GameObject.Find("Player(Clone)").transform.position
);

if(distance < 20f) {
    float intensity = 1.0f - (distance / 20f);
    CameraShake.Shake(intensity * 0.5f, 0.3f);
}
```

#### **3.4 Добавить дымовой след снаряда**
**В префабе RocketHead добавить:**
- Trail Renderer или Particle System для дымового следа
- Настройки: длина следа 2-3 секунды, постепенное затухание

---

### **Этап 4: Балансировка и оптимизация** ⚖️

#### **4.1 Настройки на сервере (SyncObjectSystem.py)**

**Текущие параметры:**
```python
self.gravity = -9.81          # Реалистичная гравитация
self.lifetime = 5.0           # Время жизни
self.explosion_radius = 5.0   # Радиус взрыва
self.explosion_damage = 50.0  # Базовый урон
initial_speed = 30.0          # Начальная скорость
```

**Рекомендуемые изменения для баланса:**
```python
self.gravity = -9.81          # Оставить
self.lifetime = 10.0          # ↑ Увеличить до 10 секунд
self.explosion_radius = 7.0   # ↑ Увеличить радиус до 7 метров
self.explosion_damage = 75.0  # ↑ Увеличить урон до 75
initial_speed = 40.0          # ↑ Увеличить скорость до 40 м/с
```

**Обоснование:**
- RPG должен быть мощнее обычного оружия
- Малый запас патронов (3 шт) требует высокого урона
- Медленная перезарядка компенсируется площадным уроном

#### **4.2 Оптимизация сетевой нагрузки**

**Проблема:** 30 пакетов в секунду на снаряд × количество снарядов

**Решение 1: Adaptive Sync Rate**
```python
def sync_to_clients(self):
    # Отправляем обновления только если позиция изменилась значительно
    if self.should_sync():
        # ... отправка пакета
        self.last_sync_position = self.position.copy()

def should_sync(self):
    if not hasattr(self, 'last_sync_position'):
        return True
    
    distance = math.sqrt(sum(
        (self.position[i] - self.last_sync_position[i])**2 
        for i in range(3)
    ))
    
    # Синхронизируем только если сместились больше чем на 10 см
    return distance > 0.1
```

**Решение 2: Понизить частоту обновлений**
```python
# В HostWorker.py изменить:
async def sync_objects_timer(self):
    while True:
        sync_object_system.update()
        await asyncio.sleep(0.05)  # 20 FPS вместо 30
```

#### **4.3 Балансировка параметров на клиенте**

**WeaponEnum.cs:**
```csharp
// Финальные сбалансированные параметры:
new Weapon(
    WeaponId.GRENADE_LAUNCHER, 
    GrenadeLauncherObject, 
    "Grenade Launcher",
    1,      // 1 снаряд в "обойме" (tube)
    3,      // 3 снаряда общий запас
    75.0f,  // Урон (информативно, реально на сервере)
    false,  // Одиночный режим
    2.0f,   // 2 секунды между выстрелами (долго!)
    "RPG_take", 
    "RPG_shot", 
    "RPG_reload",  // Долгая перезарядка
    "RPG_walk"
)
```

---

### **Этап 5: Тестирование и отладка** 🧪

#### **5.1 Unit-тесты на сервере**
```python
# test_rpg.py
def test_rocket_creation():
    # Проверяем создание ракеты
    rocket = sync_object_system.create_rocket([0, 1, 0], [0, 0, 1], "test_player")
    assert rocket is not None
    assert rocket.velocity == [0, 0, 40.0]  # initial_speed

def test_rocket_explosion():
    # Проверяем урон от взрыва
    # ...

def test_rocket_gravity():
    # Проверяем падение снаряда
    # ...
```

#### **5.2 Тестовые сценарии в игре**

**Чек-лист:**
- [ ] Выстрел создаёт снаряд на сервере
- [ ] Снаряд виден всем клиентам
- [ ] Снаряд падает под действием гравитации
- [ ] Снаряд взрывается при столкновении
- [ ] Взрыв наносит урон игрокам в радиусе
- [ ] Урон падает с расстоянием
- [ ] Эффект взрыва проигрывается
- [ ] Звук взрыва слышен
- [ ] Камера трясётся при близком взрыве
- [ ] Дымовой след отображается корректно
- [ ] Интерполяция позиции плавная
- [ ] Патроны расходуются корректно
- [ ] Перезарядка работает
- [ ] Килл учитывается с правильным weaponId (5)

#### **5.3 Стресс-тестирование**

**Тест 1: Множество снарядов**
- 10 игроков стреляют одновременно
- Проверка FPS на клиенте
- Проверка нагрузки на сервер (CPU, RAM)

**Тест 2: Сетевая задержка**
- Эмуляция lag 100-200ms
- Проверка плавности интерполяции
- Проверка корректности взрывов

---

### **Этап 6: Дополнительные улучшения** ⭐

#### **6.1 Предиктивный выстрел на клиенте**
```csharp
// В Shoot.cs при выстреле RPG:
case WeaponId.GRENADE_LAUNCHER: {
    // Создаём локальный предиктивный снаряд
    GameObject predictiveRocket = Instantiate(
        RocketPrefab, 
        cam.position, 
        Quaternion.identity
    );
    predictiveRocket.GetComponent<RocketBehaviour>().isPredictive = true;
    
    // Отправляем пакет серверу
    Packet shotPacket = new Packet(CMSG_PLAYER_WEAPON_SHOT);
    // ...
    
    // Когда придёт SMSG_CREATE_SYNC_OBJECT - уничтожаем предиктивный
}
```

**Преимущества:**
- Мгновенная реакция на выстрел (нет задержки ping)
- Сервер всё равно авторитетен
- При рассинхронизации сервер перезаписывает

#### **6.2 Урон по окружению**
```python
# В SyncObjectSystem.py при взрыве:
def explode(self):
    # Наносим урон игрокам
    # ...
    
    # Наносим урон разрушаемым объектам
    for obj in destroyable_objects:
        distance = calculate_distance(self.position, obj.position)
        if distance <= self.explosion_radius:
            obj.take_damage(self.explosion_damage)
```

#### **6.3 Отображение траектории (опционально)**
```csharp
// UI прицел с предсказанием траектории снаряда
public void DrawTrajectoryPrediction() {
    Vector3 velocity = cam.forward * 40f;
    Vector3 position = cam.position;
    
    for(int i = 0; i < 50; i++) {
        float t = i * 0.1f;
        velocity.y += -9.81f * 0.1f;
        position += velocity * 0.1f;
        
        Debug.DrawLine(lastPos, position, Color.red, 0.1f);
        lastPos = position;
    }
}
```

#### **6.4 Режим прямого попадания**
```python
# Прямое попадание снарядом даёт бонусный урон
if self.hit_player_directly:
    damage = self.explosion_damage * 1.5  # +50% за прямое попадание
```

---

## 🗂️ Структура файлов после доработки

```
UnityServerGameRep/
├── Assets/
│   ├── Code/
│   │   ├── PacketHeaders.cs             [✏️ ИЗМЕНИТЬ - добавить пакеты 42-44]
│   │   ├── Client.cs                    [✏️ ИЗМЕНИТЬ - добавить обработку пакетов]
│   │   ├── Player/
│   │   │   ├── WeaponEnum.cs           [✏️ ИЗМЕНИТЬ - параметры RPG]
│   │   │   ├── WeaponSystem.cs         [✏️ ИЗМЕНИТЬ - звуки RPG]
│   │   │   ├── Shoot.cs                [✏️ ИЗМЕНИТЬ - звук выстрела RPG]
│   │   │   └── RocketBehaviour.cs      [➕ СОЗДАТЬ]
│   │   └── Effects/
│   │       └── CameraShake.cs          [➕ СОЗДАТЬ]
│   ├── Prefabs/
│   │   ├── RocketHead.prefab           [➕ СОЗДАТЬ]
│   │   └── ExplosionEffect.prefab      [➕ СОЗДАТЬ]
│   ├── Models/
│   │   ├── WeaponObjects/
│   │   │   ├── BazookaModel.prefab     [✅ СУЩЕСТВУЕТ]
│   │   │   └── RGP_Head.blend          [✅ СУЩЕСТВУЕТ]
│   │   └── FPSAnim/FBX/animations/
│   │       ├── RPG/                    [✅ СУЩЕСТВУЕТ]
│   │       │   ├── idle.anim
│   │       │   └── shot.anim
│   │       └── hands/
│   │           ├── H_RPG_take.anim     [➕ СОЗДАТЬ]
│   │           ├── H_RPG_shot.anim     [➕ СОЗДАТЬ]
│   │           ├── H_RPG_reload.anim   [➕ СОЗДАТЬ]
│   │           └── H_RPG_walk.anim     [➕ СОЗДАТЬ]
│   └── Audio/
│       └── Weapons/RPG/
│           ├── RPG_Take.wav            [➕ ДОБАВИТЬ]
│           ├── RPG_Shot.wav            [➕ ДОБАВИТЬ]
│           ├── RPG_Reload.wav          [➕ ДОБАВИТЬ]
│           └── RPG_Explosion.wav       [➕ ДОБАВИТЬ]

ServerShooter/
├── Features/
│   └── Weapon.py                       [✅ СУЩЕСТВУЕТ]
├── SyncObjectSystem.py                  [✏️ ИЗМЕНИТЬ - оптимизация]
├── PacketHeaders.py                     [✅ СУЩЕСТВУЕТ]
└── HostWorker.py                        [✅ СУЩЕСТВУЕТ]
```

---

## ⏱️ Оценка времени

| Этап | Задачи | Время |
|------|--------|-------|
| **1. Критические исправления** | Пакеты, префаб, скрипты | 4 часа |
| **2. Параметры оружия** | WeaponEnum, анимации | 2 часа |
| **3. Аудио и эффекты** | Звуки, взрыв, shake | 3 часа |
| **4. Балансировка** | Настройка параметров | 2 часа |
| **5. Тестирование** | Отладка, фикс багов | 3 часа |
| **6. Дополнительно** | Предикция, траектория | 2 часа |
| **ИТОГО** | | **16 часов** |

---

## 🎯 Приоритеты

### **Высокий приоритет (MVP):**
1. Этап 1 - Обработка пакетов и визуализация снаряда
2. Этап 2.1-2.3 - Исправление параметров оружия
3. Этап 3.2 - Эффект взрыва
4. Этап 5 - Базовое тестирование

### **Средний приоритет:**
5. Этап 2.4 - Анимации рук
6. Этап 3.1 - Звуки
7. Этап 4 - Балансировка

### **Низкий приоритет (полировка):**
8. Этап 3.3 - Camera Shake
9. Этап 6 - Дополнительные улучшения

---

## 📝 Примечания

### **Архитектурные решения автора:**
1. **Server-authoritative подход** - правильное решение для предотвращения читов
2. **Отдельная система SyncObjectSystem** - хорошая абстракция для будущих снарядов
3. **Использование существующей системы пакетов** - консистентно с архитектурой
4. **3D модель снаряда RGP_Head** - автор планировал визуализацию

### **Что можно улучшить:**
1. **Добавить пулинг объектов** - снаряды создаются/уничтожаются часто
2. **Сжатие данных синхронизации** - float → short для позиции (±327 метров с точностью 1см)
3. **Интерполяция Hermite** вместо SmoothDamp - более точная для снарядов
4. **LOD для эффектов** - упрощённые эффекты вдали от камеры

---

## 🚀 Следующие шаги

1. ✅ План составлен
2. ⏸️ Утверждение плана
3. ⏳ Начать с Этапа 1 (критические исправления)
4. ⏳ Итеративное тестирование после каждого этапа
5. ⏳ Финальная балансировка перед релизом

---

**Дата создания:** 2026-02-13  
**Версия:** 1.0  
**Автор плана:** Claude (на основе анализа кода автора)
