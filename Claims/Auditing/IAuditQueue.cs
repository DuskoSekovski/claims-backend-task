using static Claims.Auditing.AuditQueue;

namespace Claims.Auditing
{
    public interface IAuditQueue
    {
        void QueueAudit(string id, string type, string route);
        Task<AuditMessage> DequeueAsync(CancellationToken cancellationToken);
    }
}
