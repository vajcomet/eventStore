using EventStore.Client;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eventStoreASP.Models;

namespace eventStoreASP
{
    public class Client
    {

        private EventStoreClient _client { get; set; }
        private IEnumerable<DataModel> _events { get; set; }
        public int eventsCount { get; set; } = 10;
        public IEnumerable<DataModel> events
        {
            get
            {
                stream(eventsCount);
                return _events;
            }
        }

        public EventStoreClient establishConnection(string connectionString)
        {
            var settings = EventStoreClientSettings.Create(connectionString);
            _client = new EventStoreClient(settings);
            return _client;
        }

        async public void writeSomeStream()
        {
            var eventData = new EventData(
                Uuid.NewUuid(),
                "some-event",
                Encoding.UTF8.GetBytes("{\"id\": \"1\" " + DateTime.Now.ToString() + " \"type\": \"price\"  \"value\": \"some value\"}")
            );

            await _client.AppendToStreamAsync(
                "some-new-stream",
                StreamState.Any,
                new[] { eventData },
                cancellationToken: default(CancellationToken)
            );
        }

        async void stream(long maxCount)
        {
            var result = _client.ReadStreamAsync(
                Direction.Backwards,
                "some-new-stream",
                StreamPosition.End,
                maxCount: maxCount,
                resolveLinkTos: false,
                cancellationToken: default(CancellationToken)
            );

            if (await result.ReadState == ReadState.StreamNotFound)
            {
                return;
            }

            await foreach (ResolvedEvent @event in result)
            {
                _events.Equals(Encoding.UTF8.GetString(@event.Event.Data.ToArray()));
            }
        }
    }
}
