using Expenses.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace Expenses.Web.Models
{
    public enum ExpenseReportStatus
    {
        [Display(Name="Submitted for approval")]
        Submitted = DbExpenseReportStatus.Submitted,

        [Display(Name="Saved")]
        Saved = DbExpenseReportStatus.Saved,

        [Display(Name="Approved")]
        Approved = DbExpenseReportStatus.Approved,
    }

    public class ExpenseReportModel
    {
        public ExpenseReportModel()
        {
            this.AssociatedCharges = new List<ChargeModel>();
            this.OutstandingCharges = new List<ChargeModel>();
        }

        public ExpenseReportModel(DbExpenseReport dbReport)
        {
            this.Amount = dbReport.Amount;
            this.Approver = dbReport.Approver;
            this.AssociatedCharges = dbReport.Charges.Select(c => new ChargeModel(c)).ToList();
            this.CostCenter = dbReport.CostCenter;
            this.DateResolved = dbReport.DateResolved;
            this.DateSubmitted = dbReport.DateSubmitted;
            this.EmployeeId = dbReport.EmployeeId;
            this.Id = dbReport.ExpenseReportId;
            this.Notes = dbReport.Notes;
            this.Status = (ExpenseReportStatus)dbReport.Status;
        }

        [HiddenInput(DisplayValue = false)]
        public int Id { get; set; }

        [HiddenInput(DisplayValue = false)]
        public bool HasUnsavedChanges { get; set; }

        [Display(Name = "Report status")]
        public ExpenseReportStatus Status { get; set; }

        [HiddenInput(DisplayValue = false)]
        public int EmployeeId { get; set; }

        public IList<ChargeModel> AssociatedCharges { get; set; }
        public IList<ChargeModel> OutstandingCharges { get; set; }

        [Range(0, 1000000)]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        public int CostCenter { get; set; }

        [Required]
        [StringLength(250)]
        public string Notes { get; set; }

        [Required]
        [StringLength(25)]
        public string Approver { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateSubmitted { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateResolved { get; set; }
    }
}