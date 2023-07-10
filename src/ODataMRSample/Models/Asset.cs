using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.OData.ModelBuilder;

namespace ODataMRSample.Models
{
    [MediaType]
    public class Asset
    {
        public string Id { get; set; } = string.Empty;
        [NotMapped]
        public string Path { get; set; } = string.Empty;
        public IDictionary<string, object?> Properties { get; set; } = new Dictionary<string, object?>();
    }
}
