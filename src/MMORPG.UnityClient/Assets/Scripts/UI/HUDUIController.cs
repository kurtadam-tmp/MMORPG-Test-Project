using System;
using System.Collections.Generic;
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

    [Header("Target Frame UI")]
    public GameObject TargetFramePanel;
    public Text TargetNameText;
    public Image TargetHealthBarFill;
    public Text TargetHealthText;
    public Text TargetLevelText;
    public GameObject BossBadgeIcon;

    [Header("Cast Bar & Zone Status")]
    public GameObject CastBarPanel;
    public Image CastBarFill;
    public Text CastSpellNameText;
    public Text ZoneNameText;
    public Text WeatherText;

    [Header("Skill Hotbar UI")]
    public Image[] SkillCooldownOverlayFills = new Image[4];
    public Text[] SkillCooldownTexts = new Text[4];
    public KeyCode[] Hotkeys = new KeyCode[4] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

    [Header("Chat UI")]
    public Text ChatHistoryText;
    public InputField ChatInputField;

    private readonly StringBuilder _chatBuffer = new StringBuilder();
    private float[] _cooldownTimers = new float[4];
    private float[] _cooldownDurations = new float[4] { 4.0f, 8.0f, 15.0f, 45.0f };

    private bool _isCasting = false;
    private float _castTimer = 0f;
    private float _totalCastTime = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Handle Skill Hotkeys (1, 2, 3, 4)
        for (int i = 0; i < Hotkeys.Length; i++)
        {
            if (Input.GetKeyDown(Hotkeys[i]))
            {
                TriggerSkillHotkey(i + 1);
            }

            // Update Cooldown Visual Overlays
            if (_cooldownTimers[i] > 0)
            {
                _cooldownTimers[i] -= Time.deltaTime;
                if (SkillCooldownOverlayFills != null && i < SkillCooldownOverlayFills.Length && SkillCooldownOverlayFills[i] != null)
                {
                    SkillCooldownOverlayFills[i].fillAmount = Mathf.Clamp01(_cooldownTimers[i] / _cooldownDurations[i]);
                }
                if (SkillCooldownTexts != null && i < SkillCooldownTexts.Length && SkillCooldownTexts[i] != null)
                {
                    SkillCooldownTexts[i].text = $"{_cooldownTimers[i]:F1}s";
                }
            }
            else
            {
                if (SkillCooldownOverlayFills != null && i < SkillCooldownOverlayFills.Length && SkillCooldownOverlayFills[i] != null)
                {
                    SkillCooldownOverlayFills[i].fillAmount = 0f;
                }
                if (SkillCooldownTexts != null && i < SkillCooldownTexts.Length && SkillCooldownTexts[i] != null)
                {
                    SkillCooldownTexts[i].text = string.Empty;
                }
            }
        }

        // Update Cast Bar Progress
        if (_isCasting)
        {
            _castTimer += Time.deltaTime;
            if (CastBarFill != null) CastBarFill.fillAmount = Mathf.Clamp01(_castTimer / _totalCastTime);

            if (_castTimer >= _totalCastTime)
            {
                _isCasting = false;
                if (CastBarPanel != null) CastBarPanel.SetActive(false);
                AppendChatMessage("SYSTEM", "Cast Completed!");
            }
        }
    }

    public void TriggerSkillHotkey(int skillSlot)
    {
        int index = skillSlot - 1;
        if (index < 0 || index >= _cooldownTimers.Length) return;

        if (_cooldownTimers[index] > 0)
        {
            AppendChatMessage("SYSTEM", $"Skill #{skillSlot} is on Cooldown ({_cooldownTimers[index]:F1}s remaining)!");
            return;
        }

        // Trigger Cooldown
        _cooldownTimers[index] = _cooldownDurations[index];

        // Simulate Spell Casting Time
        StartCastingSkill($"Skill #{skillSlot}", castTimeSeconds: 1.5f);

        MMORPGNativeClient.Instance?.SendUseSkill(skillSlot);
    }

    public void StartCastingSkill(string spellName, float castTimeSeconds)
    {
        _isCasting = true;
        _castTimer = 0f;
        _totalCastTime = castTimeSeconds;

        if (CastBarPanel != null) CastBarPanel.SetActive(true);
        if (CastSpellNameText != null) CastSpellNameText.text = spellName;
    }

    public void InterruptCasting(string reason)
    {
        if (_isCasting)
        {
            _isCasting = false;
            if (CastBarPanel != null) CastBarPanel.SetActive(false);
            AppendChatMessage("SYSTEM", $"<color=red>CAST INTERRUPTED ({reason})!</color>");
        }
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

    public void UpdateTargetFrame(string targetName, int currentHp, int maxHp, int level, bool isBoss)
    {
        if (TargetFramePanel != null) TargetFramePanel.SetActive(true);
        if (TargetNameText != null) TargetNameText.text = targetName;
        if (TargetHealthBarFill != null) TargetHealthBarFill.fillAmount = (float)currentHp / maxHp;
        if (TargetHealthText != null) TargetHealthText.text = $"{currentHp} / {maxHp}";
        if (TargetLevelText != null) TargetLevelText.text = $"Lvl {level}";
        if (BossBadgeIcon != null) BossBadgeIcon.SetActive(isBoss);
    }

    public void UpdateZoneStatus(string zoneName, string weatherName)
    {
        if (ZoneNameText != null) ZoneNameText.text = zoneName;
        if (WeatherText != null) WeatherText.text = $"Hava: {weatherName}";
    }

    public void ClearTargetFrame()
    {
        if (TargetFramePanel != null) TargetFramePanel.SetActive(false);
    }

    public enum ChatChannel
    {
        Global,
        Zone,
        Guild,
        Whisper,
        System
    }

    public void AppendChatMessage(string sender, string message, ChatChannel channel = ChatChannel.Global)
    {
        string colorTag = channel switch
        {
            ChatChannel.Global => "#00f2fe",  // Neon Cyan
            ChatChannel.Zone => "#ffd700",    // Gold Yellow
            ChatChannel.Guild => "#00e676",   // Emerald Green
            ChatChannel.Whisper => "#ff4081", // Pink Magenta
            ChatChannel.System => "#ffab00",  // Amber Orange
            _ => "#ffffff"
        };

        string prefix = channel switch
        {
            ChatChannel.Global => "[GLOBAL]",
            ChatChannel.Zone => "[ZONE]",
            ChatChannel.Guild => "[GUILD]",
            ChatChannel.Whisper => "[WHISPER]",
            ChatChannel.System => "[SYSTEM]",
            _ => "[CHAT]"
        };

        _chatBuffer.AppendLine($"<color={colorTag}><b>{prefix} [{sender}]</b>: {message}</color>");
        if (ChatHistoryText != null)
        {
            ChatHistoryText.text = _chatBuffer.ToString();
        }
    }

    public void OnSubmitChat()
    {
        if (ChatInputField != null && !string.IsNullOrWhiteSpace(ChatInputField.text))
        {
            string rawInput = ChatInputField.text.Trim();
            ChatChannel channel = ChatChannel.Global;
            string message = rawInput;

            if (rawInput.StartsWith("/g ") || rawInput.StartsWith("/guild "))
            {
                channel = ChatChannel.Guild;
                message = rawInput.Substring(rawInput.IndexOf(' ') + 1);
            }
            else if (rawInput.StartsWith("/z ") || rawInput.StartsWith("/zone "))
            {
                channel = ChatChannel.Zone;
                message = rawInput.Substring(rawInput.IndexOf(' ') + 1);
            }
            else if (rawInput.StartsWith("/w ") || rawInput.StartsWith("/whisper "))
            {
                channel = ChatChannel.Whisper;
                message = rawInput.Substring(rawInput.IndexOf(' ') + 1);
            }

            MMORPGNativeClient.Instance?.SendChatMessage(message);
            AppendChatMessage("YOU", message, channel);
            ChatInputField.text = string.Empty;
        }
    }
}
