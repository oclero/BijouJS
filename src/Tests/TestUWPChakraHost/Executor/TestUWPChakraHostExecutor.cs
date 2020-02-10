﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Bijou.Executor;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bijou.Test.UWPChakraHost.Executor
{
    [TestClass]
    public class TestUWPChakraHostExecutor
    {
        [DataTestMethod]
        [DataRow(@"ms-appx:///Assets/scripts/test_script_load.js", "a = 100;")]
        [DataRow(@"ms-appx:///Assets/scripts/test_script_load_non_ascii.js", "a = ♥;")]
        public async Task TestLoadScript(string filename, string expected)
        {
            var scriptContent = await UWPChakraHostExecutor.LoadScript(new Uri(filename, UriKind.Absolute));
            Assert.AreEqual(expected, scriptContent);
        }
        
        [TestMethod]
        public async Task TestLoadAndRunScriptAsync()
        {
            var reply = string.Empty;
            var messageReceived = new AutoResetEvent(false);
            var timeout = TimeSpan.FromSeconds(1);
            using (var executor = new UWPChakraHostExecutor()) {

                executor.MessageReady += (sender, msg) =>
                {
                    reply = msg;
                    messageReceived.Set();
                };
                await executor.RunScriptAsync(new Uri(@"ms-appx:///Assets/scripts/test_script_load_async.js"));
                Assert.IsTrue(messageReceived.WaitOne(timeout), $"Timeout waiting for the JS script to finish after {timeout}");
                Assert.AreEqual("Hello from the other side", reply);
            }
        }

        [TestMethod]
        public async Task TestCallFunctionAsync()
        {
            using (var executor = new UWPChakraHostExecutor()) {
                var reply = string.Empty;
                var messageReceived = new AutoResetEvent(false);
                var timeout = TimeSpan.FromSeconds(5);
                executor.MessageReady += (sender, msg) => 
                {
                    reply = msg;
                    messageReceived.Set();
                };
                await executor.RunScriptAsync(new Uri(@"ms-appx:///Assets/scripts/test_script_load_call_function.js"));
                const string request = "Hello ♥";
                executor.CallFunctionAsync("echo", request);
                Assert.IsTrue(messageReceived.WaitOne(timeout), $"Timeout waiting for the JS script to finish after {timeout}");
                Assert.AreEqual(request, reply);
            }
        }

        [TestMethod]
        public void TestSeparateExecutorsAreIsolated()
        {
            const string script = @"
                a = 0;

                function ping() {
                    a++;
                    sendToHost(a.toString());
                }
            ";

            using (var executorOne = new UWPChakraHostExecutor())
            using (var executorTwo = new UWPChakraHostExecutor()) {
                // This will define a variable 'a' set to 0 and a function 'ping' which increments it, in each executor
                executorOne.RunScriptAsync(script);
                executorTwo.RunScriptAsync(script);

                var replyOne = string.Empty;
                var replyTwo = string.Empty;
                var messageReceivedOne = new AutoResetEvent(false);
                var messageReceivedTwo = new AutoResetEvent(false);
                var timeout = TimeSpan.FromSeconds(1);
                executorOne.MessageReady += (sender, msg) =>
                {
                    replyOne = msg;
                    messageReceivedOne.Set();
                };
                executorTwo.MessageReady += (sender, msg) =>
                {
                    replyTwo = msg;
                    messageReceivedTwo.Set();
                };

                // Executors should have isolated contexts, so calling 'ping' on one should not increment 'a' in the other
                executorOne.CallFunctionAsync("ping");
                executorTwo.CallFunctionAsync("ping");
                Assert.IsTrue(messageReceivedOne.WaitOne(timeout), $"Timeout waiting for the JS script 1 to finish after {timeout}");
                Assert.IsTrue(messageReceivedTwo.WaitOne(timeout), $"Timeout waiting for the JS script 2 to finish after {timeout}");
                Assert.AreEqual(replyOne, "1");
                Assert.AreEqual(replyTwo, "1");
            }
        }
    }
}
