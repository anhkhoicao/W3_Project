using UnityEngine;

public class FloatingSprite : MonoBehaviour
{
    public float amplitude = 0.09f; // Biên độ dao động (độ cao lên xuống)
    public float frequency = 3f;  // Tần số dao động (tốc độ lên xuống)

    private Vector3 startPos;     // Vị trí ban đầu của Sprite

    void Start()
    {
        startPos = transform.position; // Lưu vị trí ban đầu
    }

    void Update()
    {
        // Tạo chuyển động lên xuống
        float newY = startPos.y + Mathf.Sin(Time.time * frequency) * amplitude;

        // Cập nhật vị trí của Sprite
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
