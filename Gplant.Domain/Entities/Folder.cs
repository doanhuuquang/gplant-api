using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gplant.Domain.Entities
{
    public class Folder
    {
        public Guid Id { get; set; }
        public required string Slug { get; set; }
        public required string Name { get; set; }
        public required DateTimeOffset CreatedAtUtc { get; set; }
        public required int MediaCount { get; set; }
    }
}
