using System;
using UnityEngine;
using UnityEngine.UI;

public class TargetContextMenuUIController : MonoBehaviour
{
    public static TargetContextMenuUIController Instance { get; private set; }

    [Header("Context Menu UI Panel")]
    public GameObject ContextMenuPanel;
    public Text TargetNameTitleText;

    private string _activeTargetName = string.Empty;
    private Guid _activeTargetId;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (ContextMenuPanel != null) ContextMenuPanel.SetActive(false);
    }

    private void Update()
    {
        // Right-Click Player Detection
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                UniversalCombatEntity entity = hit.collider.GetComponent<UniversalCombatEntity>();
                if (entity != null)
                {
                    OpenContextMenu(entity.Name, Guid.NewGuid(), Input.mousePosition);
                }
            }
        }

        // Close on Left-Click outside
        if (Input.GetMouseButtonDown(0) && ContextMenuPanel != null && ContextMenuPanel.activeSelf)
        {
            // Close context menu if not clicking on context menu panel
            Invoke(nameof(CloseContextMenu), 0.1f);
        }
    }

    public void OpenContextMenu(string targetName, Guid targetId, Vector3 screenPos)
    {
        _activeTargetName = targetName;
        _activeTargetId = targetId;

        if (ContextMenuPanel != null)
        {
            ContextMenuPanel.SetActive(true);
            ContextMenuPanel.transform.position = screenPos;
        }

        if (TargetNameTitleText != null)
        {
            TargetNameTitleText.text = targetName;
        }

        HUDUIController.Instance?.UpdateTargetFrame(targetName, 1000, 1000, 60, isBoss: false);
    }

    public void CloseContextMenu()
    {
        if (ContextMenuPanel != null)
        {
            ContextMenuPanel.SetActive(false);
        }
    }

    public void OnClickTradeRequest()
    {
        HUDUIController.Instance?.AppendChatMessage("TRADE", $"<color=cyan>{_activeTargetName} oyuncusuna Ticaret İsteği gönderildi...</color>");
        CloseContextMenu();
    }

    public void OnClickPartyInvite()
    {
        HUDUIController.Instance?.AppendChatMessage("PARTY", $"<color=green>{_activeTargetName} oyuncusu Partiye Davet Edildi!</color>");
        CloseContextMenu();
    }

    public void OnClickDuelRequest()
    {
        HUDUIController.Instance?.AppendChatMessage("PVP", $"<color=red>⚔️ {_activeTargetName} oyuncusuna 1v1 Düello Meydan Okuması gönderildi!</color>");
        CloseContextMenu();
    }

    public void OnClickInspectGear()
    {
        HUDUIController.Instance?.AppendChatMessage("INSPECT", $"<color=gold>🔍 {_activeTargetName} oyuncusunun Ekipmanları İnceleme Penceresinde Açıldı.</color>");
        CloseContextMenu();
    }

    public void OnClickSendWhisper()
    {
        HUDUIController.Instance?.AppendChatMessage("SYSTEM", $"Fısıldamak için sohbet kutusuna <b>/w {_activeTargetName} mesajınız</b> yazın.");
        CloseContextMenu();
    }
}
