using System.ComponentModel.DataAnnotations;


namespace ChatbotApi.Models
{
    public class ChatResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ConversationId { get; set; }
        public string? Error { get; set; }
        public int TokensUsed { get; set; }
    }
}