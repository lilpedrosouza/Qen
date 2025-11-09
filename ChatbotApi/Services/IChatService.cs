using ChatbotApi.Models;

namespace ChatbotApi.Services
{
    public interface IChatService
    {
        Task<ChatResponse> SendMessageAsync(ChatRequest request);
    }
}