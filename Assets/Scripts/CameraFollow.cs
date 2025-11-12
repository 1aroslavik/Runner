using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Цель для слежения")]
    public Transform target;          // объект, за которым следует камера

    [Header("Настройки камеры")]
    public float smoothSpeed = 5f;    // скорость сглаживания
    public Vector3 offset;            // смещение относительно игрока

    void LateUpdate()
    {
        if (target == null) return;

        // Позиция, к которой камера должна двигаться
        Vector3 desiredPosition = target.position + offset;

        // Плавное движение
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Обновляем позицию камеры
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
