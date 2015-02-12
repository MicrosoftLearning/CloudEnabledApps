using Expenses.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Expenses.Web.Models
{
    public class ChargeModel
    {
        public ChargeModel()
        { }

        public ChargeModel(DbCharge dbCharge)
        {
            this.Id = dbCharge.ChargeId;
            this.BilledAmount = dbCharge.BilledAmount;
            this.Description = dbCharge.Description;
            this.ExpenseDate = dbCharge.ExpenseDate;
            this.Location = dbCharge.Location;
            this.Merchant = dbCharge.Merchant;
            this.Notes = dbCharge.Notes;
            this.TransactionAmount = dbCharge.TransactionAmount;
        }

        public DbCharge ConvertToDbCharge(int employeeId)
        {
            DbCharge dbCharge = new DbCharge()
            {
                ChargeId = this.Id,
                BilledAmount = this.BilledAmount,
                Description = this.Description,
                EmployeeId = employeeId,
                ExpenseDate = this.ExpenseDate,
                Location = this.Location,
                Merchant = this.Merchant,
                Notes = this.Notes,
                TransactionAmount = this.TransactionAmount,
            };

            return dbCharge;
        }

        public int Id { get; set; }

        [StringLength(250)]
        public string Notes { get; set; }

        [Display(Name="Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ExpenseDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Merchant { get; set; }

        [Required]
        [StringLength(50)]
        public string Location { get; set; }

        [Display(Name="Billed amount")]
        [Range(0, 1000000)]
        [DataType(DataType.Currency)]
        public decimal BilledAmount { get; set; }

        [Display(Name="Transaction amount", Prompt="Transaction on your corporate credit card")]
        [Range(0, 1000000)]
        [DataType(DataType.Currency)]
        public decimal TransactionAmount { get; set; }

        [Required]
        [StringLength(250)]
        public string Description { get; set; }
    }
}