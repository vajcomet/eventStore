using EventStore.Client;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using eventStoreASP.Models;
using Newtonsoft.Json;
using System.Linq;

namespace eventStoreASP
{
    public class Client
    {
        public string connectionString { get; set; } = "esdb://admin:changeit@127.0.0.1:2113?tls=false";

        private EventStoreClient _client { get; set; }

        private List<string> _list { get; set; } = new List<string>();
        private List<DataModel> _events { get; set; } = new List<DataModel>();
        public int eventsCount { get; set; } = 10;
        public IEnumerable<DataModel> events
        {
            get
            {
                stream(eventsCount);
                return _events.Where(x => x.time > DateTime.Now.AddMinutes(-10));
            }
        }

        public EventStoreClient establishConnection()
        {
            var settings = EventStoreClientSettings.Create(connectionString);
            _client = new EventStoreClient(settings);
            stream(10);
            return _client;
        }

        async public void writeSomeStream()
        {
            DataModel temp = new DataModel();

            temp.time = DateTime.Now;
            temp.id = "1";
            temp.value = "dačo";
            temp.type = "type";

            JsonConvert.SerializeObject(temp);

            var eventData = new EventData(
                Uuid.NewUuid(),
                "some-event",
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(temp))
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
                try
                {
                    var temp = Encoding.UTF8.GetString(@event.Event.Data.ToArray());
                    var temp1 = JsonConvert.DeserializeObject<DataModel>(temp);
                    _events.Add(temp1);
                }
                catch
                {
                }
            }
        }
    }
}
