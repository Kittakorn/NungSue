﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace NungSue.Entities
{
    public partial class PriceOffer
    {
        public Guid PriceOfferId { get; set; }
        public int NewPrice { get; set; }
        public string PromotionText { get; set; }
        public Guid BookId { get; set; }

        public virtual Book Book { get; set; }
    }
}