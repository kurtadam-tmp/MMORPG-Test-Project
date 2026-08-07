using System.Collections;
using UnityEngine;

public class ZonePortalVisualizer : MonoBehaviour
{
    [Header("Portal Settings")]
    public string PortalId = "portal_101";
    public int SourceZoneId = 1;
    public int TargetZoneId = 2;
    public string DestinationName = "Shadowfen Swamps";
    public int RequiredLevel = 10;
    public Color PortalGlowColor = new Color(0f, 0.9f, 1f, 0.7f); // Neon Cyan

    [Header("Visual Elements")]
    public float PulseSpeed = 2.0f;
    public float ScalePulseAmount = 0.15f;
    public TextMesh PortalLabelTextMesh;

    private Transform _portalMeshTransform;
    private Vector3 _baseScale;
    private Transform _mainCamTransform;
    private bool _isTraversing = false;

    private void Start()
    {
        if (Camera.main != null)
        {
            _mainCamTransform = Camera.main.transform;
        }

        // Build 2.5D Glowing Portal Arch/Ring (Ground Quad 2.0m x 2.0m)
        GameObject portalObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        portalObj.name = "PortalGlowRing";
        portalObj.transform.SetParent(transform);
        portalObj.transform.localPosition = new Vector3(0, 0.05f, 0);
        portalObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        portalObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
        _portalMeshTransform = portalObj.transform;
        _baseScale = _portalMeshTransform.localScale;

        Renderer rend = portalObj.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = PortalGlowColor;
        }
        Destroy(portalObj.GetComponent<Collider>());

        // Add Trigger Sphere Collider for Player Detection (3m radius)
        SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 1.5f;

        // Build Floating Portal Destination Title Label
        CreatePortalLabel3D();
    }

    private void CreatePortalLabel3D()
    {
        GameObject textObj = new GameObject("PortalDestinationTextMesh");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2.2f, 0);
        if (_mainCamTransform != null) textObj.transform.rotation = _mainCamTransform.rotation;

        PortalLabelTextMesh = textObj.AddComponent<TextMesh>();
        PortalLabelTextMesh.fontSize = 22;
        PortalLabelTextMesh.characterSize = 0.14f;
        PortalLabelTextMesh.alignment = TextAlignment.Center;
        PortalLabelTextMesh.anchor = TextAnchor.MiddleCenter;
        PortalLabelTextMesh.color = Color.cyan;
        PortalLabelTextMesh.text = $"🌀 <b>{DestinationName}</b>\n<color=yellow>[Zone #{TargetZoneId}] - Min Lvl {RequiredLevel}</color>";
    }

    private void Update()
    {
        // Pulse Portal Visual Ring
        if (_portalMeshTransform != null)
        {
            float scaleFactor = 1.0f + Mathf.Sin(Time.time * PulseSpeed) * ScalePulseAmount;
            _portalMeshTransform.localScale = _baseScale * scaleFactor;
        }

        // Billboard Destination Label towards 2.5D Camera
        if (PortalLabelTextMesh != null && _mainCamTransform != null)
        {
            PortalLabelTextMesh.transform.rotation = _mainCamTransform.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isTraversing) return;

        MMORPGPlayerVisualizer player = other.GetComponent<MMORPGPlayerVisualizer>();
        if (player != null)
        {
            StartCoroutine(InitiatePortalTraverse(player));
        }
    }

    private IEnumerator InitiatePortalTraverse(MMORPGPlayerVisualizer player)
    {
        _isTraversing = true;
        HUDUIController.Instance?.AppendChatMessage("PORTAL", $"Traversing Portal to <b>{DestinationName}</b> (Zone #{TargetZoneId})...");

        // Play Warp FX & Sound
        VFXManager.Instance?.SpawnVFX("VFX_PortalWarp", transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.0f);

        HUDUIController.Instance?.AppendChatMessage("SYSTEM", $"<color=green>Warp Successful! Welcome to {DestinationName} (Zone #{TargetZoneId}).</color>");
        HUDUIController.Instance?.UpdateZoneStatus(DestinationName, "Clear");

        _isTraversing = false;
    }
}
