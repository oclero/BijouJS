﻿using System;
using System.Diagnostics;

namespace Bijou.Projected
{
    // Class projected to JS context, implementing console logging feature
    // Methods name are lowerCase as projection force them to lowerCase
    public static class JSConsole
    {
        // Events
        public static event EventHandler<string> ConsoleMessageReady;
        /// <summary>
        ///     JS Native function for console.log
        /// </summary>
        public static void log(string message)
        {
            Debug.WriteLine("[UWPChakraHost][trace] " + message);
            ConsoleMessageReady?.Invoke(null, message);
        }

        /// <summary>
        ///     JS Native function for console.info
        /// </summary>
        public static void info(string message)
        {
            Debug.WriteLine("[UWPChakraHost][info] " + message);
            ConsoleMessageReady?.Invoke(null, message);
        }

        /// <summary>
        ///     JS Native function for console.warn
        /// </summary>
        public static void warn(string message)
        {
            Debug.WriteLine("[UWPChakraHost][warn] " + message);
            ConsoleMessageReady?.Invoke(null, message);
        }

        /// <summary>
        ///     JS Native function for console.error
        /// </summary>
        public static void error(string message)
        {
            Debug.WriteLine("[UWPChakraHost][error] " + message);
            ConsoleMessageReady?.Invoke(null, message);
        }
    }
}
