using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    public Camera mainCamera; // Camera chính
    public float zoomedSize = 3f; // Kích thước khi zoom
    public float defaultSize = 5f; // Kích thước mặc định
    public float zoomSpeed = 2f; // Tốc độ zoom

    private bool isZoomed = false; // Trạng thái camera hiện tại

    private void Start()
    {
        // Nếu chưa gán camera, tự động gán Camera chính
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        // Lựa chọn kích thước camera dựa trên trạng thái
        float targetSize = isZoomed ? zoomedSize : defaultSize;
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Zoom"))
        {
            isZoomed = true; // Bật trạng thái zoom
            Debug.Log("Camera zoom vào.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Zoom"))
        {
            isZoomed = false; // Tắt trạng thái zoom
            Debug.Log("Camera trở lại bình thường.");
        }
    }
}
