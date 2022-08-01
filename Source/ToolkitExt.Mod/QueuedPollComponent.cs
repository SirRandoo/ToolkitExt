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
using ToolkitExt.Core;
using ToolkitExt.Core.Models;
using ToolkitExt.Core.Workers;
using Verse;

namespace ToolkitExt.Mod
{
    [UsedImplicitly]
    public class QueuedPollComponent : GameComponent
    {
        private static readonly RimLogger Logger = new RimLogger("Components:QueuedPoll");
        private static volatile bool _processingQueue;

        public QueuedPollComponent(Game game)
        {
        }

        /// <inheritdoc />
        public override void FinalizeInit()
        {
            Logger.Debug("Game loaded");
            Logger.Debug($"Should process queue? {(!_processingQueue).ToStringYesNo()}");
            
            if (!_processingQueue)
            {
                Task.Run(async () => await ProcessQueueAsync());
            }
        }

        private static async Task ProcessQueueAsync()
        {
            Logger.Debug("Processing queued polls...");
            _processingQueue = true;
            
            while (QueuedPollRepository.HasNext())
            {
                if (Current.Game == null)
                {
                    Logger.Debug("Game ended; resetting state...");
                    _processingQueue = false;

                    return;
                }
                
                RawQueuedPoll poll = QueuedPollRepository.GetNext();

                if (poll == null)
                {
                    Logger.Debug("Repository has next, but no poll was obtained; ignoring");
                    
                    return;
                }

                QueuedPollValidator.ValidationResult result = await QueuedPollValidator.ValidateAsync(poll);
                
                Logger.Debug($"Validation result for #{poll.Id} was: {result.Valid} (Reason: {result.ErrorString})");

                bool validated = await BackendClient.Instance.ValidateQueuedPoll(poll.Id, result.Valid, result.ErrorString);
                
                Logger.Debug($"Server's response for poll #{poll.Id}: {validated}");

                if (!validated)
                {
                    Logger.Warn($"Poll #{poll.Id:N0} could not be validated by the server; discarding poll...");

                    bool deleted = await BackendClient.Instance.DeleteQueuedPoll(poll.Id);
                    
                    Logger.Info($@"Poll #{poll.Id} marked for deletion. Deleted? {deleted.ToStringYesNo()}");
                    
                    return;
                }

                Logger.Debug("Queuing poll to poll manager...");
                PollManager.Instance.QueueQueuedPoll(result.Poll);
            }
        }
    }
}
