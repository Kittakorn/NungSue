﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace NungSue.Entities
{
    public partial class CustomerAddress
    {
        public Guid AddressId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string District { get; set; }
        public string SubDistrict { get; set; }
        public string Province { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime CreateDate { get; set; }

        public virtual Customer Customer { get; set; }
    }
}