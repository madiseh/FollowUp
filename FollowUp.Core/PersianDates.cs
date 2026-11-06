using System.Globalization;

namespace FollowUp.Core;

public static class PersianDates
{
    public static string Latin(string value) => string.Concat(value.Select(c =>
        c is >= '۰' and <= '۹' ? (char)('0' + c - '۰') : c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c));
    public static string Digits(string value) => string.Concat(value.Select(c => c is >= '0' and <= '9' ? (char)('۰' + c - '0') : c));
    public static string Format(DateTime date)
    {
        var p = new PersianCalendar();
        return Digits($"{p.GetYear(date):0000}/{p.GetMonth(date):00}/{p.GetDayOfMonth(date):00}");
    }
    public static string Stamp(DateTime date) => Format(date) + "  " + Digits(date.ToString("HH:mm", CultureInfo.InvariantCulture));
    public static DateTime Parse(string text)
    {
        var parts = Latin(text.Trim()).Split('/');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var y) || !int.TryParse(parts[1], out var m) || !int.TryParse(parts[2], out var d) || y < 1200 || y > 1600)
            throw new FormatException("تاریخ شمسی را مانند ۱۴۰۵/۰۸/۱۵ وارد کنید.");
        try { return new PersianCalendar().ToDateTime(y,m,d,0,0,0,0); }
        catch (ArgumentOutOfRangeException) { throw new FormatException("روز یا ماه تاریخ شمسی معتبر نیست."); }
    }
    public static bool TryParse(string text, out DateTime result)
    {
        try { result = Parse(text); return true; } catch (FormatException) { result = default; return false; }
    }
    public static DateTime ParseTime(string date, string time)
    {
        if (!TimeSpan.TryParseExact(Latin(time.Trim()), @"hh\:mm", CultureInfo.InvariantCulture, out var t) || t.TotalHours >= 24)
            throw new FormatException("ساعت را مانند ۰۹:۳۰ وارد کنید.");
        return Parse(date).Add(t);
    }
}
