using System;
using System.Linq;
using EventStore.Core.Data;
using EventStore.Core.Messages;
using EventStore.Core.Services.Storage.ReaderIndex;
using EventStore.Core.Services.TimerService;
using EventStore.Core.TransactionLog.LogRecords;
using EventStore.Projections.Core.Messages;
using EventStore.Projections.Core.Services.Processing;
using EventStore.Projections.Core.Tests.Services.core_projection;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.stream_event_distribution_point
{
    [TestFixture]
    public class when_handling_no_stream : TestFixtureWithExistingEvents
    {
        private StreamReaderEventDistributionPoint _edp;
        private Guid _publishWithCorrelationId;
        private Guid _distibutionPointCorrelationId;
        private Guid _firstEventId;
        private Guid _secondEventId;

        protected override void Given()
        {
            TicksAreHandledImmediately();
        }

        [SetUp]
        public void When()
        {
            _publishWithCorrelationId = Guid.NewGuid();
            _distibutionPointCorrelationId = Guid.NewGuid();
            _edp = new StreamReaderEventDistributionPoint(_bus, _distibutionPointCorrelationId, "stream", 0, false);
            _edp.Resume();
            _firstEventId = Guid.NewGuid();
            _secondEventId = Guid.NewGuid();
            _edp.Handle(
                new ClientMessage.ReadStreamEventsForwardCompleted(
                    _distibutionPointCorrelationId, "stream",
                    new EventLinkPair[0], RangeReadResult.NoStream, 0, 200, ExpectedVersion.NoStream));
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void cannot_be_resumed()
        {
            _edp.Resume();
        }

        [Test]
        public void cannot_be_paused()
        {
            _edp.Pause();
        }

        [Test]
        public void publishes_read_events_from_beginning_with_correct_next_event_number()
        {
            Assert.AreEqual(1, _consumer.HandledMessages.OfType<ClientMessage.ReadStreamEventsForward>().Count());
            Assert.AreEqual(
                "stream", _consumer.HandledMessages.OfType<ClientMessage.ReadStreamEventsForward>().Last().EventStreamId);
            Assert.AreEqual(
                0, _consumer.HandledMessages.OfType<ClientMessage.ReadStreamEventsForward>().Last().FromEventNumber);
        }

        [Test]
        public void publishes_correct_committed_event_received_messages()
        {
            Assert.AreEqual(
                1, _consumer.HandledMessages.OfType<ProjectionMessage.Projections.CommittedEventDistributed>().Count());
            var first =
                _consumer.HandledMessages.OfType<ProjectionMessage.Projections.CommittedEventDistributed>().Single();
            Assert.IsNull(first.Data);
            Assert.AreEqual(200, first.SafeTransactionFileReaderJoinPosition);
        }

        [Test]
        public void publishes_schedule()
        {
            Assert.AreEqual(1, _consumer.HandledMessages.OfType<TimerMessage.Schedule>().Count());
        }

        [Test]
        public void publishes_read_events_on_schedule_reply()
        {
            Assert.AreEqual(1, _consumer.HandledMessages.OfType<TimerMessage.Schedule>().Count());
            var schedule = _consumer.HandledMessages.OfType<TimerMessage.Schedule>().Single();
            schedule.Reply();
            Assert.AreEqual(2, _consumer.HandledMessages.OfType<ClientMessage.ReadStreamEventsForward>().Count());
        }

    }
}