namespace SMS.Application.Features.Promotions.DTOs;

public class PromotionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int PromotedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
