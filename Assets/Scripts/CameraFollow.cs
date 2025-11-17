using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Следим за этим объектом")]
    public Transform target;

    [Header("Настройки движения")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, 0);

    [Header("Наезд (Intro)")]
    public float startZoom = 14f;   // издалека
    public float finalZoom = 6f;    // нормальный зум
    public float zoomDuration = 1.2f;

    private Camera cam;

    // 🔥 ЭТО ВАЖНО — этo поле было отсутствующим!
    private bool introDone = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // пока интро не закончено — движение камеры не работаем
        if (!introDone)
            return;

        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = -10f;  // фикс Z для 2D

        Vector3 smoothedPosition =
            Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;
    }

    // ======================================================
    // ВЫЗЫВАЕТСЯ ИЗ PlayerSpawn после спавна игрока
    // ======================================================
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // камера прыгает к начальной позиции игрока
        transform.position = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            -10f
        );

        // запускаем наезд камеры
        StartCoroutine(IntroSequence());
    }

    IEnumerator IntroSequence()
    {
        introDone = false;

        // ставим стартовый зум
        cam.orthographicSize = startZoom;

        float t = 0;

        while (t < zoomDuration)
        {
            t += Time.deltaTime;
            float k = t / zoomDuration;

            // зум камеры
            cam.orthographicSize = Mathf.Lerp(startZoom, finalZoom, k);

            // движение камеры к игроку
            Vector3 desired = target.position + offset;
            desired.z = -10f;

            transform.position = Vector3.Lerp(transform.position, desired, k);

            yield return null;
        }

        cam.orthographicSize = finalZoom;
        introDone = true;      // 🔥 теперь LateUpdate начинает следить за игроком
    }
}
