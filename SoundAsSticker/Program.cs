using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoundAsSticker;
using Telegram.Bot;

var tgToken = "<TOKEN HERE>";

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddHttpClient("tgclient")
            .AddTypedClient<ITelegramBotClient>(client => new TelegramBotClient(tgToken, client));

        services.AddScoped<UpdateService>();
    })
    .Build();

host.Run();