namespace FollowUp.Core;

public sealed class Company
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string NationalId { get; set; } = "";
    public string Contact { get; set; } = "";
    public string Notes { get; set; } = "";
    public override string ToString() => Name;
}
public sealed class TaxCase
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string CompanyName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Subject { get; set; } = "عملکرد";
    public string Period { get; set; } = "";
    public string FileNumber { get; set; } = "";
    public string Stage { get; set; } = "برگه تشخیص";
    public string Owner { get; set; } = "";
    public string Notes { get; set; } = "";
    public string Display => $"{CompanyName} — {Title} / {Subject} / {Period}";
    public override string ToString() => Display;
}
public sealed class FollowUpTask
{
    public long Id { get; set; }
    public long CaseId { get; set; }
    public string CaseTitle { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime DueAt { get; set; }
    public DateTime RemindAt { get; set; }
    public bool IsDone { get; set; }
    public bool IsClosed { get; set; }
    public string Notes { get; set; } = "";
    public string DueText => PersianDates.Stamp(DueAt);
    public string RemindText => PersianDates.Stamp(RemindAt);
    public string Status => IsDone ? "انجام‌شده" : IsClosed ? "پرونده بسته" : DueAt < DateTime.Now ? "عقب‌افتاده" : DueAt.Date == DateTime.Today ? "امروز" : "در انتظار";
}
public sealed record StageEntry(long Id, string Stage, DateTime Date, string Notes)
{
    public string DateText => PersianDates.Format(Date);
}
public sealed record Attachment(long Id, string Name, string StoredPath, DateTime AddedAt)
{
    public string AddedText => PersianDates.Stamp(AddedAt);
}
public static class Workflow
{
    public static readonly string[] Subjects = ["عملکرد", "حقوق", "ارزش افزوده", "ماده ۱۶۹", "تکلیفی", "سایر"];
    public static readonly string[] Stages = ["برگه تشخیص", "اعتراض به تشخیص", "رسیدگی ماده ۲۳۸", "هیئت بدوی", "اعتراض به رأی بدوی", "هیئت تجدیدنظر", "هیئت ۲۱۶", "ماده ۲۵۱ مکرر", "ماده ۲۵۱", "ماده ۲۴۳", "دیوان عدالت اداری", "بسته‌شده"];
}
