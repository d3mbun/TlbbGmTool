namespace liuguang.TlbbGmTool.Models;

/// <summary>
/// Một bản ghi tài khoản
/// </summary>
public class UserAccount
{
    public int Id;
    public string Name = string.Empty;
    public string Password = string.Empty;
    public string? Question;
    public string? Answer;
    public string? Email;
    public string? IdCard;
    public int Point;
}
