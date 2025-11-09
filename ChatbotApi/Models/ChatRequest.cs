using System.ComponentModel.DataAnnotations;

namespace ChatbotApi.Models
{
    public class ChatRequest
    {
        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(4000, ErrorMessage = "A mensagem não pode exceder 4000 caracteres.")]
        public string Message { get; set; } = string.Empty;

        public string? ConversationId { get; set; }
    }
}