using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUIController : MonoBehaviour
{
    public static MinimapUIController Instance { get; private set; }

    [Header("Minimap References")]
    public Camera MinimapCamera;
    public RawImage MinimapRenderImage;
    public Text MapTitleText;
    public Text CoordinatesText;

    [Header("Zoom Settings")]
    public float CurrentZoom = 15.0f;
    public float MinZoom = 8.0f;
    public float MaxZoom = 30.0f;

    private Transform _playerTransform;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Configure Minimap Camera only if explicitly assigned with a Render Texture
        if (MinimapCamera != null && MinimapCamera.targetTexture != null)
        {
            MinimapCamera.orthographic = true;
            MinimapCamera.orthographicSize = CurrentZoom;
            MinimapCamera.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
        else if (MinimapCamera != null)
        {
            // Disable camera so it never overrides Main Camera 2.5D view
            MinimapCamera.enabled = false;
        }

        UpdateMapTitle("Whisperwood Glen (Zone #1)");
    }

    private void Update()
    {
        if (_playerTransform == null)
        {
            MMORPGPlayerVisualizer p = FindFirstObjectByType<MMORPGPlayerVisualizer>();
            if (p != null) _playerTransform = p.transform;
        }

        if (_playerTransform != null && MinimapCamera != null && MinimapCamera.enabled)
        {
            // Follow Player Position Top-Down
            MinimapCamera.transform.position = new Vector3(_playerTransform.position.x, 30f, _playerTransform.position.z);

            // Update Coordinates Text
            if (CoordinatesText != null)
            {
                CoordinatesText.text = $"X: {_playerTransform.position.x:F1} | Z: {_playerTransform.position.z:F1}";
            }
        }
    }

    public void UpdateMapTitle(string title)
    {
        if (MapTitleText != null)
        {
            MapTitleText.text = title;
        }
    }

    public void OnClickZoomIn()
    {
        CurrentZoom = Mathf.Clamp(CurrentZoom - 3.0f, MinZoom, MaxZoom);
        if (MinimapCamera != null) MinimapCamera.orthographicSize = CurrentZoom;
    }

    public void OnClickZoomOut()
    {
        CurrentZoom = Mathf.Clamp(CurrentZoom + 3.0f, MinZoom, MaxZoom);
        if (MinimapCamera != null) MinimapCamera.orthographicSize = CurrentZoom;
    }
}
