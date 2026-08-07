using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HUDUIController : MonoBehaviour
{
    public static HUDUIController Instance { get; private set; }

    [Header("Player Vitals UI")]
    public Image HealthBarFill;
    public Image ManaBarFill;
    public Text HealthText;
    public Text GoldText;
    public Text LevelText;

    [Header("Chat UI")]
    public Text ChatHistoryText;
    public InputField ChatInputField;

    private readonly StringBuilder _chatBuffer = new StringBuilder();

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateVitals(int currentHp, int maxHp, int currentMp, int maxMp, int level, long gold)
    {
        if (HealthBarFill != null) HealthBarFill.fillAmount = (float)currentHp / maxHp;
        if (ManaBarFill != null) ManaBarFill.fillAmount = (float)currentMp / maxMp;
        if (HealthText != null) HealthText.text = $"{currentHp} / {maxHp}";
        if (GoldText != null) GoldText.text = $"{gold:N0} Gold";
        if (LevelText != null) LevelText.text = $"Lvl {level}";

        InventoryGridUIController.Instance?.UpdateGold(gold);
    }

    public void AppendChatMessage(string sender, string message)
    {
        _chatBuffer.AppendLine($"<b>[{sender}]</b>: {message}");
        if (ChatHistoryText != null)
        {
            ChatHistoryText.text = _chatBuffer.ToString();
        }
    }

    public void OnSubmitChat()
    {
        if (ChatInputField != null && !string.IsNullOrWhiteSpace(ChatInputField.text))
        {
            MMORPGNativeClient.Instance?.SendChatMessage(ChatInputField.text);
            AppendChatMessage("YOU", ChatInputField.text);
            ChatInputField.text = string.Empty;
        }
    }
}
