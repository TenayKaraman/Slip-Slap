using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [Header("Input Settings")]
    public float minSwipeDistance = 50f;
    public GridEntity player; // Player yerine GridEntity kullandýk (geçici)

    private Vector2 startTouchPos;
    private bool isTouching = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Mouse/Touch input
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            isTouching = true;
        }

        if (Input.GetMouseButtonUp(0) && isTouching)
        {
            Vector2 endTouchPos = Input.mousePosition;
            Vector2 swipeDelta = endTouchPos - startTouchPos;

            if (swipeDelta.magnitude >= minSwipeDistance)
            {
                Vector2Int swipeDirection = GetSwipeDirection(swipeDelta);
                OnSwipe(swipeDirection);
            }

            isTouching = false;
        }

        // Keyboard input (test için)
        if (Input.GetKeyDown(KeyCode.W)) OnSwipe(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) OnSwipe(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) OnSwipe(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) OnSwipe(Vector2Int.right);
    }

    Vector2Int GetSwipeDirection(Vector2 swipeDelta)
    {
        if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
        {
            return swipeDelta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            return swipeDelta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    }

    void OnSwipe(Vector2Int direction)
    {
        if (player != null)
        {
            player.Move(direction);
        }
        else
        {
            Debug.Log($"Swipe detected: {direction} (No player assigned)");
        }
    }
    
}