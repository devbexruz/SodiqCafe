using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SodiqCafeMVC.Web.Models
{
    public class Cafe
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}