using Claims.Core.Enums;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Claims.Core.Entities
{
    public class Claim
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("coverId")]
        public string CoverId { get; set; }

        [BsonElement("created")]
        //[BsonDateTimeOptions(DateOnly = true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime Created { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("claimType")]
        public ClaimType Type { get; set; }

        [Range(0, 100000, ErrorMessage = "Damage cost cannot exceed 100,000.")]
        [BsonElement("damageCost")]
        public decimal DamageCost { get; set; }
    }
}