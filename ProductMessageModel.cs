using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnNet_MessageProcessor
{
    public class ProductMessageModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Uri? ImageUrl { get; set; }
        public required int CategoryId { get; set; }
        public decimal Price { get; set; }
        public int Amount { get; set; }
    }
}
