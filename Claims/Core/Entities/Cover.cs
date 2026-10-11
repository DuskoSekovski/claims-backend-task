using Claims.Core.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims.Core.Entities
{
    public class Cover
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("startDate")]
        //[BsonDateTimeOptions(DateOnly = true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        //[BsonDateTimeOptions(DateOnly = true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime EndDate { get; set; }

        [BsonElement("claimType")]
        public CoverType Type { get; set; }

        [BsonElement("premium")]
        public decimal Premium { get; set; }
    }
}