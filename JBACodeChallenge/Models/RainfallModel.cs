using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace JBACodeChallenge.Models
{
    public class RainfallModel 
    {
        [Key]
        public Guid guid { get; set; }
        public long Xref { get; set; }
        public long Yref { get; set; }
        public DateTime Date { get; set; } //
        public int Value { get; set; } //

        public RainfallModel()
        {

        }
    }
}
