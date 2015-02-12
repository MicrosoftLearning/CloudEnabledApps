using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Expenses.Data
{
    [DataContract(Name="Employee", IsReference = true)]
    public class DbEmployee
    {
        public DbEmployee()
        {
            this.Charges = new HashSet<DbCharge>();
            this.ExpenseReports = new HashSet<DbExpenseReport>();
        }

        [DataMember]
        [Key]
        public int EmployeeId { get; set; }

        [DataMember]
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [DataMember]
        [Required]
        [StringLength(25)]
        public string Alias { get; set; }

        [DataMember]
        [Required]
        [StringLength(25)]
        public string Manager { get; set; }

        [DataMember]
        public virtual ICollection<DbCharge> Charges { get; set; }

        [DataMember]
        public virtual ICollection<DbExpenseReport> ExpenseReports { get; set; }
    }
}
