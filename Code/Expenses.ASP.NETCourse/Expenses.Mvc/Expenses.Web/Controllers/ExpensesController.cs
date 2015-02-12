using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Expenses.Data;
using Expenses.Web.Models;
using Newtonsoft.Json;

namespace Expenses.Web.Controllers
{
    public class ExpensesController : Controller
    {
        private ExpensesContext db = new ExpensesContext();

        /// <summary>
        /// Extract user credentials from request.
        /// </summary>
        /// <returns>EmployeeId and full employee name of current user</returns>
        /// <remarks>Current implementation uses hard-coded user for demonstration purposes. 
        /// A real world application is supposed to extract user from an authentication token.</remarks>
        private async Task<Tuple<int, string>> GetEmployee()
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Alias == ExpensesDemoData.DefaultEmployeeAlias);
            return Tuple.Create(employee.EmployeeId, employee.Name);
        }

        // GET: Expenses
        public async Task<ActionResult> Index()
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            var outstandingCharges = await db.Charges
                .Where(c => (c.EmployeeId == employeeId) && !c.ExpenseReportId.HasValue)
                .ToListAsync();

            var reportsPendingApproval = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && r.Status == DbExpenseReportStatus.Submitted)
                .ToListAsync();

            var reportsInProgress = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && r.Status == DbExpenseReportStatus.Saved)
                .ToListAsync();

            DateTime recentDate = DateTime.Now - TimeSpan.FromDays(90.0);
            var recentlyApprovedReports = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && (r.Status == DbExpenseReportStatus.Approved) && (r.DateResolved >= recentDate))
                .ToListAsync();

            return View(new DashboardModel()
                {
                    OutstandingCharges = outstandingCharges.Select(c => new ChargeModel(c)).ToList(),
                    ReportsPendingApproval = reportsPendingApproval.Select(r => new ExpenseReportModel(r)).ToList(),
                    ReportsInProgress = reportsInProgress.Select(r => new ExpenseReportModel(r)).ToList(),
                    RecentlyApprovedReports = recentlyApprovedReports.Select(r => new ExpenseReportModel(r)).ToList(),
                });
        }

        // GET: Expenses/Charges
        public async Task<ActionResult> Charges()
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            var outstandingCharges = await db.Charges
                .Where(c => (c.EmployeeId == employeeId) && !c.ExpenseReportId.HasValue)
                .ToListAsync();

            return View(outstandingCharges);
        }

        // GET: Expenses/NewCharge
        public async Task<ActionResult> NewCharge()
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            var newCharge = new ChargeModel()
            {
                 ExpenseDate = DateTime.Today,
            };

            return View(newCharge);
        }

        // POST: Expenses/NewCharge
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewCharge(ChargeModel chargeModel)
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            if (ModelState.IsValid)
            {
                db.Charges.Add(chargeModel.ConvertToDbCharge(employeeId));
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(chargeModel);
        }

        // GET: Expenses/Charge/5
        public async Task<ActionResult> Charge(int? id)
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbCharge dbCharge = await db.Charges.FindAsync(id);
            if (dbCharge == null)
            {
                return HttpNotFound();
            }
            return View(new ChargeModel(dbCharge));
        }

        // POST: Expenses/Charge/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Charge(ChargeModel chargeModel)
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            if (ModelState.IsValid)
            {
                var dbCharge = chargeModel.ConvertToDbCharge(employeeId);

                db.Entry(dbCharge).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(chargeModel);
        }

        // GET: Expenses/DeleteCharge/5
        public async Task<ActionResult> DeleteCharge(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbCharge dbCharge = await db.Charges.FindAsync(id);
            if (dbCharge == null)
            {
                return HttpNotFound();
            }
            return View(new ChargeModel(dbCharge));
        }

        // POST: Expenses/DeleteCharge/5
        [HttpPost, ActionName("DeleteCharge")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteChargeConfirmed(int id)
        {
            DbCharge dbCharge = await db.Charges.FindAsync(id);
            db.Charges.Remove(dbCharge);
            await db.SaveChangesAsync();
            return RedirectToAction("Charges");
        }

        // GET: Expenses/Reports
        public async Task<ActionResult> Reports()
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            var savedReports = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && r.Status == DbExpenseReportStatus.Saved)
                .ToListAsync();

            var submittedReports = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && r.Status == DbExpenseReportStatus.Submitted)
                .ToListAsync();

            DateTime recentApprovalCutoffDate = DateTime.Today - TimeSpan.FromDays(90.0);
            var recentlyApprovedReports = await db.Reports
                .Where(r => (r.EmployeeId == employeeId) && r.Status == DbExpenseReportStatus.Approved && r.DateResolved > recentApprovalCutoffDate)
                .ToListAsync();

            return View(new ReportsViewModel()
            {
                SavedReports = savedReports.Select(r => new ExpenseReportModel(r)).ToList(),
                SubmittedReports = submittedReports.Select(r => new ExpenseReportModel(r)).ToList(),
                RecentlyApprovedReports = recentlyApprovedReports.Select(r => new ExpenseReportModel(r)).ToList(),
            });
        }

        // GET: Expenses/NewReport
        public async Task<ActionResult> NewReport()
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Alias == ExpensesDemoData.DefaultEmployeeAlias);
            var employeeId = employee.EmployeeId;
            var manager = employee.Manager;
            ViewBag.UserName = employee.Name;

            var outstandingCharges = await db.Charges
                .Where(c => c.EmployeeId == employeeId && !c.ExpenseReportId.HasValue)
                .ToListAsync();

            var newReport = new ExpenseReportModel()
            {
                Approver = manager,
                CostCenter = 1055,
                EmployeeId = employeeId,
                OutstandingCharges = outstandingCharges.Select(c => new ChargeModel(c)).ToList(),
            };

            return View(newReport);
        }

        // POST: Expenses/NewReport
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewReport(
            ExpenseReportModel expenseReportModel,
            string associatedChargesIds,
            string outstandingChargesIds,
            int? addCharge,
            int? removeCharge)
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Alias == ExpensesDemoData.DefaultEmployeeAlias);
            var employeeId = employee.EmployeeId;
            ViewBag.UserName = employee.Name;

            expenseReportModel.AssociatedCharges = await GetChargesFromSerializedIdList(associatedChargesIds, employeeId);
            expenseReportModel.OutstandingCharges = await GetChargesFromSerializedIdList(outstandingChargesIds, employeeId);

            if (addCharge.HasValue)
            {
                // the request was to add a charge to the report
                ReplaceChargeObjectBetweenLists(addCharge.Value, expenseReportModel.OutstandingCharges, expenseReportModel.AssociatedCharges);
            }
            else if (removeCharge.HasValue)
            {
                // the request was to remove a charge from the report
                ReplaceChargeObjectBetweenLists(removeCharge.Value, expenseReportModel.AssociatedCharges, expenseReportModel.OutstandingCharges);
            }
            else
            {
                // Submit request
                if (ModelState.IsValid)
                {
                    expenseReportModel.EmployeeId = employeeId;
                    var dbExpenseReport = new DbExpenseReport()
                        {
                            Amount = expenseReportModel.Amount,
                            Approver = expenseReportModel.Approver,
                            Charges = expenseReportModel.AssociatedCharges.Select(c => c.ConvertToDbCharge(employeeId)).ToList(),
                            CostCenter = expenseReportModel.CostCenter,
                            DateResolved = expenseReportModel.DateResolved,
                            DateSubmitted = expenseReportModel.DateSubmitted,
                            Employee = employee,
                            EmployeeId = expenseReportModel.EmployeeId,
                            ExpenseReportId = expenseReportModel.Id,
                            Notes = expenseReportModel.Notes,
                            Status = (DbExpenseReportStatus)expenseReportModel.Status,
                        };
                    db.Reports.Add(dbExpenseReport);
                    await db.SaveChangesAsync();

                    // add charges to report
                    foreach (var c in expenseReportModel.AssociatedCharges)
                    {
                        var dbCharge = await db.Charges.FindAsync(c.Id);
                        dbCharge.ExpenseReportId = dbExpenseReport.ExpenseReportId;
                        db.Entry(dbCharge).State = EntityState.Modified;
                    }

                    await db.SaveChangesAsync();

                    return RedirectToAction("Report", new { id = dbExpenseReport.ExpenseReportId });
                }
            }

            // This is required to update hidden fields in the view.
            ModelState.Clear();

            // Recalculate the total expense amount, since we have added or removed some charges.
            expenseReportModel.Amount = expenseReportModel.AssociatedCharges.Sum(c => c.BilledAmount);

            return View(expenseReportModel);
        }

        // GET: Expenses/Report/5
        public async Task<ActionResult> Report(int? id)
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbExpenseReport dbReport = await db.Reports.FindAsync(id);
            if (dbReport == null)
            {
                return HttpNotFound();
            }

            var associatedCharges = await db.Charges
                .Where(c => c.EmployeeId == employeeId && c.ExpenseReportId.HasValue && c.ExpenseReportId.Value == id)
                .ToListAsync();

            var outstandingCharges = await db.Charges
                .Where(c => c.EmployeeId == employeeId && !c.ExpenseReportId.HasValue)
                .ToListAsync();

            var report = new ExpenseReportModel(dbReport);
            report.HasUnsavedChanges = false;
            report.EmployeeId = employeeId;
            report.AssociatedCharges = associatedCharges.Select(c => new ChargeModel(c)).ToList();
            report.OutstandingCharges = outstandingCharges.Select(c => new ChargeModel(c)).ToList();

            return View(report);
        }

        // POST: Expenses/Report/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Report(
            ExpenseReportModel expenseReportModel,
            string associatedChargesIds,
            string outstandingChargesIds,
            string command,
            int? addCharge,
            int? removeCharge)
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Alias == ExpensesDemoData.DefaultEmployeeAlias);
            var employeeId = employee.EmployeeId;
            ViewBag.UserName = employee.Name;

            expenseReportModel.AssociatedCharges = await GetChargesFromSerializedIdList(associatedChargesIds, employeeId);
            expenseReportModel.OutstandingCharges = await GetChargesFromSerializedIdList(outstandingChargesIds, employeeId);

            if (addCharge.HasValue)
            {
                // the request was to add a charge to the report
                ReplaceChargeObjectBetweenLists(addCharge.Value, expenseReportModel.OutstandingCharges, expenseReportModel.AssociatedCharges);
                expenseReportModel.HasUnsavedChanges = true;
            }
            else if (removeCharge.HasValue)
            {
                // the request was to remove a charge from the report
                ReplaceChargeObjectBetweenLists(removeCharge.Value, expenseReportModel.AssociatedCharges, expenseReportModel.OutstandingCharges);
                expenseReportModel.HasUnsavedChanges = true;
            }
            else if (!String.IsNullOrEmpty(command) && command == "Save")
            {
                // Submit request
                if (ModelState.IsValid)
                {
                    var dbExpenseReport = await db.Reports.FindAsync(expenseReportModel.Id);
                    dbExpenseReport.Amount = expenseReportModel.Amount;
                    dbExpenseReport.Notes = expenseReportModel.Notes;
                    db.Entry(dbExpenseReport).State = EntityState.Modified;

                    // add charges to report
                    foreach (var c in expenseReportModel.AssociatedCharges)
                    {
                        var dbCharge = await db.Charges.FindAsync(c.Id);
                        dbCharge.ExpenseReportId = dbExpenseReport.ExpenseReportId;
                        db.Entry(dbCharge).State = EntityState.Modified;
                    }

                    // remove charges from report
                    foreach (var c in expenseReportModel.OutstandingCharges)
                    {
                        var dbCharge = await db.Charges.FindAsync(c.Id);
                        dbCharge.ExpenseReportId = null;
                        db.Entry(dbCharge).State = EntityState.Modified;
                    }

                    await db.SaveChangesAsync();

                    expenseReportModel.HasUnsavedChanges = false;
                }
            }
            else if (!String.IsNullOrEmpty(command) && command == "SubmitForApproval")
            {
                // Submit request
                if (ModelState.IsValid)
                {
                    var dbExpenseReport = await db.Reports.FindAsync(expenseReportModel.Id);

                    if (dbExpenseReport.Status != DbExpenseReportStatus.Saved)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    dbExpenseReport.Amount = expenseReportModel.Amount;
                    dbExpenseReport.Notes = expenseReportModel.Notes;
                    dbExpenseReport.Status = DbExpenseReportStatus.Submitted;
                    dbExpenseReport.DateSubmitted = DateTime.Today;
                    db.Entry(dbExpenseReport).State = EntityState.Modified;

                    await db.SaveChangesAsync();

                    expenseReportModel.DateSubmitted = DateTime.Today;
                    expenseReportModel.Status = ExpenseReportStatus.Submitted;
                    expenseReportModel.HasUnsavedChanges = false;
                }
            }
            else if (!String.IsNullOrEmpty(command) && command == "Cancel")
            {
                return RedirectToAction("Reports");
            }

            // This is required to update hidden fields in the view.
            ModelState.Clear();

            // Recalculate the total expense amount, since we have added or removed some charges.
            expenseReportModel.Amount = expenseReportModel.AssociatedCharges.Sum(c => c.BilledAmount);

            return View(expenseReportModel);
        }

        private static void ReplaceChargeObjectBetweenLists(int id, IList<ChargeModel> fromList, IList<ChargeModel> toList)
        {
            int index = 0;
            for ( ; index < fromList.Count; index++)
            {
                if (fromList[index].Id == id)
                {
                    break;
                }
            }

            if (index >= fromList.Count)
            {
                // cannot find element in list
                return;
            }

            var charge = fromList[index];
            toList.Add(charge);
            fromList.RemoveAt(index);
        }

        // GET: Expenses/Report/5
        public async Task<ActionResult> ViewReport(int? id)
        {
            var employee = await this.GetEmployee();
            var employeeId = employee.Item1;
            ViewBag.UserName = employee.Item2;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbExpenseReport dbReport = await db.Reports.FindAsync(id);
            if (dbReport == null)
            {
                return HttpNotFound();
            }

            var associatedCharges = await db.Charges
                .Where(c => c.EmployeeId == employeeId && c.ExpenseReportId.HasValue && c.ExpenseReportId.Value == id)
                .ToListAsync();

            var outstandingCharges = await db.Charges
                .Where(c => c.EmployeeId == employeeId && !c.ExpenseReportId.HasValue)
                .ToListAsync();

            var report = new ExpenseReportModel(dbReport);
            report.EmployeeId = employeeId;
            report.AssociatedCharges = associatedCharges.Select(c => new ChargeModel(c)).ToList();
            report.OutstandingCharges = outstandingCharges.Select(c => new ChargeModel(c)).ToList();

            return View(report);
        }

        // POST: Expenses/ViewReport/5
        [HttpPost, ActionName("ViewReport")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ApproveReportPostback(int? Id, string command)
        {
            if (Id.HasValue && !String.IsNullOrEmpty(command))
            {
                DbExpenseReport dbReport = await db.Reports.FindAsync(Id.Value);

                if (dbReport == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                if (dbReport.Status != DbExpenseReportStatus.Submitted)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                if (command == "Approve")
                {
                    dbReport.Status = DbExpenseReportStatus.Approved;
                    dbReport.DateResolved = DateTime.Today;
                }
                else if (command == "Reject")
                {
                    dbReport.Status = DbExpenseReportStatus.Saved;
                }

                db.Entry(dbReport).State = EntityState.Modified;
                await db.SaveChangesAsync();

                return RedirectToAction("ViewReport", new { id = Id });
            }

            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: Expenses/DeleteReport/5
        public async Task<ActionResult> DeleteReport(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DbExpenseReport dbReport = await db.Reports.FindAsync(id);
            if (dbReport == null)
            {
                return HttpNotFound();
            }
            return View(new ExpenseReportModel(dbReport));
        }

        // POST: Expenses/DeleteReport/5
        [HttpPost, ActionName("DeleteReport")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteReportConfirmed(int id)
        {
            // find all associated charges and dis-associate them
            var charges = await db.Charges
                .Where(c => c.ExpenseReportId.HasValue && c.ExpenseReportId.Value == id)
                .ToListAsync();

            foreach (var c in charges)
            {
                c.ExpenseReportId = null;
                db.Entry(c).State = EntityState.Modified;
            }

            DbExpenseReport dbReport = await db.Reports.FindAsync(id);
            db.Reports.Remove(dbReport);
            await db.SaveChangesAsync();
            return RedirectToAction("Reports");
        }

        // GET: Expenses/Settings
        public ActionResult Settings()
        {
            return View();
        }

        // POST: Expenses/Settings
        [HttpPost, ActionName("Settings")]
        [ValidateAntiForgeryToken]
        public ActionResult ResetDatabaseConfirmed()
        {
            ExpensesDemoData.CleanRepository();
            ExpensesDemoData.CreateNewDemoEmployee(ExpensesDemoData.DefaultEmployeeAlias);

            return RedirectToAction("Index");
        }

        private async Task<IList<ChargeModel>> GetChargesFromSerializedIdList(string jsonChargeIds, int employeeId)
        {
            if (!String.IsNullOrEmpty(jsonChargeIds))
            {
                var ids = JsonConvert.DeserializeObject<List<int>>(jsonChargeIds);
                var charges = await db.Charges
                    .Where(c => ids.Contains(c.ChargeId) && c.EmployeeId == employeeId)
                    .OrderBy(c => c.ChargeId)
                    .ToListAsync();
                return charges.Select(c => new ChargeModel(c)).ToList();
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
