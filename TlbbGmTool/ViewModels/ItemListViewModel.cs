using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Models;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.ViewModels.Data;
using liuguang.TlbbGmTool.Views.Item;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace liuguang.TlbbGmTool.ViewModels;

public class ItemListViewModel : ViewModelBase
{
    #region Fields
    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection? Connection;
    #endregion

    #region Properties

    public BagContainer ItemsContainer { get; } = new();

    public Visibility AddEquipVisible => ItemsContainer.RoleBagType == BagType.ItemBag ? Visibility.Visible : Visibility.Collapsed;
    public Visibility AddGemVisible => ItemsContainer.RoleBagType == BagType.MaterialBag ? Visibility.Visible : Visibility.Collapsed;
    private bool CanInsertItem => ItemsContainer.ItemList.Count < ItemsContainer.BagMaxSize;

    /// <summary>
    /// Hiện cửa sổ sửa vật phẩm
    /// </summary>
    public Command EditItemCommand { get; }
    /// <summary>
    /// Lệnh sao chép vật phẩm
    /// </summary>
    public Command CopyItemCommand { get; }
    /// <summary>
    /// Lệnh xóa vật phẩm
    /// </summary>
    public Command DeleteItemCommand { get; }
    /// <summary>
    /// Hiện cửa sổ phát trang bị
    /// </summary>
    public Command AddEquipCommand { get; }
    /// <summary>
    /// Hiện cửa sổ phát ngọc
    /// </summary>
    public Command AddGemCommand { get; }
    /// <summary>
    /// Hiện cửa sổ phát đạo cụ
    /// </summary>
    public Command AddItemCommand { get; }
    #endregion

    public ItemListViewModel()
    {
        EditItemCommand = new(ShowItemEditor);
        CopyItemCommand = new(ProcessCopyItem);
        DeleteItemCommand = new(ProcessDeleteItem);
        AddEquipCommand = new(ShowAddEquipEditor, () => CanInsertItem);
        AddGemCommand = new(ShowAddGemEditor, () => CanInsertItem);
        AddItemCommand = new(ShowAddItemEditor, () => CanInsertItem);
        ItemsContainer.PropertyChanged += ItemsContainer_PropertyChanged;
        ItemsContainer.ItemList.CollectionChanged += ItemList_CollectionChanged;
    }

    /// <summary>
    /// Khi độ dài danh sách vật phẩm thay đổi, cập nhật trạng thái nút phát (không cho phép phát nếu túi đầy)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ItemList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        AddEquipCommand.RaiseCanExecuteChanged();
        AddGemCommand.RaiseCanExecuteChanged();
        AddItemCommand.RaiseCanExecuteChanged();
    }

    private void ItemsContainer_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ItemsContainer.RoleBagType))
        {
            RaisePropertyChanged(nameof(AddEquipVisible));
            RaisePropertyChanged(nameof(AddGemVisible));
        }
    }

    public async Task LoadItemListAsync()
    {
        if (Connection is null)
        {
            return;
        }
        try
        {
            var itemList = await Task.Run(async () =>
            {
                return await ItemDbService.LoadItemListAsync(Connection, ItemsContainer.CharGuid, ItemsContainer.PosOffset, ItemsContainer.BagMaxSize);
            });
            ItemsContainer.FillItemList(itemList);
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lỗi khi tải dữ liệu", ex);
        }
    }

    private void ShowItemEditor(object? parameter)
    {
        if (parameter is not ItemLogViewModel itemLog)
        {
            return;
        }
        if (itemLog.ItemClass == 1)
        {
            ShowDialog(new EquipEditorWindow(), (EquipEditorViewModel vm) =>
            {
                vm.Connection = Connection;
                vm.ItemLog = itemLog;
            });
        }
        else if ((itemLog.ItemClass >= 2) && (itemLog.ItemClass <= 4))
        {
            // Bản đồ kho báu
            if (itemLog.ItemBaseId == 30000000)
            {
                ShowDialog(new StoreMapEditorWindow(), (CommonItemEditorViewModel vm) =>
                {
                    vm.RoleBagType = ItemsContainer.RoleBagType;
                    vm.Connection = Connection;
                    vm.ItemLog = itemLog;
                });
            }
            else
            {
                ShowDialog(new CommonItemEditorWindow(), (CommonItemEditorViewModel vm) =>
                {
                    vm.RoleBagType = ItemsContainer.RoleBagType;
                    vm.Connection = Connection;
                    vm.ItemLog = itemLog;
                });
            }
        }
        else if (itemLog.ItemClass == 5)
        {
            ShowDialog(new GemEditorWindow(), (GemEditorViewModel vm) =>
            {
                vm.Connection = Connection;
                vm.ItemLog = itemLog;
            });
        }
        else
        {
            ShowErrorMessage("Có lỗi xảy ra", $"Loại không xác định: class={itemLog.ItemClass}");
        }
    }
    private async void ProcessCopyItem(object? parameter)
    {
        if (Connection is null)
        {
            return;
        }
        if (parameter is not ItemLogViewModel itemLog)
        {
            return;
        }
        if (!Confirm("Xác nhận sao chép", $"Bạn có chắc chắn muốn sao chép {itemLog.ItemName} không?"))
        {
            return;
        }
        var pData = new byte[itemLog.PData.Length];
        Array.Copy(itemLog.PData, pData, pData.Length);
        var serverType = Connection.GameServerType;
        ItemLogViewModel newItemLog = new(new()
        {
            CharGuid = itemLog.CharGuid,
            ItemBaseId = itemLog.ItemBaseId,
            PData = pData,
            Creator = itemLog.Creator,
        }, serverType);
        try
        {
            await Task.Run(async () =>
            {
                await ItemDbService.InsertItemAsync(Connection, ItemsContainer.PosOffset, ItemsContainer.BagMaxSize, newItemLog);
            });
            ItemsContainer.InsertNewItem(newItemLog);
            ShowMessage("Sao chép thành công", $"Sao chép {newItemLog.ItemName} thành công, vị trí={newItemLog.Pos}");
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Sao chép thất bại", ex, true);
        }
    }

    private async void ProcessDeleteItem(object? parameter)
    {
        if (Connection is null)
        {
            return;
        }
        if (parameter is not ItemLogViewModel itemLog)
        {
            return;
        }
        if (!Confirm("Xác nhận xóa", $"Bạn có chắc chắn muốn xóa {itemLog.ItemName} không?"))
        {
            return;
        }
        try
        {
            await Task.Run(async () =>
            {
                await ItemDbService.DeleteItemAsync(Connection, itemLog.Id);
            });
            ItemsContainer.ItemList.Remove(itemLog);
            ShowMessage("Xóa thành công", $"Xóa {itemLog.ItemName} thành công");
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Xóa thất bại", ex, true);
        }

    }
    private void ShowAddEquipEditor()
    {
        ShowDialog(new EquipEditorWindow(), (EquipEditorViewModel vm) =>
        {
            vm.Connection = Connection;
            vm.ItemsContainer = ItemsContainer;
        });
    }
    private void ShowAddGemEditor()
    {
        ShowDialog(new GemEditorWindow(), (GemEditorViewModel vm) =>
        {
            vm.Connection = Connection;
            vm.ItemsContainer = ItemsContainer;
        });
    }
    private void ShowAddItemEditor()
    {
        ShowDialog(new CommonItemEditorWindow(), (CommonItemEditorViewModel vm) =>
        {
            vm.Connection = Connection;
            vm.RoleBagType = ItemsContainer.RoleBagType;
            vm.ItemsContainer = ItemsContainer;
        });
    }
}
