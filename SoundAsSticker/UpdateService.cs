using Microsoft.Extensions.Logging;
using SoundAsSticker.Constants;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SoundAsSticker;

public class UpdateService(ITelegramBotClient botClient, ILogger<UpdateService> logger)
{
    private static readonly Dictionary<long, UserState> UserStates = new();

    public Task ProcessMessage(Update update)
    {
        if (update is not { Message: { } message }) return Task.CompletedTask;

        if (UserStates.TryGetValue(message.Chat.Id, out var state) && state == UserState.AwaitingAudio)
        {
            return HandleAudioMessage(message);
        }

        return message.Text?.Split(' ', StringSplitOptions.TrimEntries)[0] switch
        {
            CommandsList.StartCommand => InitMenu(),
            CommandsList.AddStickerCommand => AddSticker(message.Chat.Id),
            _ => Task.CompletedTask
        };
    }

    private Task InitMenu()
    {
        return botClient.SetMyCommands([
                new BotCommand(CommandsList.StartCommand, "Start a bot."),
                new BotCommand(CommandsList.AddStickerCommand, "Add a sound sticker.")
            ],
            BotCommandScope.Default());
    }

    private async Task AddSticker(long chatId)
    {
        logger.LogInformation("User entered /add command");
        UserStates[chatId] = UserState.AwaitingAudio;

        await botClient.SendMessage(chatId, "Please send an audio or voice message.");
    }

    private async Task HandleAudioMessage(Message message)
    {
        var chatId = message.Chat.Id;

        if (message.Audio is not null || message.Voice is not null)
        {
            logger.LogInformation("Received audio or voice");

            await botClient.SendMessage(chatId, "What a nice audio!");
        }
        else
        {
            logger.LogInformation("Expected audio, but got something else");
            await botClient.SendMessage(chatId, "I was waiting for an audio or voice message.");
        }

        UserStates.Remove(chatId);
    }
}