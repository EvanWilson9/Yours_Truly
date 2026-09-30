using Google.GenAI;
using Google.GenAI.Types;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using MimeKit;
using RelationshipWorkerService.Entities;
using RelationshipWorkerService.Entities.Enums;
using RelationshipWorkerService.Tools;

namespace RelationshipWorkerService
{
    public class EmailSystem
    {
        private readonly EmailSettings _emailSettings;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;
        FileLogger fileLogger = new FileLogger();
        private static readonly List<User> UsersList = new List<User>();
        private DateTime todayUtc => DateTime.SpecifyKind(DateTime.Today.ToUniversalTime(), DateTimeKind.Utc);
        private DateTime tomorrowUtc => todayUtc.AddDays(1);

        // Dependency Injection
        public EmailSystem(IDbContextFactory<AppDbContext> contextFactory, IOptions<EmailSettings> emailSettings)
        {
            _contextFactory = contextFactory;
            _emailSettings = emailSettings.Value;
        }

        // System Process
        public async Task RunEmailSystem(CancellationToken cancellationToken = default)
        {
            if (await IsDailyPromptComplete(cancellationToken))
            {
                fileLogger.writeToLog($"[Daily session complete] ({todayUtc}, {tomorrowUtc}");
                return;
            }

            await GetUsers(cancellationToken);
            await CheckForTodaysPrompt(cancellationToken);
            await CheckSendPromptEmail(cancellationToken);
            await CheckForResponseEmails(cancellationToken);
            await SendUserResponses(cancellationToken);
        }

        private async Task<bool> IsDailyPromptComplete(CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var isDailyPromptComplete = await context.Prompts.AnyAsync(
                p => p.CreatedAt >= todayUtc && p.CreatedAt <= tomorrowUtc && p.IsComplete == true
            );

            return isDailyPromptComplete;
        }

        private async Task SendUserResponses(CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            foreach (User user in UsersList)
            {
                //fileLogger.writeToLog($"{user.Name}, {user.Id}");
                if (!await DoesPromptResponseExists(user, true, cancellationToken))
                {
                    return;
                }
            }

            MimeMessage message;
            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, cancellationToken);
            User otherUser = null!;
            User currentUser = null!;

            var prompt = await context.Prompts.FirstOrDefaultAsync(
                p => p.CreatedAt >= todayUtc && p.CreatedAt <= tomorrowUtc);

            if (prompt is null) return;


            foreach (User user in UsersList)
            {
                currentUser = user;
                otherUser = UsersList.First(u => u != currentUser);

                //fileLogger.writeToLog($"{currentUser.Name} + {otherUser.Name}");

                var promptResponse = await context.PromptResponses.FirstOrDefaultAsync(
                    pr => pr.SentAt >= todayUtc && pr.SentAt <= tomorrowUtc && pr.UserId == currentUser.Id);

                if (promptResponse is null) return;

                message = new MimeMessage();
                message.From.Add(new MailboxAddress("Yours Truly", _emailSettings.Email));
                message.To.Add(MailboxAddress.Parse(otherUser.Email));
                message.Subject = $"Here's What {currentUser.Name} Had to Say...";
                message.Body = new TextPart("plain")
                {
                    Text = $"{prompt.PromptText}\n\n{currentUser.Name}'s Reply:\n{promptResponse.ResponseText}"
                };

                try
                {
                    await client.SendAsync(message, cancellationToken);

                }
                catch (Exception exception)
                {
                    await context.SaveChangesAsync(cancellationToken);
                    fileLogger.writeToLog(exception.ToString());
                }
            }
            prompt.IsComplete = true;
            await context.SaveChangesAsync(cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        // Get both users from database
        private async Task GetUsers(CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var users = await context.Users.ToListAsync(cancellationToken);
            UsersList.Clear();
            UsersList.AddRange(users);
        }

        public async Task<string> GetPromptFromApi(CancellationToken cancellationToken)
        {
            string promptOfTheDay = string.Empty;
            //fileLogger.writeToLog("=====Getting Prompt=====");
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            try
            {
                var client = new Client();
                // Fetch recent questions from DB
                var recentQuestions = await context.Prompts
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .Select(p => p.PromptText)
                    .ToListAsync(cancellationToken);

                string historyContext = recentQuestions.Count > 0
                    ? $"Do NOT repeat concepts or phrasing from these recent questions:\n- {string.Join("\n- ", recentQuestions)}\n"
                    : string.Empty;

                var response = await client.Models.GenerateContentAsync(
                    model: "gemini-3.5-flash",
                    contents: $"You are a relationship expert. Generate exactly one daily reflection question for a couple.\n" +
                              $"{historyContext}" +
                              $"Ensure the topic feels distinct from prior questions.\n" +
                              $"Return ONLY the question text itself.",
                    config: new GenerateContentConfig { Temperature = 0.9f },
                    cancellationToken: cancellationToken
                );

                if (response.Text is null)
                {
                    fileLogger.writeToLog("Retrieve prompt from api failed");
                    return "";
                }

                promptOfTheDay = response.Text;
            }
            catch (Exception exception)
            {
                fileLogger.writeToLog($"Exception:\n{exception.ToString()}");
            }

            fileLogger.writeToLog(promptOfTheDay);
            //fileLogger.writeToLog("=====Finished Getting Prompt=====");

            return promptOfTheDay;
        }

        private async Task StorePromptInDatabase(Prompt prompt, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            await context.Prompts.AddAsync(prompt, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task CheckForTodaysPrompt(CancellationToken cancellationToken = default)
        {
            try
            {
                var prompt = await GetTodaysPrompt(cancellationToken);

                if (prompt is null)
                {
                    string promptText = await GetPromptFromApi(cancellationToken);

                    if (String.IsNullOrEmpty(promptText))
                    {
                        fileLogger.writeToLog("Generated prompt is empty, returning...");
                        return;
                    }

                    prompt = new Prompt
                    {
                        PromptText = promptText
                    };

                    await StorePromptInDatabase(prompt, cancellationToken);
                }
            }
            catch (Exception exception)
            {
                fileLogger.writeToLog(exception.ToString());
            }
        }

        private async Task<Prompt> GetTodaysPrompt(CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var prompt = await context.Prompts
                .FirstOrDefaultAsync(prompt => prompt.CreatedAt >= todayUtc && prompt.CreatedAt <= tomorrowUtc, cancellationToken);

            if (prompt is null)
            {
                return null!;
            }

            return prompt;
        }

        private async Task CheckSendPromptEmail(CancellationToken cancellationToken)
        {
            MimeMessage message;
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var prompt = await GetTodaysPrompt(cancellationToken);
            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect, cancellationToken);
            await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, cancellationToken);

            foreach (User user in UsersList)
            {
                if (await DoesPromptResponseExists(user, false, cancellationToken))
                {
                    continue;
                }

                message = new MimeMessage();
                message.From.Add(new MailboxAddress("Yours Truly", _emailSettings.Email));
                message.To.Add(MailboxAddress.Parse(user.Email!));
                message.Subject = "Prompt of The Day";
                message.Body = new TextPart("plain")
                {
                    Text = $"{prompt.PromptText}\n\nPut 'SEND' at end of response to submit.\n\nI love you!"
                };

                var createPromptResponse = new PromptResponse
                {
                    PromptId = prompt.Id,
                    UserId = user.Id,
                    Status = EmailStatus.Pending,
                    SentAt = DateTime.UtcNow
                };

                await context.PromptResponses.AddAsync(createPromptResponse, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);

                try
                {
                    await client.SendAsync(message, cancellationToken);

                    createPromptResponse.Status = EmailStatus.Sent;
                    await context.SaveChangesAsync(cancellationToken);
                }
                catch (Exception exception)
                {
                    createPromptResponse.Status = EmailStatus.Failed;
                    await context.SaveChangesAsync(cancellationToken);
                    fileLogger.writeToLog(exception.ToString());
                }
            }
            await client.DisconnectAsync(true, cancellationToken);
        }

        private async Task<bool> DoesPromptResponseExists(User user, bool isResponded, CancellationToken cancellationToken)
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            bool exists = false;

            if (isResponded)
            {
                exists = await context.PromptResponses
                    .AnyAsync(pr => pr.SentAt <= tomorrowUtc
                      && pr.SentAt >= todayUtc
                      && pr.UserId == user.Id
                      && (pr.Status == EmailStatus.Responded),
                      cancellationToken);
            }
            else
            {
                exists = await context.PromptResponses
                    .AnyAsync(pr => pr.SentAt <= tomorrowUtc
                                && pr.SentAt >= todayUtc
                                && pr.UserId == user.Id,
                                cancellationToken);
            }

            return exists;
        }

        private async Task CheckForResponseEmails(CancellationToken cancellationToken = default)
        {
            using var client = new ImapClient();
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

                await client.ConnectAsync("imap.gmail.com", 993, true, cancellationToken);
                await client.AuthenticateAsync(_emailSettings.Email, _emailSettings.Password, cancellationToken);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite, cancellationToken);

                foreach (var uid in inbox.Search(SearchQuery.NotSeen, cancellationToken))
                {
                    var message = await inbox.GetMessageAsync(uid, cancellationToken);
                    string emailAddress = message.From.Mailboxes.FirstOrDefault()?.Address!;

                    if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(message.TextBody))
                    {
                        await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                        continue;
                    }

                    foreach (User user in UsersList)
                    {
                        if (await DoesPromptResponseExists(user, true, cancellationToken))
                        {
                            continue;
                        }

                        if (IsValidResponse(emailAddress, message.TextBody, user))
                        {
                            var promptResponse = await context.PromptResponses
                                .FirstOrDefaultAsync(pr => pr.SentAt <= tomorrowUtc && pr.SentAt >= todayUtc && pr.UserId == user.Id, cancellationToken);

                            if (promptResponse is null)
                            {
                                continue;
                            }

                            promptResponse.ResponseText = message.TextBody;
                            promptResponse.ResponseAt = DateTime.UtcNow;
                            promptResponse.Status = EmailStatus.Responded;
                            await context.SaveChangesAsync(cancellationToken);

                            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, cancellationToken);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                fileLogger.writeToLog(exception.ToString());
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
        }

        private bool IsValidResponse(string senderEmailAddress, string message, User user)
        {
            if (!string.Equals(senderEmailAddress, user.Email, StringComparison.OrdinalIgnoreCase)) return false;
            if (!message.Contains("SEND")) return false;

            return true;
        }
    }
}