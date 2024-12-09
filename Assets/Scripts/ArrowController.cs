using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour
{
    public RectTransform arrow; // Mũi tên (UI Object)
    public RectTransform progressBar; // Thanh progress
    public RectTransform successZone; // Vùng xanh (vùng success)

    public float speed = 200f; // Tốc độ di chuyển
    private bool movingRight = true; // Hướng di chuyển (phải/trái)

    private bool isMinigameActive = false; // Trạng thái Minigame
    private bool isMinigameStarted = false; // Kiểm tra minigame đã bắt đầu hay chưa

    void Update()
    {
        Debug.Log("Ben Arrow: " + isMinigameActive);
        if (!isMinigameActive) return;

        // Di chuyển mũi tên qua lại
        if (movingRight)
        {
            arrow.anchoredPosition += Vector2.right * speed * Time.deltaTime;
            if (arrow.anchoredPosition.x >= progressBar.rect.width / 2)
                movingRight = false;
        }
        else
        {
            arrow.anchoredPosition += Vector2.left * speed * Time.deltaTime;
            if (arrow.anchoredPosition.x <= -progressBar.rect.width / 2)
                movingRight = true;
        }

        // Kiểm tra nếu nhấn Space để kiểm tra thành công
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckSuccess();
        }
    }



    // Phương thức thay đổi trạng thái Minigame
    public void SetMinigameState(bool isActive)
    {
        // Chỉ thực hiện thay đổi trạng thái khi trạng thái thật sự thay đổi
        if (isMinigameActive == isActive) return;

        isMinigameActive = isActive;
        Debug.Log("Minigame Active: " + isMinigameActive);

        if (isActive)
        {
            // Nếu minigame bắt đầu, reset mũi tên
            if (!isMinigameStarted)
            {
                if (arrow != null)
                {
                    arrow.anchoredPosition = new Vector2(-progressBar.rect.width / 2, arrow.anchoredPosition.y);
                }
                isMinigameStarted = true; // Đánh dấu minigame đã bắt đầu
                Debug.Log("Minigame started, arrow reset.");
            }
        }
        else
        {
            // Khi minigame kết thúc
            isMinigameStarted = false;
            Debug.Log("Minigame stopped.");
        }
    }

    public bool CheckSuccess()
    {
        if (!isMinigameActive)
        {
            Debug.Log("Minigame chưa hoạt động!");
            return false;
        }

        float arrowX = arrow.anchoredPosition.x;
        float successMin = successZone.anchoredPosition.x - successZone.rect.width / 2;
        float successMax = successZone.anchoredPosition.x + successZone.rect.width / 2;

        if (arrowX >= successMin && arrowX <= successMax)
        {
            Debug.Log("Success!");

            // Gọi hàm hiển thị cá trong PlayerController
            FindObjectOfType<PlayerController>().OnFishingSuccess();

            return true;
        }
        else
        {
            Debug.Log("Miss!");
            return false;
        }
    }


}