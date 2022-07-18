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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace ToolkitExt.Api
{
    public sealed class RimLogger
    {
        private static SynchronizationContext _context = SynchronizationContext.Current;

        private readonly string _name;
        private bool _debugChecked;
        private bool _debugEnabled;

        public RimLogger(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Formats a log message.
        /// </summary>
        /// <param name="message">The message to format</param>
        /// <returns>The formatted message</returns>
        [NotNull]
        public string FormatMessage(string message) => $"{_name} :: {message}";

        /// <summary>
        ///     Formats a log message.
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="message">The message to format</param>
        /// <returns>The formatted message</returns>
        [NotNull]
        public string FormatMessage([NotNull] string level, string message) => $"{level.ToUpperInvariant()} {_name} :: {message}";

        /// <summary>
        ///     Formats a log message.
        /// </summary>
        /// <param name="level">The log level of the message</param>
        /// <param name="message">The message to format</param>
        /// <param name="color">The color code of the message</param>
        /// <returns>The formatted message</returns>
        [NotNull]
        public string FormatMessage([NotNull] string level, string message, [NotNull] string color) =>
            $@"<color=""#{color.TrimStart('#')}"">{FormatMessage(level, message)}</color>";

        /// <summary>
        ///     Logs a message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Log(string message)
        {
            LogInternal(FormatMessage(message));
        }

        private static void LogInternal(string message)
        {
            if (UnityData.IsInMainThread)
            {
                _context ??= SynchronizationContext.Current;

                Verse.Log.Message(message);
            }
            else
            {
                _context?.Post(_ => Verse.Log.Message(message), null);
            }
        }

        /// <summary>
        ///     Logs an INFO level message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Info(string message)
        {
            LogInternal(FormatMessage("INFO", message));
        }

        /// <summary>
        ///     Logs a WARN level message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Warn(string message)
        {
            LogInternal(FormatMessage("WARN", message, "#FF6B00"));
        }

        /// <summary>
        ///     Logs an ERROR level message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Error(string message)
        {
            LogInternal(FormatMessage("ERR", message, "#FF768CE"));
            Verse.Log.TryOpenLogWindow();
        }

        /// <summary>
        ///     Logs an ERROR level message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="exception">The exception to log</param>
        public void Error(string message, [NotNull] Exception exception)
        {
            Error($"{message} :: {exception.GetType().Name}({exception.Message})\n\n{exception.ToStringSafe()}");
        }

        /// <summary>
        ///     Logs a DEBUG level message to RimWorld's log window.
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Debug(string message)
        {
            if (Prefs.DevMode && Prefs.LogVerbose)
            {
                LogInternal(FormatMessage("DEBUG", message, ColorUtility.ToHtmlStringRGB(ColorLibrary.LightPink)));
            }
        }
    }
}
