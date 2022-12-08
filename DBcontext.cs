using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace ASKOmaster
{
    public class SparePartsContext : DbContext
    {
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Release> Release { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=172.20.190.100;Username=roshan_xps;Password=731797;Database=SpareParts");
        }
    }

    public class Jobs
    {
        [Key][Required] public string idoss { get; set; }
        public string? wos { get; set; }
        public DateTime? date { get; set; }
        public string? tech { get; set; }
        public string[]? parts { get; set; }
        public string? model { get; set; }
        public string? art { get; set; }
        public string? warranty { get; set; }
    }
    public class Release
    {
        [Key][Required] public string sap { get; set; }
        public DateTime date { get; set; }
        public string part { get; set; }
        public int quantity { get; set; }
        public string? dn { get; set; }
    }
}
