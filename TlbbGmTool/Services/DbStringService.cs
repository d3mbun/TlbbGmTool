using System.Text;

namespace liuguang.TlbbGmTool.Services;

/// <summary>
/// Chuyển đổi mã hóa
/// </summary>
public static class DbStringService
{
    /// <summary>
    /// Mã hóa văn bản: Tiếng Trung giản thể (GB18030)
    /// </summary>
    private static readonly Encoding StrEncoding = Encoding.GetEncoding("GB18030");

    /// <summary>
    /// Mã hóa lưu trữ CSDL: Tây Âu (ISO)
    /// </summary>
    private static readonly Encoding StorageEncoding = Encoding.GetEncoding("iso-8859-1");

    /// <summary>
    /// Giải mã chuỗi từ CSDL sang chuỗi thông thường
    /// </summary>
    /// <param name="dbString"></param>
    /// <returns></returns>
    public static string ToCommonString(string dbString)
    {
        var bytes = StorageEncoding.GetBytes(dbString);
        return StrEncoding.GetString(bytes);
    }

    /// <summary>
    /// Mã hóa chuỗi thông thường sang mã hóa của CSDL
    /// </summary>
    /// <param name="commonString"></param>
    /// <returns></returns>
    public static string ToDbString(string commonString)
    {
        var bytes = StrEncoding.GetBytes(commonString);
        return StorageEncoding.GetString(bytes);
    }
}
