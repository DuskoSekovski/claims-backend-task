namespace Claims.Auditing
{
    public class Auditer : IAuditer
    {
        private readonly IAuditQueue _queue;

        public Auditer(IAuditQueue queue)
        {
            _queue = queue;
        }

        public void AuditClaim(string id, string httpRequestType)
        {
            _queue.QueueAudit(id, httpRequestType, "CLAIM");
        }
        
        public void AuditCover(string id, string httpRequestType)
        {
            _queue.QueueAudit(id, httpRequestType, "COVER");
        }
    }
}
