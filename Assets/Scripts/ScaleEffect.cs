using System.Collections;
using UnityEngine;

public class ScaleEffect : MonoBehaviour
{
    [SerializeField] float animationDuration = 0.2f; // Thời gian hiệu ứng
    private Vector3 initialScale = Vector3.zero; // Scale bắt đầu
    private Vector3 targetScale = Vector3.one;   // Scale kết thúc
    private Coroutine scaleCoroutine;

    /// <summary>
    /// Bắt đầu hiệu ứng scale từ nhỏ đến lớn.
    /// </summary>
    public void PlayOpenEffect()
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        transform.localScale = initialScale; // Đặt scale về nhỏ nhất
        scaleCoroutine = StartCoroutine(ScaleOverTime(initialScale, targetScale));
    }

    /// <summary>
    /// Bắt đầu hiệu ứng scale từ lớn về nhỏ.
    /// </summary>
    public void PlayCloseEffect(System.Action onComplete = null)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleOverTime(targetScale, initialScale, onComplete));
    }

    private IEnumerator ScaleOverTime(Vector3 from, Vector3 to, System.Action onComplete = null)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            transform.localScale = Vector3.Lerp(from, to, elapsed / animationDuration);
            elapsed += Time.unscaledDeltaTime; // Không bị ảnh hưởng bởi Time.timeScale
            yield return null;
        }

        transform.localScale = to; // Đảm bảo giá trị cuối cùng chính xác
        onComplete?.Invoke(); // Gọi callback nếu có
    }
}
