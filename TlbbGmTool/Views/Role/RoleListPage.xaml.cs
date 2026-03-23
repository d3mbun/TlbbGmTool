using liuguang.TlbbGmTool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace liuguang.TlbbGmTool.Views.Role;

/// <summary>
/// Logic tương tác cho RoleListPage.xaml
/// </summary>
public partial class RoleListPage : Page
{
    private bool _vmBind = false;
    public RoleListPage()
    {
        InitializeComponent();
    }
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        if (_vmBind)
        {
            return;
        }
        _vmBind = true;
        //
        var mainWindow = Window.GetWindow(this);
        var mainWindowVm = (MainWindowViewModel)mainWindow.DataContext;
        var vm = (RoleListViewModel)DataContext;
        vm.OwnedWindow = mainWindow;
        vm.Connection = mainWindowVm.MainModel.Connection;
        //MessageBox.Show("bind2");
        mainWindowVm.PropertyChanged += (sender, evt) =>
        {
            // Sau khi ngắt kết nối, xóa danh sách kết quả tìm kiếm
            if (evt.PropertyName == nameof(mainWindowVm.CanDisConnServer))
            {
                if (mainWindowVm.CanConnServer)
                {
                    vm.RoleList.Clear();
                    //MessageBox.Show("clear2...");
                }
            }
        };
    }
}
