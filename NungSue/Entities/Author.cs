﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace NungSue.Entities
{
    public partial class Author
    {
        public Author()
        {
            BookAuthors = new HashSet<BookAuthor>();
        }

        public Guid AuthorId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<BookAuthor> BookAuthors { get; set; }
    }
}