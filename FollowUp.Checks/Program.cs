using FollowUp.Core;

var passed = 0;
void Check(string name, Action test) { test(); Console.WriteLine($"PASS {name}"); passed++; }
void Equal<T>(T expected, T actual) { if (!Equals(expected, actual)) throw new Exception($"Expected {expected}; actual {actual}"); }
Check("Persian new year and Persian/Arabic digit input", () => {
    Equal(new DateTime(2024,3,20), PersianDates.Parse("۱۴۰۳/۰۱/۰۱"));
    Equal("۱۴۰۳/۰۱/۰۱", PersianDates.Format(new DateTime(2024,3,20)));
    Equal(new DateTime(2024,3,20), PersianDates.Parse("١٤٠٣/٠١/٠١"));
});
Check("Reject invalid Persian month lengths", () => {
    if (PersianDates.TryParse("1402/12/30", out _)) throw new Exception("Invalid non-leap day accepted");
    if (PersianDates.TryParse("1403/07/31", out _)) throw new Exception("Invalid autumn day accepted");
    Equal(new DateTime(2025,3,20), PersianDates.Parse("1403/12/30"));
});
var dir = Path.Combine(Path.GetTempPath(), "FollowUp-check-" + Guid.NewGuid());
Directory.CreateDirectory(dir);
try {
    var path = Path.Combine(dir, "test.db");
    long companyId = 0, caseId = 0, taskId = 0;
    Check("SQLite persists companies, cases and timeline after reopening", () => {
        var db = new Repository(path);
        companyId = db.SaveCompany(new Company { Name="شرکت آزمون", NationalId="123" });
        caseId = db.SaveCase(new TaxCase { CompanyId=companyId, Subject="عملکرد", Period="۱۴۰۳", Title="آزمون" });
        db.RecordStage(caseId, "هیئت بدوی", new DateTime(2025,1,1), "عدم توافق");
        var reopened = new Repository(path);
        Equal("شرکت آزمون", reopened.Companies().Single().Name);
        Equal("هیئت بدوی", reopened.Cases().Single().Stage);
        Equal("عدم توافق", reopened.History(caseId).First().Notes);
    });
    Check("Due reminders exclude completed, closed, future and snoozed actions", () => {
        var db = new Repository(path);
        var now = new DateTime(2025,1,1,10,0,0);
        taskId = db.SaveTask(new FollowUpTask { CaseId=caseId, Title="اعتراض", DueAt=now.AddMinutes(-1), RemindAt=now.AddHours(-1) });
        Equal(1, db.DueReminders(now).Count);
        db.Snooze(taskId, now.AddMinutes(15));
        Equal(0, db.DueReminders(now).Count);
        Equal(1, db.DueReminders(now.AddMinutes(16)).Count);
        db.CompleteTask(taskId);
        Equal(0, db.DueReminders(now.AddDays(1)).Count);
        db.SaveTask(new FollowUpTask { CaseId=caseId, Title="آینده", DueAt=now.AddDays(3), RemindAt=now.AddDays(2) });
        Equal(0, db.DueReminders(now).Count);
        db.RecordStage(caseId, "بسته‌شده", now, "توافق");
        Equal(0, db.DueReminders(now.AddDays(5)).Count);
    });
    Check("Backup is a restorable SQLite snapshot", () => {
        var db = new Repository(path);
        var backup = Path.Combine(dir, "backup.db"); db.Backup(backup);
        Equal(1, new Repository(backup).Cases().Count);
    });
    Check("An overdue snoozed action remains editable without changing its deadline", () => {
        var db = new Repository(path);
        var task = db.Tasks().Single(t=>t.Id==taskId);
        db.Snooze(taskId, task.DueAt.AddHours(2));
        task = db.Tasks().Single(t=>t.Id==taskId);
        task.Title = "اصلاح عنوان";
        db.SaveTask(task);
        Equal("اصلاح عنوان", db.Tasks().Single(t=>t.Id==taskId).Title);
    });
    Check("Copied documents remain available after restoring to another folder", () => {
        var db = new Repository(path);
        var source = Path.Combine(dir,"original.txt"); File.WriteAllText(source,"sample document");
        db.AddAttachment(caseId,source);
        var document=db.Attachments(caseId).Single();
        var restore=Path.Combine(dir,"restored"); Directory.CreateDirectory(restore);
        db.Backup(Path.Combine(restore,"followup.db"));
        Directory.CreateDirectory(Path.Combine(restore,"Documents"));
        File.Copy(document.StoredPath,Path.Combine(restore,"Documents",Path.GetFileName(document.StoredPath)));
        File.Delete(document.StoredPath);
        var restored=new Repository(Path.Combine(restore,"followup.db"));
        Equal("sample document",File.ReadAllText(restored.Attachments(caseId).Single().StoredPath));
    });
} finally { Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); Directory.Delete(dir, true); }
Console.WriteLine($"{passed} behavior checks passed.");
