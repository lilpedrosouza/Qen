using ChatbotApi.Models;
using ChatbotApi.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace ChatbotApi.Services
{
    public class ChatService : IChatService
    {
        private readonly ChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;
        private static readonly Dictionary<string, List<ChatMessage>> _conversations = new();

        public ChatService(IOptions<OpenAISettings> settings, ILogger<ChatService> logger)
        {
            _logger = logger;

            _chatClient = new ChatClient(
                model: settings.Value.Model,
                apiKey: settings.Value.ApiKey
            );
        }

        public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
        {
            try
            {
                var conversationId = request.ConversationId ?? Guid.NewGuid().ToString();

                if (!_conversations.ContainsKey(conversationId))
                {
                    _conversations[conversationId] = new List<ChatMessage>
                    {
                        new SystemChatMessage("Você é um assistente útil e amigável.")
                    };
                }

                _conversations[conversationId].Add(new UserChatMessage(request.Message));

                var completion = await _chatClient.CompleteChatAsync(_conversations[conversationId]);

                var assistantMessage = completion.Value.Content[0].Text;

                _conversations[conversationId].Add(new AssistantChatMessage(assistantMessage));

                return new ChatResponse
                {
                    Success = true,
                    Message = assistantMessage,
                    ConversationId = conversationId,
                    TokensUsed = completion.Value.Usage.TotalTokenCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a mensagem do chat.");

                return new ChatResponse
                {
                    Success = false,
                    Message = string.Empty,
                    Error = "Ocorreu um erro ao processar sua solicitação.",
                };
            }
        }

    }
}
