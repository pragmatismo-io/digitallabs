﻿/*****************************************************************************\
*
* Copyright (c) Pragmatismo.io. All rights reserved.
* Licensed under the MIT license.
*
* Pragmatismo.io: http://pragmatismo.io
*
* MIT License:
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be
* included in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
* NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
* LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
* OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
* WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
* 
\*****************************************************************************/

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Connector;

#endregion

namespace Pragmatismo.Io.Framework.Bots
{
    public class BotToUserSpeech : IBotToUser
    {
        private readonly Action<string> _callback;
        private readonly IMessageActivity _toBot;

        public BotToUserSpeech(IMessageActivity toBot, Action<string> callback)
        {
            SetField.NotNull(out _toBot, nameof(toBot), toBot);
            _callback = callback;
        }

        public IMessageActivity MakeMessage()
        {
            return _toBot;
        }

        public async Task PostAsync(IMessageActivity message,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            _callback(message.Text);
            if (message.Attachments?.Count > 0)
                _callback(ButtonsToText(message.Attachments));
        }

        private static string ButtonsToText(IList<Attachment> attachments)
        {
            var cardAttachments =
                attachments?.Where(attachment => attachment.ContentType.StartsWith("application/vnd.microsoft.card"));
            var builder = new StringBuilder();
            if (cardAttachments != null && cardAttachments.Any())
            {
                builder.AppendLine();
                foreach (var attachment in cardAttachments)
                {
                    var type = attachment.ContentType.Split('.').Last();
                    if (type == "hero" || type == "thumbnail")
                    {
                        var card = (HeroCard) attachment.Content;
                        if (!string.IsNullOrEmpty(card.Title))
                            builder.AppendLine(card.Title);
                        if (!string.IsNullOrEmpty(card.Subtitle))
                            builder.AppendLine(card.Subtitle);
                        if (!string.IsNullOrEmpty(card.Text))
                            builder.AppendLine(card.Text);
                        if (card.Buttons != null)
                            foreach (var button in card.Buttons)
                                builder.AppendLine(button.Title);
                    }
                }
            }
            return builder.ToString();
        }
    }
}