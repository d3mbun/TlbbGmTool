namespace liuguang.TlbbGmTool.Models;

/// <summary>
/// Một bản ghi vật phẩm
/// </summary>
public class ItemLog
{
    /// <summary>
    /// ID bản ghi
    /// </summary>
    public int Id;
    /// <summary>
    /// Guid nhân vật sở hữu
    /// </summary>
    public int CharGuid;
    /// <summary>
    /// Guid vật phẩm
    /// </summary>
    public int Guid;
    /// <summary>
    /// Số hiệu thế giới
    /// </summary>
    public int World = 101;
    /// <summary>
    /// Số hiệu máy chủ
    /// </summary>
    public int Server = 0;
    /// <summary>
    /// Mã vật phẩm
    /// </summary>
    public int ItemBaseId;
    /// <summary>
    /// Vị trí
    /// </summary>
    public int Pos;
    /// <summary>
    /// P1 - P17
    /// </summary>
    public byte[] PData = new byte[17 * 4];
    /// <summary>
    /// Người chế tạo
    /// </summary>
    public string Creator = string.Empty;
    public bool IsValid = true;
    public int DbVersion = 0;
    public string FixAttr = string.Empty;
    public string TVar = string.Empty;
    public int VisualId = 0;
    public int MaxgemId = -1;
}
