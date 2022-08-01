// MIT License
// 
// Copyright (c) 2022 SirRandoo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Threading.Tasks;
using JetBrains.Annotations;
using ToolkitExt.Api;
using ToolkitExt.Api.Enums;
using ToolkitExt.Api.Events;
using ToolkitExt.Core.Responses;
using ToolkitExt.Core.Workers;
using Verse;

namespace ToolkitExt.Core.Handlers
{
    internal sealed class QueuedPollCreatedHandler : FilteredMessageHandler
    {
        private static readonly RimLogger Logger = new RimLogger("Handlers:QueuedPollCreated");

        internal QueuedPollCreatedHandler() : base(PusherEvent.QueuedPollCreated)
        {
        }

        /// <inheritdoc/>
        protected override async Task<bool> HandleEvent([NotNull] WsMessageEventArgs args)
        {
            var @event = await args.AsEventAsync<QueuedPollCreatedResponse>();

            if (@event == null)
            {
                return false;
            }

            if (Current.Game == null)
            {
                Logger.Debug("There is no active game; adding it to the queue...");
                QueuedPollRepository.Add(@event.Data);

                return true;
            }

            Logger.Debug("Validating received queued poll...");
            QueuedPollValidator.ValidationResult result = await QueuedPollValidator.ValidateAsync(@event.Data);
            
            Logger.Debug($"Is queued poll valid? {result.Valid.ToStringYesNo()}");

            Logger.Debug("Sending validation result to server...");
            await BackendClient.Instance.ValidateQueuedPoll(@event.Data.Id, result.Valid, result.ErrorString);

            if (!result.Valid)
            {
                Logger.Warn($"Poll #{@event.Data.Id:N0} could not be validated by the server; discarding poll...");

                bool deleted = await BackendClient.Instance.DeleteQueuedPoll(@event.Data.Id);
                    
                Logger.Info($@"Poll #{@event.Data.Id} marked for deletion. Deleted? {deleted.ToStringYesNo()}");

                return false;
            }

            Logger.Debug("Queuing poll...");
            PollManager.Instance.QueueQueuedPoll(result.Poll);

            return true;
        }
    }
}
