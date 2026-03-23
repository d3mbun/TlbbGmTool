namespace liuguang.TlbbGmTool.Models;

/// <summary>
/// Trạng thái kết nối CSDL
/// </summary>
public enum DbStatus
{
    //Chưa kết nối
    NotConnect,
    //Đang kết nối
    Connecting,
    //Đã kết nối
    Connected
}
