using liuguang.TlbbGmTool.Models;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.ViewModels;
using System.Windows;

namespace liuguang.TlbbGmTool.Views.Pet;

public partial class PetSelectorWindow : Window
{
    public PetAttrBase? SelectedPet { get; set; }

    public PetSelectorWindow()
    {
        InitializeComponent();
        if (DataContext is ViewModelBase vm)
        {
            vm.OwnedWindow = this;
        }
    }
}
