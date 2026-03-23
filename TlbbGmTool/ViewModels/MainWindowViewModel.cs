using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using liuguang.Dbc;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Models;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.Views.About;
using liuguang.TlbbGmTool.Views.Server;

namespace liuguang.TlbbGmTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #region Fields
    private MainWindowModel _mainWindowModel = new();
    public GameServerViewModel? _selectedServer;
    private DbStatus _currentDbStatus = DbStatus.NotConnect;
    #endregion

    #region Properties
    public override Window? OwnedWindow => Application.Current.MainWindow;
    /// <summary>
    /// Gọi từ Page
    /// </summary>
    public MainWindowModel MainModel => _mainWindowModel;
    public string WindowTitle
    {
        get
        {
            var title = "TLBB GM Tool - by Lưu Quang";
#if DEBUG
            title = "[debug]" + title;
#endif
            if (!string.IsNullOrEmpty(_mainWindowModel.DbVersion))
            {
                title += $"(MySQL: {_mainWindowModel.DbVersion})";
            }

            if (_mainWindowModel.DataStatus == DataStatus.Loading)
            {
                title += "(Đang tải cấu hình...)";
            }

            return title;
        }
    }

    private DataStatus DataStatus
    {
        set
        {
            if (SetProperty(ref _mainWindowModel.DataStatus, value))
            {
                RaisePropertyChanged(nameof(WindowTitle));
                RaisePropertyChanged(nameof(GameDataLoaded));
            }
        }
    }

    /// <summary>
    /// Dữ liệu game đã tải xong chưa
    /// </summary>
    public bool GameDataLoaded
        => _mainWindowModel.DataStatus == DataStatus.Loaded;

    public GameServerViewModel? SelectedServer
    {
        get => _selectedServer;
        set
        {
            if (SetProperty(ref _selectedServer, value))
            {
                RaisePropertyChanged(nameof(CanConnServer));
                ConnectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<GameServerViewModel> ServerList => _mainWindowModel.ServerList;

    private DbStatus CurrentDbStatus
    {
        set
        {
            if (_selectedServer is null)
            {
                return;
            }
            if (SetProperty(ref _currentDbStatus, value))
            {
                _selectedServer.DbStatus = value;
                RaisePropertyChanged(nameof(CanConnServer));
                RaisePropertyChanged(nameof(CanDisConnServer));
                ConnectCommand.RaiseCanExecuteChanged();
                DisConnectCommand.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Có thể kết nối không
    /// </summary>
    /// <returns></returns>
    public bool CanConnServer
    {
        get
        {
            if (_selectedServer is null)
            {
                return false;
            }
            return _selectedServer.DbStatus == DbStatus.NotConnect;
        }
    }


    /// <summary>
    /// Có thể ngắt kết nối không
    /// </summary>
    public bool CanDisConnServer
    {
        get
        {
            if (_selectedServer is null)
            {
                return false;
            }
            return _selectedServer.DbStatus == DbStatus.Connected;
        }
    }

    public Command ConnectCommand { get; }

    public Command DisConnectCommand { get; }

    public Command ExitCommand { get; }

    public Command ServerListCommand { get; }

    public Command AboutCommand { get; }
    #endregion


    public MainWindowViewModel()
    {
        ConnectCommand = new(ConnectDbAsync, () => CanConnServer);
        DisConnectCommand = new(DisConnectDb, () => CanDisConnServer);
        ExitCommand = new(Application.Current.Shutdown);
        ServerListCommand = new(ShowServerListWindow);
        AboutCommand = new(ShowAboutWindow);
        ServerList.CollectionChanged += (sender, e) =>
        {
            if (ServerList.Count == 0)
            {
                return;
            }
            // Mặc định chọn cái đầu tiên
            if (_selectedServer is null || !ServerList.Contains(_selectedServer))
            {
                SelectedServer = ServerList.First();
            }
        };
    }

    public async void ConnectDbAsync()
    {
        if (_selectedServer is null)
        {
            return;
        }
        CurrentDbStatus = DbStatus.Connecting;
        try
        {
            await Task.Run(async () =>
            {
                await _mainWindowModel.Connection.OpenAsync(_selectedServer.AsServer());
                _mainWindowModel.DbVersion = await _mainWindowModel.Connection.CheckVersionAsync();
            });
            RaisePropertyChanged(nameof(WindowTitle));
        }
        catch (Exception e)
        {
            CurrentDbStatus = DbStatus.NotConnect;
            ShowErrorMessage("Kết nối cơ sở dữ liệu thất bại", e);
            return;
        }
        CurrentDbStatus = DbStatus.Connected;
        // Đặt lại tùy chọn
        LvItemSelectorViewModel.ResetLastData();
        // CSDL đã từng kết nối thành công
        // Tải dữ liệu từ tệp axp của client
        this.DataStatus = DataStatus.Loading;
        try
        {
            await Task.Run(async () =>
            {
                var dirPath = Path.Combine(_selectedServer.ClientPath, "Data", "Config");
                var axpPath = Path.Combine(_selectedServer.ClientPath, "Data", "Config.axp");
                await AxpService.LoadDataAsync(dirPath, axpPath, _selectedServer.GameServerType);
            });
            this.DataStatus = DataStatus.Loaded;
        }
        catch (Exception ex)
        {
            this.DataStatus = DataStatus.NotLoad;
            var stackTrace = (ex.InnerException ?? ex).StackTrace;
            ShowErrorMessage("Tải tệp txt thất bại", $"{ex.Message}\n{stackTrace}");
        }
    }

    public async void DisConnectDb()
    {
        if (_selectedServer is null)
        {
            return;
        }
        try
        {
            await _mainWindowModel.Connection.CloseAsync();
        }
        catch (Exception e)
        {
            ShowErrorMessage("Ngắt kết nối cơ sở dữ liệu thất bại", e);
        }
        _mainWindowModel.DbVersion = string.Empty;
        RaisePropertyChanged(nameof(WindowTitle));
        CurrentDbStatus = DbStatus.NotConnect;
        this.DataStatus = DataStatus.NotLoad;
    }

    /// <summary>
    /// Giải phóng tài nguyên trước khi đóng
    /// </summary>
    /// <returns></returns>
    public async Task FreeResourceAsync()
    {
        if (_selectedServer != null)
        {
            if (_selectedServer.DbStatus == DbStatus.Connected)
            {
                try
                {
                    await _mainWindowModel.Connection.CloseAsync();
                }
                catch (Exception)
                {
                }
            }
        }
    }

    /// <summary>
    /// Tải dữ liệu
    /// </summary>
    /// <returns></returns>
    public async Task LoadDataAsync()
    {
        var taskList = new Task[]{
            LoadServerListAsync(),
            CommonConfigService.LoadConfigAsync(SharedData.MenpaiMap, SharedData.Attr0Map, SharedData.Attr1Map)
        };
        try
        {
#if NET
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            DbcFile.UseViscii = true;
            await Task.WhenAll(taskList);
        }
        catch (Exception e)
        {
            ShowErrorMessage("Lỗi khi tải cấu hình", e);
        }
    }

    /// <summary>
    /// Tải danh sách cấu hình máy chủ
    /// </summary>
    /// <returns></returns>
    private async Task LoadServerListAsync()
    {
        var servers = await ServerService.LoadServersAsync();
        foreach (var item in servers)
        {
            var server = new GameServerViewModel(item);
            ServerList.Add(server);
        }
    }

    private void ShowServerListWindow()
    {
        ShowDialog(new ServerListWindow(), (ServerListViewModel vm) =>
        {
            vm.ServerList = ServerList;
        });
    }

    private void ShowAboutWindow()
    {
        ShowDialog(new AboutWindow());
    }
}
