using System;
using System.ComponentModel.DataAnnotations;

namespace TimegripExtended.Business.Domain
{
    public class ExcludedTask
    {
        [Key]
        public int Key { get; set; }
        public string JiraNumber { get; set; }
        public string Name { get; set; }
        public DateTime Excluded { get; set; }
    }
}