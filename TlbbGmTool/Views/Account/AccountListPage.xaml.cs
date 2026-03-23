using liuguang.TlbbGmTool.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace liuguang.TlbbGmTool.Views.Account;

/// <summary>
/// Logic tương tác cho AccountListPage.xaml
/// </summary>
public partial class AccountListPage : Page
{
    private bool _vmBind = false;
    public AccountListPage()
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
        var vm = (AccountListViewModel)DataContext;
        vm.OwnedWindow = mainWindow;
        vm.Connection = mainWindowVm.MainModel.Connection;
        //MessageBox.Show("bind1");
        mainWindowVm.PropertyChanged += (sender, evt) =>
        {
            // Sau khi ngắt kết nối, xóa danh sách kết quả tìm kiếm
            if (evt.PropertyName == nameof(mainWindowVm.CanDisConnServer))
            {
                if (mainWindowVm.CanConnServer)
                {
                    vm.AccountList.Clear();
                    //MessageBox.Show("clear1...");
                }
            }
        };
    }
}
