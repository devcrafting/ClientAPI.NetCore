using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Internal;
using EventStore.ClientAPI.Exceptions;
using EventStore.Core.Tests.ClientAPI.Helpers;
using System.Threading.Tasks;

namespace EventStore.Core.Tests.ClientAPI
{
    public class when_keep_reconnecting_without_server_responding
    {
        [Test]
        public void read_events_should_timeout()
        {
            var settings =
                ConnectionSettings.Create()
                    .EnableVerboseLogging()
                    .KeepReconnecting()
                    .SetOperationTimeoutTo(TimeSpan.FromSeconds(1))
                    .WithConnectionTimeoutOf(TimeSpan.FromSeconds(10))
                    .SetReconnectionDelayTo(TimeSpan.FromMilliseconds(0))
                    .FailOnNoServerResponse();

            using (var connection = EventStoreConnection.Create(settings, TestNode.BlackHole))
            {
                connection.ConnectAsync().Wait();
                Assert.Throws<OperationTimedOutException>(() =>
                {
                    var task = connection.ReadAllEventsForwardAsync(Position.Start, 100, false);
                    // nunit timeout attribute still not working with dotnet core
                    Exception expectedException = null;
                    Task.WhenAny(task, Task.Delay(10000))
                        .ContinueWith(t => expectedException = t.Result.Exception).Wait();
                    if (expectedException != null)
                        throw expectedException.InnerException;
                });
            }
        }
    }
}
