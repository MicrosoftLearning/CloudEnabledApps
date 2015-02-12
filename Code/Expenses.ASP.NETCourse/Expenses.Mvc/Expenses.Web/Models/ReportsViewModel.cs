using Expenses.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Expenses.Web.Models
{
    public class ReportsViewModel
    {
        public IList<ExpenseReportModel> SavedReports { get; set; }
        public IList<ExpenseReportModel> SubmittedReports { get; set; }
        public IList<ExpenseReportModel> RecentlyApprovedReports { get; set; }
    }
}