﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace NungSue.Entities
{
    public partial class History
    {
        public Guid CustomerId { get; set; }
        public Guid BookId { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Book Book { get; set; }
        public virtual Customer Customer { get; set; }
    }
}