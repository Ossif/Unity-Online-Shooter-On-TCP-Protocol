using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotifyLogic : MonoBehaviour
{
    //notifications
    public GameObject notificationPrefab;
    public Transform CanvasTransform;
    public int maxNotifications = 3;
    public int currentNotifications = 0;
    
    public void CreateNotify(NotificationType type, string title, string description)
    {
        if (currentNotifications >= maxNotifications)
        {
            return;
        }

        GameObject notificationObject = Instantiate(notificationPrefab, CanvasTransform);
        Notification notification = notificationObject.GetComponent<Notification>();

        notification.Initialize(type, title, description, HideNotification);


        float x = Screen.width - notification.GetWidth() - 10f;
        float y = Screen.height - ((currentNotifications + 1) * notification.GetHeight()) - 10f;
        notification.SetPosition(new Vector2(x, y));

        currentNotifications++;

        //Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        currentNotifications--;

        if (transform.childCount > 0)
        {
            Destroy(transform.GetChild(0).gameObject);
        }
    }
}
