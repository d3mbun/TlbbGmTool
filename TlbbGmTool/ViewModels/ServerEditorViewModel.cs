using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Services;

namespace liuguang.TlbbGmTool.ViewModels;

public class ServerEditorViewModel : ViewModelBase
{
    #region Fields
    private bool _isConnecting = false;
    private GameServerViewModel? _inputServerInfo;
    private readonly GameServerViewModel _serverInfo = new(new());
    private readonly List<ComboBoxNode<ServerType>> _serverTypes = [];
    private ComboBoxNode<ServerType> _selectedNode;
    #endregion

    #region Properties
    public GameServerViewModel ServerInfo
    {
        get => _serverInfo;
        set
        {
            _inputServerInfo = value;
            _serverInfo.CopyFrom(value);
            RaisePropertyChanged(nameof(WindowTitle));
        }
    }
    public ObservableCollection<GameServerViewModel>? ServerList { get; set; }

    public string WindowTitle => (_inputServerInfo is null) ? "Thêm máy chủ" : "Chỉnh sửa máy chủ";
    public List<ComboBoxNode<ServerType>> ServerTypes => _serverTypes;
    public ComboBoxNode<ServerType> SelectedNode
    {
        get => _selectedNode;
        set => SetProperty(ref _selectedNode, value);
    }

    public Command SaveServerCommand { get; }

    public Command ConnTestCommand { get; }

    public Command ChoseFolderCommand { get; }
    #endregion

    public ServerEditorViewModel()
    {
        ConnTestCommand = new(TryToConnect, CanTestConn);
        ChoseFolderCommand = new(ShowFolderDialog);
        SaveServerCommand = new(SaveServerConfig, CanSave);
        ServerInfo.PropertyChanged += (sender, e) =>
        {
            SaveServerCommand.RaiseCanExecuteChanged();
            ConnTestCommand.RaiseCanExecuteChanged();
        };
        _serverTypes.Add(new("Bản truyền thống", ServerType.Common));
        _serverTypes.Add(new("Bản hoài cổ", ServerType.HuaiJiu));
        _selectedNode = _serverTypes[0];
    }

    private void ShowFolderDialog()
    {
        using var dialog = new FolderBrowserDialog();
        if (!string.IsNullOrEmpty(ServerInfo.ClientPath))
        {
            if (Directory.Exists(ServerInfo.ClientPath))
            {
                dialog.SelectedPath = ServerInfo.ClientPath;
            }
        }
        var result = dialog.ShowDialog();
        if (result == DialogResult.OK)
        {
            ServerInfo.ClientPath = dialog.SelectedPath;
        }
    }
    private bool CanSave()
    {
        return !(string.IsNullOrEmpty(ServerInfo.ServerName) || string.IsNullOrEmpty(ServerInfo.DbHost)
            || string.IsNullOrEmpty(ServerInfo.AccountDbName) || string.IsNullOrEmpty(ServerInfo.GameDbName)
            || string.IsNullOrEmpty(ServerInfo.DbUser) || string.IsNullOrEmpty(ServerInfo.ClientPath));
    }


    private bool CanTestConn()
    {
        return !(_isConnecting || string.IsNullOrEmpty(ServerInfo.DbHost)
            || string.IsNullOrEmpty(ServerInfo.AccountDbName) || string.IsNullOrEmpty(ServerInfo.GameDbName)
            || string.IsNullOrEmpty(ServerInfo.DbUser));
    }

    private async void SaveServerConfig()
    {
        // Kiểm tra thư mục client có hợp lệ không
        var configAxpPath = Path.Combine(ServerInfo.ClientPath, "Data", "Config.axp");
        if (!File.Exists(configAxpPath))
        {
            ShowErrorMessage("Đường dẫn không hợp lệ", $"Đường dẫn client [{ServerInfo.ClientPath}] không hợp lệ");
            return;
        }
        ServerInfo.GameServerType = _selectedNode.Value;
        //
        if (_inputServerInfo is null)
        {
            //add
            ServerList?.Add(ServerInfo);
        }
        else
        {
            //update
            _inputServerInfo.CopyFrom(ServerInfo);
        }
        // Lưu cấu hình
        var serverList = from item in ServerList select item.AsServer();
        if (serverList is null)
        {
            return;
        }
        try
        {
            await ServerService.SaveGameServersAsync(serverList);
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lưu cấu hình thất bại", ex);
            return;
        }
        OwnedWindow?.Close();
    }

    private async void TryToConnect()
    {
        _isConnecting = true;
        ConnTestCommand.RaiseCanExecuteChanged();
        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = ServerInfo.DbHost,
            Port = ServerInfo.DbPort,
            Database = ServerInfo.AccountDbName,
            UserID = ServerInfo.DbUser,
            Password = ServerInfo.DbPassword,
            ConnectionTimeout = 20,
        };
        if (ServerInfo.DisabledSsl)
        {
            connectionStringBuilder.SslMode = MySqlSslMode.Disabled;
        }

        var mySqlConnection = new MySqlConnection
        {
            ConnectionString = connectionStringBuilder.ConnectionString,
        };
        try
        {
            // Chạy ngầm, tránh tắc nghẽn luồng UI
            await Task.Run(async () =>
            {
                await mySqlConnection.OpenAsync();
                await mySqlConnection.CloseAsync();

            });
        }
        catch (Exception e)
        {
            ShowErrorMessage("Kết nối thất bại", e);
            return;
        }
        finally
        {

            _isConnecting = false;
            ConnTestCommand.RaiseCanExecuteChanged();
        }
        ShowMessage("Kết nối thành công", "Kết nối cơ sở dữ liệu thành công");
    }
}
