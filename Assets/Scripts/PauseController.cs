using System.Collections;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] float animationDuration = 0.2f; // Thời gian hiệu ứng

    private Vector3 initialScale = Vector3.zero; // Scale bắt đầu (nhỏ nhất)
    private Vector3 targetScale = Vector3.one;   // Scale kết thúc (bình thường)
    private Coroutine scaleCoroutine;

    private bool isPaused = false; // Biến trạng thái tạm dừng

    public void Pause()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        pauseMenu.SetActive(true); // Bật menu
        scaleCoroutine = StartCoroutine(ScaleOverTime(pauseMenu.transform, initialScale, targetScale, animationDuration));

        isPaused = true; // Đặt trạng thái tạm dừng
    }

    public void Resume()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleOverTime(pauseMenu.transform, targetScale, initialScale, animationDuration, () =>
        {
            pauseMenu.SetActive(false); // Tắt menu sau hiệu ứng
        }));

        isPaused = false; // Đặt trạng thái tiếp tục
    }

    public bool IsPaused()
    {
        return isPaused; // Trả về trạng thái tạm dừng
    }

    private IEnumerator ScaleOverTime(Transform target, Vector3 from, Vector3 to, float duration, System.Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime; // Sử dụng unscaledDeltaTime để không bị ảnh hưởng bởi Time.timeScale = 0
            yield return null;
        }

        target.localScale = to; // Đảm bảo giá trị cuối cùng
        onComplete?.Invoke(); // Gọi callback nếu có
    }
}