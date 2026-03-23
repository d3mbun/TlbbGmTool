namespace liuguang.TlbbGmTool.Models;

/// <summary>
/// Thông tin cơ bản về tâm pháp được lấy từ tệp txt
/// </summary>
public class XinFaBase
{
    /// <summary>
    /// Định nghĩa Tâm Pháp
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// ID môn phái
    /// </summary>
    public readonly int Menpai;

    /// <summary>
    /// Tên
    /// </summary>
    public readonly string Name;

    /// <summary>
    /// Mô tả
    /// </summary>
    public readonly string Description;

    public XinFaBase(int id, int menpai, string name, string description)
    {
        Id = id;
        Menpai = menpai;
        Name = name;
        Description = description;
    }
}
