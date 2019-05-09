using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSales
{
    public class Sales
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime DateSale { get; set; }
        public decimal Price { get; set; }
        public int SellerId { get; set; }
    }

    public class Sellers
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Products
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
