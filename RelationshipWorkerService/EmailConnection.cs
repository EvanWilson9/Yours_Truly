using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using RelationshipWorkerService.Tools;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RelationshipWorkerService
{
    internal class EmailConnection
    {

        FileLogger fileLogger = new FileLogger();

        public async Task CheckInboxAsync()
        {
            try
            {
                fileLogger.writeToLog("=====Session Started=====");
                using var client = new ImapClient();

                //await client.ConnectAsync("imap.gmail.com", 993, true);
                //await client.AuthenticateAsync(emailAddress, password);

                var inbox = client.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadWrite);

                foreach (var email in inbox.Search(SearchQuery.NotSeen))
                {
                    var message = await inbox.GetMessageAsync(email);

                    if (message.TextBody == null)
                    {
                        fileLogger.writeToLog("=====Message is null=====");
                        return;
                    }

                    bool isValid = message.TextBody.Contains("SEND");
                    fileLogger.writeToLog($"{message.TextBody}... This message is Valid = {isValid}");
                }

                await client.DisconnectAsync(true);
                fileLogger.writeToLog("=====Session Ended=====");

            }
            catch (Exception exception)
            {
                fileLogger.writeToLog(exception.ToString());
            }
        }

        public async Task SendEmailAsync()
        {
            MimeMessage message = new MimeMessage();
            //message.From.Add(new MailboxAddress("Yours Truly", emailAddress));
            message.To.Add(MailboxAddress.Parse("evanpwilson1@gmail.com"));

            message.Subject = "Prompt of The Day";

            message.Body = new TextPart("plain")
            {
                Text = "[PROMPT GOES HERE]\n\nReply to this email with your response.\n\nI love you!"
            };

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);
                //await client.AuthenticateAsync(emailAddress, password);
                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
            catch (Exception exception)
            {
                fileLogger.writeToLog(exception.ToString());
            }
        }
    }
}