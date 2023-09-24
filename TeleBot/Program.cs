using System.Text.Json;
using TeleBot.Models;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TeleBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var tokenPath = System.IO.File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "Token.json"));
            var tokenModel = JsonSerializer.Deserialize<TokenModel>(tokenPath);
            var botClient = new TelegramBotClient(tokenModel.Token);

            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.WriteLine($"Type \"stop\" to stop the bot");
            while (Console.ReadLine().ToLower() != "stop") { }
            // Send cancellation request to stop bot
            cts.Cancel();
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            UpdateType messageType = update.Type;

            switch (messageType)
            {
                case UpdateType.Message:
                    await CreateMessageResponse(botClient, update, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    await CreateallbackQueryResponse(botClient, update, cancellationToken);
                    break;
            }
        }

        private static async Task CreateallbackQueryResponse(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery.Data == "where")
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "Купить по адресу: ул Где то там",
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
            }
            if (update.CallbackQuery.Data == "buy")
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "Наличкой конечно, что за вопрос?",
                    parseMode: ParseMode.MarkdownV2,
                    disableNotification: true,
                    cancellationToken: cancellationToken);
            }
            if (update.CallbackQuery.Data == "disconnect")
            {
                Message sentMessage = await botClient.SendStickerAsync(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    sticker: InputFile.FromUri(
                        "https://chpic.su/_data/stickers/o/OkeyStickersss/OkeyStickersss_011.webp"),
                    cancellationToken: cancellationToken);
            }
        }

        private static async Task CreateMessageResponse(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Пока наш бот молчун и не отвечает ни на какие вопросы
            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            if (messageText == "/start")
            {
                var text = "Привет! Что надо?";
                var ikm = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Где купить?", "where"),
                        InlineKeyboardButton.WithCallbackData("Как купить?", "buy"),
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Ничего не надо.", "disconnect"),
                    },
                });
                await botClient.SendTextMessageAsync(chatId, text, replyMarkup: ikm, cancellationToken: cancellationToken);
                return;
            }
            else return;

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Ответ бота",
                parseMode: ParseMode.MarkdownV2,
                disableNotification: true,
                replyToMessageId: update.Message.MessageId,
                // Вставить ссылку в ответ бота
                //replyMarkup: new InlineKeyboardMarkup(
                //    InlineKeyboardButton.WithUrl(
                //        text: "Check sendMessage method",
                //        url: "https://core.telegram.org/bots/api#sendmessage")),
                cancellationToken: cancellationToken);
        }
    }
}