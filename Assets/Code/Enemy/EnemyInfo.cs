using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponEnumIds;

public class EnemyInfo : MonoBehaviour
{
    public string playerId;
    public string PlayerName;
    public int playerAnimId = 0;
    public WeaponId weaponId = 0;
    public GameObject WeaponParentBone;
    public GameObject WeaponObject = null;

    // Система плавного движения для устранения дергания
    private Vector3 targetPosition;
    private Vector3 networkPosition;
    private float horizontalLerpRate = 15f; // Скорость интерполяции для X,Z
    private float verticalLerpRate = 20f; // Более быстрая интерполяция для Y (прыжки)
    private bool hasReceivedFirstPosition = false;

    private void Awake() { 
        WeaponParentBone = transform.Find("Armature").Find("mixamorig:Hips").Find("mixamorig:Spine").Find("mixamorig:Spine1").Find("mixamorig:Spine2").Find("mixamorig:RightShoulder").Find("mixamorig:RightArm").Find("mixamorig:RightForeArm").Find("mixamorig:RightHand").Find("mixamorig:RightHand_end").gameObject;
    }
    
    private void Start()
    {
        SetupNetworkPlayer(); // Специальная настройка для сетевых игроков
        networkPosition = transform.position;
        targetPosition = transform.position;
    }

    private void Update()
    {
        // Плавная интерполяция к целевой позиции для устранения дергания
        if (hasReceivedFirstPosition)
        {
            Vector3 currentPos = transform.position;
            Vector3 newPosition = currentPos;
            
            // Разная скорость интерполяции для горизонтального и вертикального движения
            // Горизонтальное движение (X, Z)
            newPosition.x = Mathf.Lerp(currentPos.x, targetPosition.x, horizontalLerpRate * Time.deltaTime);
            newPosition.z = Mathf.Lerp(currentPos.z, targetPosition.z, horizontalLerpRate * Time.deltaTime);
            
            // Вертикальное движение (Y) - быстрее для плавных прыжков
            newPosition.y = Mathf.Lerp(currentPos.y, targetPosition.y, verticalLerpRate * Time.deltaTime);
            
            transform.position = newPosition;
        }
    }

    // Метод для установки новой сетевой позиции
    public void SetNetworkPosition(Vector3 newPosition)
    {
        networkPosition = newPosition;
        
        if (!hasReceivedFirstPosition)
        {
            // Первая позиция устанавливается сразу
            transform.position = newPosition;
            targetPosition = newPosition;
            hasReceivedFirstPosition = true;
        }
        else
        {
            // Проверяем, не слишком ли большое расстояние (телепортация)
            float distance = Vector3.Distance(transform.position, newPosition);
            if (distance > 5f) // Если расстояние больше 5 метров - телепортируем сразу
            {
                transform.position = newPosition;
                targetPosition = newPosition;
            }
            else
            {
                // Устанавливаем новую целевую позицию для плавного движения
                targetPosition = newPosition;
            }
        }
    }

    // Специальная настройка для сетевых игроков - отключаем физику полностью
    public void SetupNetworkPlayer()
    {
        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        
        foreach(Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = true; // Все Rigidbody кинематические
        }        
        foreach(Collider col in colliders)
        {
            col.enabled = false; // Отключаем все коллайдеры кроме основного
        }
        
        // ОСНОВНОЕ ОТЛИЧИЕ: основной Rigidbody тоже кинематический для сетевых игроков
        this.transform.GetComponent<Rigidbody>().isKinematic = true;
        this.transform.GetComponent<Rigidbody>().useGravity = false; // Отключаем гравитацию
        this.transform.GetComponent<CapsuleCollider>().enabled = true;
        this.transform.GetComponent<Animator>().enabled = true;
    }

    public void RaggDollOff()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach(Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = true;
        }        
        foreach(Collider col in colliders)
        {
            col.enabled = false;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = false;
        this.transform.GetComponent<CapsuleCollider>().enabled = true;
        this.transform.GetComponent<Animator>().enabled = true;
    }
    
    public void RaggDollOn()
    {

        Rigidbody[] rb = this.transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach(Rigidbody rigidbody in rb)
        {
            rigidbody.isKinematic = false;
        }        
        foreach(Collider col in colliders)
        {
            col.enabled = true;
        }
        this.transform.GetComponent<Rigidbody>().isKinematic = true;
        this.transform.GetComponent<CapsuleCollider>().enabled = false;
        this.transform.GetComponent<Animator>().enabled = false;
        
    }
}
