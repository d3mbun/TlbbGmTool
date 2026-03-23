using liuguang.TlbbGmTool.Common;

namespace liuguang.TlbbGmTool.Models;

public class GameServer
{
    /// <summary>
    /// Tên máy chủ
    /// </summary>
    public string ServerName = string.Empty;

    /// <summary>
    /// Host CSDL
    /// </summary>
    public string DbHost = string.Empty;

    /// <summary>
    /// Cổng
    /// </summary>
    public ushort DbPort = 3306;

    /// <summary>
    /// Tên CSDL tài khoản
    /// </summary>
    public string AccountDbName = "web";

    /// <summary>
    /// Tên CSDL game
    /// </summary>
    public string GameDbName = "tlbbdb";

    /// <summary>
    /// User CSDL
    /// </summary>
    public string DbUser = "root";

    /// <summary>
    /// Mật khẩu CSDL
    /// </summary>
    public string DbPassword = string.Empty;

    /// <summary>
    /// Vô hiệu hóa SSL
    /// </summary>
    public bool DisabledSsl = true;

    /// <summary>
    /// Loại server
    /// </summary>
    public ServerType GameServerType = ServerType.Common;

    /// <summary>
    /// Đường dẫn client
    /// </summary>
    public string ClientPath = string.Empty;
}
