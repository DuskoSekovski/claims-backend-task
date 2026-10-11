using System.Threading.Channels;

namespace Claims.Auditing
{
    public class AuditQueue : IAuditQueue
    {
        public record AuditMessage(string Id, string Type, string Route);
        private readonly Channel<AuditMessage> _queue = Channel.CreateBounded<AuditMessage>(new BoundedChannelOptions(1000)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
        public async Task<AuditMessage> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }

        public void QueueAudit(string id, string type, string route)
        {
            _queue.Writer.TryWrite(new AuditMessage(id, type, route));
        }
    }
}
