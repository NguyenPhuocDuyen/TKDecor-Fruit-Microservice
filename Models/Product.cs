﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Models
{
    public partial class Product
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
            Reviews = new HashSet<Review>();
        }

        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? RecentPrice { get; set; } = 0;
        public string Thumbnail { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public int? UserId { get; set; }
        public int? Amount { get; set; } = 0;
        public DateTime? CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdateAt { get; set; } = DateTime.Now;

        public virtual Category Category { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
    }
}
