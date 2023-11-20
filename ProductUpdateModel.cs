using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnNet_MessageProcessor
{
    public class ProductUpdateModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public ItemImageModel? Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    public class ItemImageModel
    {
        public string? Url { get; set; }
        public string? AltText { get; set; }
    }

}
