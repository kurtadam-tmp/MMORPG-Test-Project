using UnityEngine;

public class MMORPGPlayerVisualizer : MonoBehaviour
{
    [Header("2.5D Isometric Movement")]
    public float MoveSpeed = 6.0f;
    public float CameraDistance = 12.0f;
    public float CameraAngleX = 45.0f;
    public float CameraAngleY = 45.0f;

    [Header("Server Connection")]
    public string ServerIp = "127.0.0.1";
    public int ServerPort = 7777;

    private MMORPGNativeClient _netClient;
    private MMORPG2DAnimationController _animController;
    private Camera _mainCamera;

    private void Start()
    {
        // Auto-attach MMORPGNativeClient if missing
        _netClient = GetComponent<MMORPGNativeClient>();
        if (_netClient == null)
        {
            _netClient = gameObject.AddComponent<MMORPGNativeClient>();
            _netClient.ServerIp = ServerIp;
            _netClient.ServerPort = ServerPort;
        }

        // Auto-attach 2.5D Animation Controller & Boss Combat System
        _animController = GetComponent<MMORPG2DAnimationController>();
        if (_animController == null)
        {
            _animController = gameObject.AddComponent<MMORPG2DAnimationController>();
        }

        if (GetComponent<MMORPGBossCombatSystem>() == null)
        {
            gameObject.AddComponent<MMORPGBossCombatSystem>();
        }

        // Configure 2.5D Isometric Camera Angle
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            GameObject camObj = new GameObject("2.5D_Isometric_Camera");
            _mainCamera = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }

        // Set Camera to Orthographic 2.5D Isometric projection
        _mainCamera.orthographic = true;
        _mainCamera.orthographicSize = 6.0f;
        _mainCamera.transform.rotation = Quaternion.Euler(CameraAngleX, CameraAngleY, 0);

        // Build 2.5D Hero Avatar representation (Stylized Quad/Sprite)
        if (transform.Find("HeroVisual") == null)
        {
            GameObject heroVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            heroVisual.name = "HeroVisual";
            heroVisual.transform.SetParent(transform);
            heroVisual.transform.localPosition = new Vector3(0, 0.75f, 0);
            heroVisual.transform.rotation = Quaternion.Euler(CameraAngleX, CameraAngleY, 0);

            Renderer rend = heroVisual.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0f, 0.95f, 0.95f);
                _animController.SpriteRenderer = rend;
                _animController.SpriteBillboard = heroVisual.transform;
            }
        }

        // Create 2.5D Tiled Floor Arena
        if (GameObject.Find("MMORPG_2.5D_Ground") == null)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "MMORPG_2.5D_Ground";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(5, 1, 5); // 50m x 50m 2.5D arena
            Renderer floorRend = floor.GetComponent<Renderer>();
            if (floorRend != null)
            {
                floorRend.material.color = new Color(0.12f, 0.18f, 0.25f);
            }
        }

        // Spawn World Boss Ignis (Lvl 50 Raid Boss) in Arena
        if (GameObject.Find("Boss_Ignis_Visualizer") == null)
        {
            GameObject bossObj = new GameObject("Boss_Ignis_Visualizer");
            bossObj.transform.position = new Vector3(12f, 0f, 12f);
            bossObj.AddComponent<MMORPGBossVisualizer>();
        }
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isAttacking = Input.GetKeyDown(KeyCode.Space);

        // Calculate 2.5D Isometric relative movement direction
        Vector3 forward = _mainCamera.transform.forward;
        Vector3 right = _mainCamera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * vertical + right * horizontal).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            transform.Translate(moveDirection * MoveSpeed * Time.deltaTime, Space.World);
        }

        // Update 2.5D 8-Way Animation State Machine
        _animController?.UpdateAnimationState(moveDirection, isAttacking);

        // Execute Boss Combat Attack on Space Keypress
        if (isAttacking)
        {
            GetComponent<MMORPGBossCombatSystem>()?.ExecuteSkillAttack();
        }

        // Smooth 2.5D Isometric Camera Follow
        if (_mainCamera != null)
        {
            Vector3 targetCamPos = transform.position - (_mainCamera.transform.forward * CameraDistance);
            _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetCamPos, Time.deltaTime * 5.0f);
        }
    }
}
