using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Airlines_Миронченков.Models
{
    public class Ticket
    {
        public int price { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public DateTime time_start { get; set; }
        public DateTime time_end { get; set; }

        public Ticket() { }
        public Ticket(int price, string from, string to, DateTime time_start, DateTime time_end)
        {
            this.price = price;
            this.from = from;
            this.to = to;
            this.time_start = time_start;
            this.time_end = time_end;
        }
    }
}