using Gplant.Application.AIPlugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Runtime.CompilerServices;
using System.Text;

namespace Gplant.Application.Services
{
    public class AIChatService(IServiceProvider serviceProvider)
    {
        private readonly ChatHistory chatHistory = new(
            """
                Bạn là AI Assistant của Gplant - shop cây cảnh online.
                Trả lời thân thiện bằng tiếng Việt.

                Khi tool trả về dữ liệu dạng TOOL_DATA:TYPE:JSON, hãy nhúng vào câu trả lời theo format:
                [[TYPE:JSON]]

                Ví dụ tool trả về: TOOL_DATA:ORDER_LIST:{"orders":[...]}
                Thì câu trả lời phải là:
                "Đây là danh sách đơn hàng của bạn: [[ORDER_LIST:{"totalCount":2,"orders":[...]}]] Bạn cần hỗ trợ gì thêm không?"

                Các component hỗ trợ:
                - [[ORDER_LIST:{"totalCount":N,"orders":[{"orderNumber":"...","status":"...","paymentStatus":"...","total":N,"createdAt":"...","itemCount":N}]}]]
                - [[ORDER_CARD:{"orderNumber":"...","status":"...","paymentStatus":"...","total":N,"createdAt":"..."}]]
                - [[PLANT_LIST:{"plants":[{"id":"...","name":"...","price":N,"category":"..."}]}]]

                Quan trọng: Chỉ dùng [[...]] khi có TOOL_DATA thực. Không bịa dữ liệu.
            """
        );

        public async IAsyncEnumerable<string> ChatStreamAsync(
            string userMessage,
            Guid userId,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var kernel      = BuildKernel(userId);
            var chatService = kernel.GetRequiredService<IChatCompletionService>();

            chatHistory.AddUserMessage(userMessage);

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                Temperature = 0.3
            };

            var fullResponse = new StringBuilder();

            await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken))
            {
                if (string.IsNullOrEmpty(chunk.Content)) continue;
                fullResponse.Append(chunk.Content);
                yield return chunk.Content;
            }

            chatHistory.AddAssistantMessage(fullResponse.ToString());
        }

        private Kernel BuildKernel(Guid userId)
        {
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: "gemma4:e2b",
                apiKey: "ollama",
                endpoint: new Uri("http://localhost:11434/v1/")
            );

            var orderPlugin = serviceProvider.GetRequiredService<OrderPlugin>();
            var plantPlugin = serviceProvider.GetRequiredService<PlantPlugin>();

            orderPlugin.SetUserId(userId);

            builder.Plugins.AddFromObject(plantPlugin, "PlantTools");
            builder.Plugins.AddFromObject(orderPlugin, "OrderTools");

            return builder.Build();
        }
    }
}