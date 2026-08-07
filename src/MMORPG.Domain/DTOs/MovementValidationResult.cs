namespace MMORPG.Domain.DTOs;

public class MovementValidationResult
{
    public bool IsValid { get; set; }
    public bool IsRubberbandTriggered { get; set; }
    public float CorrectedX { get; set; }
    public float CorrectedY { get; set; }
    public float CorrectedZ { get; set; }
    public long SequenceId { get; set; }
    public string Message { get; set; } = string.Empty;
}
