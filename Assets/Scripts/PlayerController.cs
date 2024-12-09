using System.Collections;
using UnityEngine;
using TMPro; // Đảm bảo bạn import namespace TextMeshPro
public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb; // Gán Rigidbody2D từ Inspector


    // Danh sách sprite của các loại cá (gán qua Inspector)
    public Sprite[] fishSprites;
    public GameObject fishPrefab; // Prefab cá để hiển thị (gán qua Inspector)
    public Transform player; // Vị trí nhân vật (gán qua Inspector)

    // Thêm trong PlayerController
    public GameObject minigameUI; // Tham chiếu đến UI của Minigame (gán qua Inspector)
    private bool isMinigameActive = false; // Trạng thái Minigame

    // Thêm biến trong PlayerController
    public ArrowController arrowController;
    private bool isInNPCZone = false; // Kiểm tra xem có đang ở vùng NPC1 không
    public PauseController pauseController;
    public float speed = 1.7f; // Tốc độ di chuyển
    private float moveDirection; // Hướng di chuyển (-1, 0, 1)

    private bool isFacingRight = true; // Kiểm tra hướng mặt nhân vật
    private bool isSea = false; // Kiểm tra nhân vật đang ở trong biển
    private bool isFishing = false; // Kiểm tra trạng thái câu cá

    public int fishCaughtCount = 0; // Biến đếm số cá đã câu được

    public int gold = 0; // Biến đếm số cá đã câu được
    public TextMeshProUGUI fishCaughtText; // Cập nhật kiểu thành TextMeshProUGUI
    public TextMeshProUGUI goldText; // Cập nhật kiểu thành TextMeshProUGUI
    private Animator animator; // Animator của nhân vật

    public SpriteRenderer fishingSprite; // Sprite cần ẩn/hiện (gán qua Inspector)

    public SpriteRenderer SellfishingSprite; // Sprite cần ẩn/hiện (gán qua Inspector)

    public float fishBiteDelay; // Thời gian chờ cá cắn câu (giây)

    // Camera

    private bool isCameraFollowing = true; // Mặc định camera theo dõi
    public Transform cameraTarget;
    public Vector3 cameraOffset;
    public float cameraSmoothTime = 0.3f;
    private Vector3 cameraVelocity = Vector3.zero;

    // Thông số Zoom camera
    public Camera mainCamera; // Camera chính
    public float normalCameraSize = 2.6f; // Kích thước camera bình thường
    public float zoomedCameraSize = 1.8f; // Kích thước camera khi zoom vào
    public float zoomSpeed = 2f; // Tốc độ zoom

    private bool isZoomed = false; // Kiểm tra trạng thái zoom

    void Start()
    {
        // Lấy Animator từ GameObject
        animator = GetComponent<Animator>();
        fishingSprite.enabled = false;
        SellfishingSprite.enabled = false;
        if (animator == null)
        {
            Debug.LogError("Animator chưa được gán! Hãy đảm bảo GameObject có Component Animator.");
        }

        if (cameraOffset == Vector3.zero)
        {
            cameraOffset = new Vector3(0, 0, -10); // Gán giá trị mặc định
        }

        // Đặt camera target nếu chưa có
        if (cameraTarget == null)
        {
            cameraTarget = transform;
        }

        // Gán camera chính nếu chưa có
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        // Di chuyển nhân vật
        MoveCharacter();

        // Xử lý khi nhấn phím Space
        HandleFishing();

        // Lật nhân vật nếu cần
        FlipCharacter();

        // Xử lý khi nhấn phím E trong vùng NPC1
        HandleNPCInteraction();

        // Zoom camera nếu đang ở trong khu vực zoom
        if (isZoomed)
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, zoomedCameraSize, Time.deltaTime * zoomSpeed);
        }
        else
        {
            mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, normalCameraSize, Time.deltaTime * zoomSpeed);
        }
    }

    private void MoveCharacter()
    {
        // Nếu đang câu cá thì không cho phép di chuyển
        if (isFishing)
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // Đứng yên, giữ nguyên trục Y (nhảy nếu có)
            animator.SetFloat("xVelocity", 0); // Tốc độ di chuyển bằng 0
            return; // Thoát hàm
        }

        // Lấy hướng di chuyển từ phím A/D hoặc mũi tên trái/phải
        moveDirection = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);

        // Cập nhật Animator
        if (animator != null)
        {
            animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x)); // Tốc độ di chuyển
        }
    }


    private void HandleNPCInteraction()
    {
        if (isInNPCZone && Input.GetKeyDown(KeyCode.E))
        {
            if (pauseController != null)
            {
                // Kiểm tra trạng thái của menu tạm dừng
                if (pauseController.IsPaused())
                {
                    pauseController.Resume(); // Gọi Resume nếu menu đang mở
                    Debug.Log("Resume game khi nhấn E.");
                }
                else
                {
                    pauseController.Pause(); // Gọi Pause nếu menu đang đóng
                    Debug.Log("Pause game khi nhấn E.");
                }
            }
            else
            {
                Debug.LogError("PauseController chưa được gán!");
            }
        }
    }

    private void HandleFishing()
    {
        Debug.Log("Ben Player lay arrow" + arrowController);
        if (arrowController != null)
        {
            Debug.Log("Ben Player " + isMinigameActive);
            arrowController.SetMinigameState(isMinigameActive); // Cập nhật trạng thái Minigame cho ArrowController
        }

        if (Input.GetKeyDown(KeyCode.Space) && isSea)
        {
            if (!isFishing && !isMinigameActive)
            {
                // Bắt đầu câu cá (gọi Coroutine để chờ)
                StartCoroutine(StartFishingCoroutine());
            }
            else if (isMinigameActive)
            {
                // Kiểm tra trạng thái success/miss trước khi ẩn UI
                arrowController.CheckSuccess();

                // Ẩn Minigame
                minigameUI.SetActive(false); // Tắt UI Minigame
                isMinigameActive = false; // Tắt trạng thái Minigame
                isFishing = false; // Tắt trạng thái câu cá
                animator.SetBool("isFishing", false); // Dừng animation câu cá
                Debug.Log("Ẩn Minigame.");
            }
        }
    }

    private IEnumerator StartFishingCoroutine()
    {
        // Đặt trạng thái đang câu cá
        isFishing = true;

        // Bật animation ném cần câu (với trigger)
        animator.SetBool("isFishing", true);
        animator.SetTrigger("ThrowFish");
        Debug.Log("Đang chờ cá cắn câu...");

        fishBiteDelay = Random.Range(6f, 10f); // Random trong khoảng [6, 10]
        Debug.Log("Thời gian chờ cá cắn câu: " + fishBiteDelay + " giây");
        // Chờ trong một khoảng thời gian (giả lập thời gian chờ cá cắn câu)
        yield return new WaitForSeconds(fishBiteDelay); // Chờ 2 giây (hoặc giá trị tùy chỉnh)

        // Hiển thị Minigame
        minigameUI.SetActive(true); // Bật UI Minigame
        isMinigameActive = true; // Đặt trạng thái Minigame đang hoạt động
        Debug.Log("Cá đã cắn câu, bắt đầu Minigame!");
    }
    private void FlipCharacter()
    {
        // Lật mặt nhân vật khi thay đổi hướng
        if (moveDirection > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1; // Lật nhân vật
        transform.localScale = scale;
    }

    private void LateUpdate()
    {
        if (isCameraFollowing && cameraTarget != null)
        {
            Vector3 targetPosition = cameraTarget.position + cameraOffset;
            Camera.main.transform.position = Vector3.SmoothDamp(
                Camera.main.transform.position,
                targetPosition,
                ref cameraVelocity,
                cameraSmoothTime
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Sea"))
        {
            isSea = true;
            Debug.Log("Vào khu vực biển.");
            if (fishingSprite != null) fishingSprite.enabled = true;
        }

        if (collision.CompareTag("NPC1"))
        {
            isInNPCZone = true; // Vào vùng NPC1
            if (SellfishingSprite != null) SellfishingSprite.enabled = true;
            Debug.Log("Vào khu vực NPC1.");
        }

        if (collision.CompareTag("Zoom"))
        {
            isZoomed = true;
            Debug.Log("Vào khu vực zoom.");
        }

        if (collision.CompareTag("NoFollowZone"))
        {
            isCameraFollowing = false;
            Debug.Log("Camera ngừng theo dõi nhân vật.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Sea"))
        {
            isSea = false;
            Debug.Log("Rời khu vực biển.");
            if (fishingSprite != null) fishingSprite.enabled = false;
        }

        if (collision.CompareTag("NPC1"))
        {
            isInNPCZone = false; // Thoát vùng NPC1
            if (SellfishingSprite != null) SellfishingSprite.enabled = false;
            Debug.Log("Rời khu vực NPC1.");
        }

        if (collision.CompareTag("Zoom"))
        {
            isZoomed = false;
            Debug.Log("Rời khu vực zoom.");
        }

        if (collision.CompareTag("NoFollowZone"))
        {
            isCameraFollowing = true;
            Debug.Log("Camera bắt đầu theo dõi lại nhân vật.");
        }
    }
    // Thêm vào hàm CheckSuccess() trong ArrowController
    public void OnFishingSuccess()
    {
        // Chọn ngẫu nhiên sprite cá
        Sprite randomFishSprite = fishSprites[Random.Range(0, fishSprites.Length)];
        if (fishSprites.Length == 0)
        {
            Debug.LogError("Fish sprites are empty!");
            return;
        }

        // Tạo đối tượng cá từ prefab
        Vector3 newPosition = new Vector3(player.position.x + 1.2f, player.position.y - 0.4f, player.position.z);
        GameObject fish = Instantiate(fishPrefab, newPosition, Quaternion.identity);

        // Gán sprite ngẫu nhiên cho đối tượng cá
        SpriteRenderer fishRenderer = fish.GetComponent<SpriteRenderer>();
        if (fishRenderer != null)
        {
            fishRenderer.sprite = randomFishSprite;
        }

        // Bắt đầu hiệu ứng bay cá về phía nhân vật
        StartCoroutine(FishFlyToPlayer(fish));

        // Tăng số lượng cá đã câu được
        fishCaughtCount++;

        if (fishCaughtText != null)
        {
            fishCaughtText.text = fishCaughtCount.ToString();
        }


        // Hiển thị số lượng cá đã câu được trong log hoặc UI
        Debug.Log("Số cá đã câu được: " + fishCaughtCount);
    }
    private IEnumerator FishFlyToPlayer(GameObject fish)

    {
        float delayTime = 0.8f; // Thời gian trì hoãn (1 giây)
        yield return new WaitForSeconds(delayTime); // Chờ 1 giây

        float duration = 0.8f; // Thời gian hiệu ứng
        float elapsedTime = 0f;

        Vector3 startPosition = fish.transform.position; // Vị trí ban đầu của cá
        Vector3 targetPosition = player.position; // Vị trí của nhân vật

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            // Di chuyển cá từ vị trí ban đầu tới nhân vật
            fish.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);

            // Hiệu ứng xoay cá khi di chuyển
            fish.transform.Rotate(Vector3.forward * 360 * Time.deltaTime);

            yield return null;
        }

        // Sau khi tới nhân vật, tiêu hủy đối tượng cá
        Destroy(fish);

        Debug.Log("Cá đã được thu thập!");
    }


    public void SellOne()
    {
        if (fishCaughtCount > 0)
        {
            // Giảm số lượng cá
            fishCaughtCount--;

            // Cập nhật số cá trên UI
            if (fishCaughtText != null)
            {
                fishCaughtText.text = fishCaughtCount.ToString();
            }

            // Tăng vàng
            int goldEarned = 10; // Giá trị vàng mỗi con cá
            AddGold(goldEarned);

            Debug.Log("Bán 1 con cá. Còn lại: " + fishCaughtCount + " cá. Tổng vàng: " + gold);
        }
        else
        {
            Debug.Log("Không còn cá để bán!");
        }
    }

    public void SellAll()
    {
        if (fishCaughtCount > 0)
        {
            // Lưu số lượng cá trước khi bán
            int fishToSell = fishCaughtCount;

            // Đặt số cá về 0
            fishCaughtCount = 0;

            // Cập nhật UI số cá
            if (fishCaughtText != null)
            {
                fishCaughtText.text = fishCaughtCount.ToString();
            }

            // Tăng vàng
            int goldEarned = fishToSell * 10; // Giá trị vàng mỗi con cá
            AddGold(goldEarned);

            Debug.Log("Bán toàn bộ " + fishToSell + " con cá. Tổng vàng: " + gold);
        }
        else
        {
            Debug.Log("Không còn cá để bán!");
        }
    }


    private void AddGold(int amount)
    {
        gold += amount; // Tăng số vàng
        if (goldText != null)
        {
            goldText.text = gold.ToString(); // Cập nhật UI
        }
        Debug.Log("Nhận được " + amount + " vàng. Tổng vàng hiện tại: " + gold);
    }








}


