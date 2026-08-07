using UnityEngine;

public class MMORPGPlayerVisualizer : MonoBehaviour
{
    [Header("2.5D Isometric Movement")]
    public float MoveSpeed = 6.0f;
    public float CameraDistance = 12.0f;
    public float CameraAngleX = 45.0f;
    public float CameraAngleY = 45.0f;

    [Header("Character Sınıf Görsel Ayarları")]
    public string CharacterClass = "Warrior"; // Warrior, Mage, Rogue, Priest, Paladin, Necromancer

    [Header("Server Connection")]
    public string ServerIp = "127.0.0.1";
    public int ServerPort = 7777;

    private MMORPGNativeClient _netClient;
    private MMORPG2DAnimationController _animController;
    private MMORPGCombatSystem _combatSystem;
    private Camera _mainCamera;
    private Renderer _heroRenderer;

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

        // Auto-attach 2.5D Animation Controller & Universal Combat System
        _animController = GetComponent<MMORPG2DAnimationController>();
        if (_animController == null)
        {
            _animController = gameObject.AddComponent<MMORPG2DAnimationController>();
        }

        _combatSystem = GetComponent<MMORPGCombatSystem>();
        if (_combatSystem == null)
        {
            _combatSystem = gameObject.AddComponent<MMORPGCombatSystem>();
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

            _heroRenderer = heroVisual.GetComponent<Renderer>();
            if (_heroRenderer != null)
            {
                ApplyClassColorTint(CharacterClass);
                _animController.SpriteRenderer = _heroRenderer;
                _animController.SpriteBillboard = heroVisual.transform;
            }
        }

        // Create Class Aura Indicator under Feet
        CreateClassAuraCircle();

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

        // Spawn Test Mobs in Arena
        SpawnArenaTestMobs();
    }

    public void ApplyClassColorTint(string className)
    {
        CharacterClass = className;
        if (_heroRenderer == null) return;

        Color classColor = className.ToLowerInvariant() switch
        {
            "warrior" => new Color(0.9f, 0.2f, 0.2f),    // Crimson Red
            "mage" => new Color(0.1f, 0.6f, 1.0f),       // Arcane Blue
            "rogue" => new Color(0.1f, 0.9f, 0.3f),      // Poison Green
            "priest" => new Color(1.0f, 0.9f, 0.5f),     // Holy Gold
            "paladin" => new Color(1.0f, 0.8f, 0.1f),    // Platinum Gold
            "necromancer" => new Color(0.6f, 0.1f, 0.9f),// Shadow Purple
            _ => new Color(0.0f, 0.95f, 0.95f)
        };

        _heroRenderer.material.color = classColor;
    }

    private void CreateClassAuraCircle()
    {
        GameObject aura = GameObject.CreatePrimitive(PrimitiveType.Quad);
        aura.name = "ClassAuraCircle";
        aura.transform.SetParent(transform);
        aura.transform.localPosition = new Vector3(0, 0.02f, 0);
        aura.transform.rotation = Quaternion.Euler(90, 0, 0);
        aura.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        Renderer r = aura.GetComponent<Renderer>();
        if (r != null)
        {
            r.material.color = new Color(1f, 1f, 1f, 0.25f);
        }
        Destroy(aura.GetComponent<Collider>());
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

        // Execute Universal Combat Attack on Space Keypress
        if (isAttacking)
        {
            _combatSystem?.ExecuteAttack();
            VFXManager.Instance?.SpawnVFX("VFX_Slash", transform.position + transform.forward * 1.2f, Quaternion.identity);
        }

        // Smooth 2.5D Isometric Camera Follow (45-degree angle offset)
        if (_mainCamera != null)
        {
            _mainCamera.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            Vector3 targetCamPos = transform.position + new Vector3(-8f, 12f, -8f);
            _mainCamera.transform.position = Vector3.Lerp(_mainCamera.transform.position, targetCamPos, Time.deltaTime * 5.0f);
        }
    }

    private void SpawnArenaTestMobs()
    {
        if (GameObject.Find("TestMobsParent") != null) return;

        GameObject mobsParent = new GameObject("TestMobsParent");

        // Mob #1: Forest Goblin (Lvl 1 - 150 HP)
        CreateMob(mobsParent.transform, "Forest Goblin", 1, 150, new Vector3(4f, 0f, 4f), new Color(0.2f, 0.8f, 0.2f));

        // Mob #2: Wild Wolf (Lvl 3 - 300 HP)
        CreateMob(mobsParent.transform, "Wild Wolf", 3, 300, new Vector3(-5f, 0f, 6f), new Color(0.8f, 0.5f, 0.2f));

        // Mob #3: Skeleton Warrior (Lvl 5 - 550 HP)
        CreateMob(mobsParent.transform, "Skeleton Warrior", 5, 550, new Vector3(8f, 0f, -4f), new Color(0.7f, 0.7f, 0.8f));
    }

    private void CreateMob(Transform parent, string name, int level, int maxHp, Vector3 pos, Color color)
    {
        GameObject mobObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        mobObj.name = name;
        mobObj.transform.SetParent(parent);
        mobObj.transform.position = pos + Vector3.up * 0.75f;
        mobObj.transform.rotation = Quaternion.Euler(CameraAngleX, CameraAngleY, 0);

        UniversalCombatEntity entity = mobObj.AddComponent<UniversalCombatEntity>();
        entity.Name = name;
        entity.Level = level;
        entity.MaxHealth = maxHp;
        entity.CurrentHealth = maxHp;
        entity.ColorTint = color;
    }
}
