using Expenses.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expenses.Web.Models
{
    public class DashboardModel
    {
        public IList<ChargeModel> OutstandingCharges { get; set; }
        public IList<ExpenseReportModel> ReportsInProgress { get; set; }
        public IList<ExpenseReportModel> ReportsPendingApproval { get; set; }
        public IList<ExpenseReportModel> RecentlyApprovedReports { get; set; }
    }
}