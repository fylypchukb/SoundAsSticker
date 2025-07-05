using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SoundAsSticker;

public class TelegramBot(ILogger<TelegramBot> logger, UpdateService updateService)
{
    [Function("TelegramBot")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");
        var response = req.CreateResponse(HttpStatusCode.OK);

        try
        {
            var body = await req.ReadAsStringAsync() ?? throw new ArgumentNullException(nameof(req));
            var update = JsonSerializer.Deserialize<Update>(body, JsonBotAPI.Options);

            if (update is null)
            {
                logger.LogWarning("Unable to deserialize Update object.");
                return response;
            }

            await updateService.ProcessMessage(update);
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {Message}", e.Message);
        }

        return response;
    }
}