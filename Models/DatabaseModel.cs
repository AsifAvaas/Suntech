using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SunTech.Models
{
    public class DatabaseModel
    {
        public class User
        {
            [Key]
            public int UserId { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; }

            [Required, MaxLength(50)]
            public string Role { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string PasswordHash { get; set; }

            // Navigation property
            public ICollection<Report> Reports { get; set; }
        }

        public class Report
        {
            [Key]
            public int ReportId { get; set; }

            [Required]
            public DateTime Date { get; set; }

            [Required, MaxLength(50)]
            public string Type { get; set; }

            [ForeignKey("User")]
            public int GeneratedBy { get; set; }
            public User User { get; set; }
        }

        public class Product
        {
            [Key]
            public int ProductId { get; set; }

            [Required, MaxLength(100)]
            public string Name { get; set; }

            [MaxLength(50)]
            public string Category { get; set; }

            public int Quantity { get; set; }

            [MaxLength(100)]
            public string Supplier { get; set; }

            public decimal Price { get; set; }

            public DateTime ExpiryDate { get; set; }

            // Navigation properties
            public ICollection<Inflow> Inflows { get; set; }
            public ICollection<Storage> Storages { get; set; }
            public ICollection<Dispatch> Dispatches { get; set; }
            public ICollection<Waste> Wastes { get; set; }
        }

        public class Inflow
        {
            [Key]
            public int InflowId { get; set; }

            [ForeignKey("Product")]
            public int ProductId { get; set; }
            public Product Product { get; set; }

            public int Quantity { get; set; }

            public DateTime DateReceived { get; set; }

            public int SupplierId { get; set; }
        }

        public class Storage
        {
            [Key]
            public int StorageId { get; set; }

            [ForeignKey("Product")]
            public int ProductId { get; set; }
            public Product Product { get; set; }

            [Required]
            public string Location { get; set; }

            public int Quantity { get; set; }
        }

        public class Dispatch
        {
            [Key]
            public int DispatchId { get; set; }

            [ForeignKey("Product")]
            public int ProductId { get; set; }
            public Product Product { get; set; }

            public int Quantity { get; set; }

            public DateTime DateDispatched { get; set; }

            public int CustomerId { get; set; }
        }

        public class Waste
        {
            [Key]
            public int WasteId { get; set; }

            [ForeignKey("Product")]
            public int ProductId { get; set; }
            public Product Product { get; set; }

            [Required]
            public string Reason { get; set; }

            public int Quantity { get; set; }

            public DateTime DateLogged { get; set; }
        }
    }
}
