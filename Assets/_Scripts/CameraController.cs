using UnityEngine;

/// <summary>
/// WASD(또는 방향키)로 카메라를 수평 이동시키고, 마우스 휠로는 카메라가 보는 방향(forward)
/// 그대로 전진/후진시켜 확대/축소하며, 스페이스바로 최초 카메라 뷰(위치/회전/FOV)로
/// 복귀시키는 스크립트.
///
/// 팬(WASD 이동)과 줌(마우스 휠)은 서로 다른 값으로 완전히 분리되어 있다:
///  - panX/panZ : WASD로만 바뀌며 Min/Max X, Min/Max Z 범위로 클램프된다.
///  - zoomHeight: 마우스 휠로만 바뀌며 Min/Max Zoom Y 범위로 클램프된다.
/// 최종 카메라 위치는 이 둘을 합쳐서 계산하므로, 줌을 아무리 하더라도
/// WASD로 이동 가능한 공간(팬 범위)은 줄어들거나 늘어나지 않고 항상 동일하다.
///
/// 이 스크립트를 붙인 오브젝트의 Camera 컴포넌트를 자동으로 사용하므로,
/// 반드시 Camera가 붙어있는 오브젝트(메인 카메라 등)에 부착해야 한다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("초기 카메라 상태 (스페이스바로 복귀할 기준값)")]
    [SerializeField] private Vector3 initialPosition = new Vector3(11.26123f, 153.1006f, -10f);
    [SerializeField] private Vector3 initialRotation = new Vector3(40.859f, -1.456f, -0.664f);
    [SerializeField] private float initialFieldOfView = 12f;

    [Header("이동 속도")]
    [SerializeField] private float moveSpeed = 20f;

    [Header("스무딩 (값이 작을수록 더 부드럽고 관성이 커짐)")]
    [SerializeField] private float smoothTime = 0.15f;

    [Header("X축 이동 제한 (background-ground 스프라이트의 실제 월드 범위)")]
    [SerializeField] private float minX = -78.95f;
    [SerializeField] private float maxX = 88.09f;

    [Header("Z축 이동 제한 (background-ground 스프라이트의 실제 월드 범위)")]
    [SerializeField] private float minZ = -36.43f;
    [SerializeField] private float maxZ = 15.63f;

    [Header("줌 설정 (마우스 휠로 카메라를 보는 방향으로 전진/후진)")]
    [SerializeField] private float zoomSpeed = 40f;
    [Tooltip("최대로 확대(줌인)했을 때의 카메라 높이(Y) 하한")]
    [SerializeField] private float minZoomY = 50f;
    [Tooltip("최대 줌아웃 높이(Y). Initial Position의 Y와 같아야 \"처음 카메라 뷰 = 맵 전체\"가 성립하고, 그 이상으로는 축소되지 않는다.")]
    [SerializeField] private float maxZoomY = 153.1006f;
    [Tooltip("이 값(0~1) 이상 줌아웃되면(Max Zoom Y, 즉 처음 뷰에 가까워지면) WASD 이동을 잠근다. 1이면 처음 뷰 그대로일 때만 잠기고, 조금이라도 줌인하면 즉시 풀린다.")]
    [SerializeField, Range(0f, 1f)] private float panLockZoomThreshold = 0.99f;

    // 이 스크립트가 붙은 오브젝트의 Camera 컴포넌트 캐시.
    private Camera cam;

    // 팬(WASD)으로만 바뀌는 X/Z 목표값. 줌과는 완전히 독립적이라 팬 가능 범위가 항상 동일하다.
    private float panX;
    private float panZ;

    // 줌(마우스 휠)으로만 바뀌는 목표 높이(Y).
    private float zoomHeight;

    // 회전은 절대 바뀌지 않으므로, 줌 방향(forward)을 시작 시 한 번만 계산해 재사용한다.
    private Vector3 zoomForward;

    // 팬/줌을 합쳐 계산한 최종 목표 위치. 실제 이동은 이 값을 향해 SmoothDamp로 따라간다.
    private Vector3 targetPosition;

    // SmoothDamp가 내부적으로 사용하는 현재 속도 (레퍼런스 파라미터로 필요).
    private Vector3 currentVelocity;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        // 카메라의 위치/회전/FOV를 지정된 초기 상태로 맞춘다.
        transform.position = initialPosition;
        transform.eulerAngles = initialRotation;
        cam.fieldOfView = initialFieldOfView;

        zoomForward = (Quaternion.Euler(initialRotation) * Vector3.forward).normalized;

        panX = initialPosition.x;
        panZ = initialPosition.z;
        zoomHeight = initialPosition.y;

        targetPosition = initialPosition;
    }

    private void Update()
    {
        HandleZoom();
        zoomHeight = Mathf.Clamp(zoomHeight, minZoomY, maxZoomY);

        if (IsFullyZoomedOut())
        {
            // 완전 축소(줌아웃) 상태: 전체 맵이 보이므로 카메라를 초기 개요 위치(X/Z)에
            // 고정하고 WASD 입력은 무시한다.
            panX = initialPosition.x;
            panZ = initialPosition.z;
        }
        else
        {
            // 줌인해서 화면에 여유 공간이 생겼을 때만 WASD로 주변을 정찰할 수 있다.
            HandleInput();
        }

        panX = Mathf.Clamp(panX, minX, maxX);
        panZ = Mathf.Clamp(panZ, minZ, maxZ);

        HandleReset();

        targetPosition = ComputeTargetPosition();
        MoveCamera();
    }

    /// <summary>
    /// 현재 줌 높이가 Max Zoom Y(완전 축소) 근처인지 여부를 반환한다.
    /// Lerp(minZoomY, maxZoomY, panLockZoomThreshold) 지점을 넘어서면 잠금 상태로 본다.
    /// </summary>
    private bool IsFullyZoomedOut()
    {
        float lockHeight = Mathf.Lerp(minZoomY, maxZoomY, panLockZoomThreshold);
        return zoomHeight >= lockHeight;
    }

    /// <summary>
    /// WASD / 방향키 입력을 받아 팬 목표(panX/panZ)를 갱신한다.
    /// 카메라의 로컬 방향이 아니라 월드 X-Z 평면 기준으로 이동시켜, 비스듬한 각도로
    /// 내려다봐도 항상 일정한 방향(위/아래/좌/우)으로 이동하도록 한다.
    /// 회전값은 절대 변경하지 않는다.
    /// </summary>
    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D, 좌/우 방향키
        float vertical = Input.GetAxisRaw("Vertical");     // W/S, 상/하 방향키

        Vector2 inputDirection = new Vector2(horizontal, vertical);

        // 대각선 이동 시 속도가 더 빨라지지 않도록 정규화.
        if (inputDirection.sqrMagnitude > 1f)
        {
            inputDirection.Normalize();
        }

        panX += inputDirection.x * (moveSpeed * Time.deltaTime);
        panZ += inputDirection.y * (moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 마우스 휠 입력을 받아 줌 목표 높이(zoomHeight)를 갱신한다.
    /// 팬 목표(panX/panZ)에는 전혀 영향을 주지 않으므로, 줌인/줌아웃 어느 상태에서도
    /// WASD로 이동 가능한 공간(Min X~Max X, Min Z~Max Z)이 항상 동일하게 유지된다.
    /// </summary>
    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f)) return;

        // 휠을 위로 올리면(양수) 목표 높이가 낮아져 확대(줌인)된다.
        zoomHeight -= scroll * zoomSpeed;
    }

    /// <summary>
    /// 스페이스바를 누르면 팬/줌/회전/FOV를 모두 최초 카메라 뷰로 되돌린다.
    /// 위치는 SmoothDamp로 부드럽게, 회전과 FOV는 즉시 복귀시킨다.
    /// </summary>
    private void HandleReset()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            panX = initialPosition.x;
            panZ = initialPosition.z;
            zoomHeight = initialPosition.y;
            transform.eulerAngles = initialRotation;
            cam.fieldOfView = initialFieldOfView;
        }
    }

    /// <summary>
    /// 팬(panX/panZ)과 줌(zoomHeight)을 합쳐 최종 카메라 목표 위치를 계산한다.
    /// 줌은 카메라가 보는 방향(zoomForward)을 따라 전진/후진한 것처럼 보이도록,
    /// 목표 높이(zoomHeight)에 도달하는 데 필요한 forward 방향 거리(t)만큼
    /// 팬 위치에서 추가로 이동시킨다.
    /// </summary>
    private Vector3 ComputeTargetPosition()
    {
        float t = 0f;
        if (Mathf.Abs(zoomForward.y) > 0.0001f)
        {
            t = (zoomHeight - initialPosition.y) / zoomForward.y;
        }

        return new Vector3(
            panX + zoomForward.x * t,
            zoomHeight,
            panZ + zoomForward.z * t
        );
    }

    /// <summary>
    /// 현재 위치를 목표 위치로 부드럽게 이동시킨다 (관성/미끄러짐 효과).
    /// 회전값은 전혀 변경하지 않는다.
    /// </summary>
    private void MoveCamera()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

#if UNITY_EDITOR
    /// <summary>
    /// 씬 뷰에서 팬 이동 제한 범위를 시각적으로 확인할 수 있도록 기즈모를 그린다.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, transform.position.y, (minZ + maxZ) * 0.5f);
        Vector3 size = new Vector3(maxX - minX, 0.1f, maxZ - minZ);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}