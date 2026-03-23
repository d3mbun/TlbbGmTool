using System.Collections.Generic;
using System.Collections.ObjectModel;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.ViewModels;

namespace liuguang.TlbbGmTool.Models;

public class MainWindowModel
{
    /// <summary>
    /// Trạng thái tải dữ liệu
    /// </summary>
    public DataStatus DataStatus = DataStatus.NotLoad;

    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection Connection = new();

    /// <summary>
    /// Phiên bản CSDL
    /// </summary>
    public string DbVersion = string.Empty;

    /// <summary>
    /// server list
    /// </summary>
    public ObservableCollection<GameServerViewModel> ServerList = new();
}
