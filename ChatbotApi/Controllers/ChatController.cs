using ChatbotApi.Models;
using ChatbotApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]

    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        /// <summary>
        /// Envia uma mensagem para o chatbot e recebe uma resposta
        /// </summary>
        /// <param name="request">Objeto contendo a mensagem do usuário e opcionalmente o ID da conversa</param>
        /// <returns>Resposta do chatbot com a mensagem gerada pela IA</returns>
        /// <response code="200">Mensagem processada com sucesso</response>
        /// <response code="400">Requisição inválida (mensagem vazia ou muito longa)</response>
        /// <response code="500">Erro interno ao processar a mensagem</response>

        [HttpPost("message")]
        [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Requisição inválida: {Errors}",
                string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Recebendo mensagem: {Message} | ConversationId: {ConversationId}",
                request.Message, request.ConversationId ?? "Nova Conversa");

            try
            {
                var response = await _chatService.SendMessageAsync(request);

                if (!response.Success)
                {
                    _logger.LogError("Erro ao processar a mensagem: {Error}", response.Error);
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
                }
                _logger.LogInformation("Resposta gerada com sucesso. Tokens usados: {TokensUsed}",
                    response.TokensUsed);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exceção ao processar a mensagem do chat.");
                return StatusCode(StatusCodes.Status500InternalServerError, new ChatResponse
                {
                    Success = false,
                    Message = string.Empty,
                    Error = "Ocorreu um erro ao processar sua solicitação.",
                });
            }
        }
        /// <summary>
        /// Limpa o histórico de uma conversa específica
        /// </summary>
        /// <param name="conversationId">ID da conversa a ser limpa</param>
        /// <returns>Confirmação da limpeza</returns>
        /// <response code="200">Conversa limpa com sucesso</response>
        /// <response code="404">Conversa não encontrada</response>

        [HttpDelete("conversation/{conversationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ClearConversation(string conversationId)
        {
            if (string.IsNullOrWhiteSpace(conversationId))
            {
                return BadRequest(new { message = "ID da conversa inválido." });
            }

            _logger.LogInformation("Solicitação para limpar conversa com ID: {ConversationId}", conversationId);

            return Ok(new
            {
                message = "Conversa limpa com sucesso (simulado).",
                conversationId = conversationId,
                clearedAt = DateTime.UtcNow
            });
        }
        /// <summary>
        /// Endpoint de health check para verificar se a API está funcionando
        /// </summary>
        /// <returns>Status da API</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                status = "API está funcionando corretamente.",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}