using System;
namespace ProvisionService.Models
{
	public class ProvisionResponse
	{
        public Data Data { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public string TransactionId { get; set; }
    }

    public class Product
    {
        public string ProductName { get; set; }
        public double Cost { get; set; }
        public long ProductId { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public int ValidityDays { get; set; }
        public string Type { get; set; }
        public int ValidityHours { get; set; }
        public CostCurrency CostCurrency { get; set; }
        public string TransactionId { get; set; }
    }

    public class CostCurrency
    {
        public int USD { get; set; }
        public int LRD { get; set; }
    }

    public class Data
    {
        public Product Product { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}

