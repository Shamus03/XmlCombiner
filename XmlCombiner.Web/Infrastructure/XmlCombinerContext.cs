﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XmlCombiner.Web.Domain;

namespace XmlCombiner.Web.Infrastructure
{
    public class XmlCombinerContext : DbContext
    {
        public DbSet<Feed> Feeds { get; set; }

        public XmlCombinerContext(DbContextOptions<XmlCombinerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feed>(feed =>
            {
                feed.HasMany(f => f.AdditionalParameters)
                    .WithOne();
            });
        }
    }
}