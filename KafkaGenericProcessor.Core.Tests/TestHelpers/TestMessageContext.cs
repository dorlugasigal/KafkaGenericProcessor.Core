using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaGenericProcessor.Core.Exceptions;

namespace KafkaGenericProcessor.Core.Tests.TestHelpers
{
    #region KafkaFlow Type Mocks

    // Interfaces from KafkaFlow we need to mock
    namespace KafkaFlow
    {
        public interface IMessageContext
        {
            Message Message { get; }
            IConsumerContext ConsumerContext { get; }
            IProducerContext ProducerContext { get; }
            IDictionary<string, object> Items { get; }
            IMessageHeaders Headers { get; }
            IReadOnlyCollection<string> Brokers { get; }
            IDependencyResolver DependencyResolver { get; }
            IMessageContext SetMessage(object key, object value);
            Task<MiddlewareExecutionResult> Next();
        }

        public interface IConsumerContext
        {
            string ConsumerName { get; }
            string GroupId { get; }
            string Topic { get; }
            int Partition { get; }
            long Offset { get; }
            int WorkerId { get; }
            bool AutoMessageCompletion { get; set; }
            bool ShouldStoreOffset { get; set; }
            TopicPartitionOffset TopicPartitionOffset { get; }
            DateTime MessageTimestamp { get; }
            IDependencyResolver ConsumerDependencyResolver { get; }
            IDependencyResolver WorkerDependencyResolver { get; }
            Task<TopicPartitionOffset> Completion { get; }
            event Action WorkerStopped;
            Task StoreOffset();
            void Pause();
            void Resume();
            void Complete();
            IOffsetsWatermark GetOffsetsWatermark();
        }

        public interface IMessageContextMessage
        {
            object Key { get; }
            object Value { get; }
            IReadOnlyDictionary<string, byte[]> Headers { get; }
            DateTime UtcTimestamp { get; }
        }

        public interface IProducerContext { }
        
        public interface IDependencyResolver
        {
            object Resolve(Type type);
            T Resolve<T>() where T : class;
        }

        public interface IMessageHeaders : IReadOnlyDictionary<string, byte[]> { }

        public interface IOffsetsWatermark { }

        public enum MiddlewareExecutionResult
        {
            Success,
            Skipped
        }

        public class Message
        {
            public object Key { get; }
            public object Value { get; }
            
            public Message(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }

        public class TopicPartitionOffset
        {
            public string Topic { get; }
            public int Partition { get; }
            public long Offset { get; }

            public TopicPartitionOffset(string topic, int partition, long offset)
            {
                Topic = topic;
                Partition = partition;
                Offset = offset;
            }
        }
    }
    
    #endregion

    /// <summary>
    /// A test implementation of IMessageContext for unit testing
    /// </summary>
    public class TestMessageContext : KafkaFlow.IMessageContext
    {
        private readonly Dictionary<string, byte[]> _headers = new Dictionary<string, byte[]>();
        private readonly List<string> _brokers = new List<string> { "localhost:9092" };

        public TestMessageContext(object messageValue, string? messageKey = null)
        {
            Message = new KafkaFlow.Message(messageKey, messageValue);
            ConsumerContext = new TestConsumerContext();
            Items = new Dictionary<string, object>();
            Headers = new TestMessageHeaders(_headers);
            DependencyResolver = new TestDependencyResolver();
        }

        public KafkaFlow.IMessageContext SetMessage(object key, object value)
        {
            Message = new KafkaFlow.Message(key, value);
            return this;
        }

        public KafkaFlow.Message Message { get; private set; }
        
        public KafkaFlow.IConsumerContext ConsumerContext { get; }
        
        public KafkaFlow.IProducerContext ProducerContext => throw new NotImplementedException("ProducerContext not implemented in test context");
        
        public IDictionary<string, object> Items { get; }

        public KafkaFlow.IMessageHeaders Headers { get; }

        public IReadOnlyCollection<string> Brokers => _brokers;

        public KafkaFlow.IDependencyResolver DependencyResolver { get; }
        
        public Task<KafkaFlow.MiddlewareExecutionResult> Next()
        {
            // Default implementation does nothing
            return Task.FromResult(KafkaFlow.MiddlewareExecutionResult.Success);
        }
    }

    public class TestMessageHeaders : KafkaFlow.IMessageHeaders
    {
        private readonly Dictionary<string, byte[]> _headers;

        public TestMessageHeaders(Dictionary<string, byte[]> headers)
        {
            _headers = headers;
        }

        public byte[] this[string key] => _headers[key];
        public IEnumerable<string> Keys => _headers.Keys;
        public IEnumerable<byte[]> Values => _headers.Values;
        public int Count => _headers.Count;
        public bool ContainsKey(string key) => _headers.ContainsKey(key);
        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => _headers.GetEnumerator();
        public bool TryGetValue(string key, out byte[] value) => _headers.TryGetValue(key, out value);
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _headers.GetEnumerator();
    }

    public class TestDependencyResolver : KafkaFlow.IDependencyResolver
    {
        public object Resolve(Type type) => Activator.CreateInstance(type);
        public T Resolve<T>() where T : class => (T)Resolve(typeof(T));
    }

    public class TestOffsetsWatermark : KafkaFlow.IOffsetsWatermark
    {
        public long High { get; set; } = 100;
        public long Low { get; set; } = 0;
    }

    public class TestConsumerContext : KafkaFlow.IConsumerContext
    {
        private readonly KafkaFlow.TopicPartitionOffset _topicPartitionOffset;

        public TestConsumerContext(int workerId = 1)
        {
            WorkerId = workerId;
            _topicPartitionOffset = new KafkaFlow.TopicPartitionOffset("test-topic", 0, 0);
            AutoMessageCompletion = true;
            ShouldStoreOffset = true;
        }

        public int WorkerId { get; }
        
        public string ConsumerName => "test-consumer";
        
        public string GroupId => "test-group";
        
        public string Topic => "test-topic";
        
        public int Partition => 0;
        
        public long Offset => 0;

        public bool AutoMessageCompletion { get; set; }

        public bool ShouldStoreOffset { get; set; }

        public KafkaFlow.TopicPartitionOffset TopicPartitionOffset => _topicPartitionOffset;
        
        public DateTime MessageTimestamp => DateTime.UtcNow;
        
        public KafkaFlow.IDependencyResolver ConsumerDependencyResolver => new TestDependencyResolver();
        
        public KafkaFlow.IDependencyResolver WorkerDependencyResolver => new TestDependencyResolver();
        
        public Task<KafkaFlow.TopicPartitionOffset> Completion =>
            Task.FromResult(_topicPartitionOffset);
        
        public event Action WorkerStopped;
        
        public Task StoreOffset() => Task.CompletedTask;
        
        public void Pause() { }
        
        public void Resume() { }

        public void Complete() { }
        
        public KafkaFlow.IOffsetsWatermark GetOffsetsWatermark() => new TestOffsetsWatermark();
    }
}