namespace Claims.Auditing
{
    public class AuditBackgroundWorker : BackgroundService
    {
        private readonly IAuditQueue _queue;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public AuditBackgroundWorker(IAuditQueue queue, IServiceScopeFactory serviceScopeFactory)
        {
            _queue = queue;
            _serviceScopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var message = await _queue.DequeueAsync(stoppingToken);

                    using var scope = _serviceScopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

                    if (message.Route == "CLAIM")
                    {
                        context.ClaimAudits.Add(new ClaimAudit
                        {
                            Created = DateTime.UtcNow,
                            HttpRequestType = message.Type,
                            ClaimId = message.Id
                        });
                    }
                    else if (message.Route == "COVER")
                    {
                        context.CoverAudits.Add(new CoverAudit
                        {
                            Created = DateTime.UtcNow,
                            HttpRequestType = message.Type,
                            CoverId = message.Id
                        });
                    }

                    await context.SaveChangesAsync(stoppingToken);
                }

                catch (Exception ex)
                {
                }
            }
        }
    }
}
