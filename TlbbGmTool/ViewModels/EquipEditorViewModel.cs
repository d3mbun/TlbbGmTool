using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Models;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.ViewModels.Data;
using liuguang.TlbbGmTool.Views.Item;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace liuguang.TlbbGmTool.ViewModels;
public class EquipEditorViewModel : ViewModelBase
{
    #region Fields
    private bool _isSaving = false;
    private ItemLogViewModel? _inputItemLog;
    private readonly EquipDataViewModel _equipData = new();
    private BagContainer? _itemsContainer;
    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection? Connection;
    #endregion
    #region Properties
    public bool IsSaving
    {
        get => _isSaving;
        set
        {
            if (SetProperty(ref _isSaving, value))
            {
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }
    public string WindowTitle
    {
        get
        {
                return "Phát Trang Bị";
            return "Chỉnh sửa Trang Bị " + _equipData.EquipName;
        }
    }
    public ItemLogViewModel ItemLog
    {
        set
        {
            _inputItemLog = value;
            EquipDataService.Read(value.ItemBaseId, value.PData, _equipData, Connection?.GameServerType ?? ServerType.Common);
            _equipData.Creator = value.Creator;
        }
    }
    public BagContainer ItemsContainer
    {
        set
        {
            _itemsContainer = value;
            var defaultItem = (from itemBaseInfo in SharedData.ItemBaseMap.Values
                               where itemBaseInfo.TClass == 1
                               select new ItemBaseViewModel(itemBaseInfo)).FirstOrDefault();
            if (defaultItem != null)
            {
                LoadNewItemBase(defaultItem);
            }
        }
    }
    public EquipDataViewModel EquipData => _equipData;
    /// <summary>
    /// Chọn sao
    /// </summary>
    public List<ComboBoxNode<byte>> StarSection { get; } = new();
    /// <summary>
    /// Chọn số lỗ
    /// </summary>
    public List<ComboBoxNode<byte>> GemMaxCountSection { get; } = new();
    #endregion

    #region Commands
    public Command SelectEquipCommand { get; }
    public Command SelectVisualCommand { get; }
    public Command SelectGem0Command { get; }
    public Command SelectGem1Command { get; }
    public Command SelectGem2Command { get; }
    public Command SelectGem3Command { get; }
    public Command SelectAttrCommand { get; }
    public Command SaveCommand { get; }
    #endregion

    public EquipEditorViewModel()
    {
        SelectEquipCommand = new(ShowSelectEquipWindow);
        SelectVisualCommand = new(ShowSelectVisualWindow);
        SelectGem0Command = new(() => ShowSelectGemWindow(0), () => _equipData.GemMaxCount > 0);
        SelectGem1Command = new(() => ShowSelectGemWindow(1), () => _equipData.GemMaxCount > 1);
        SelectGem2Command = new(() => ShowSelectGemWindow(2), () => _equipData.GemMaxCount > 2);
        SelectGem3Command = new(() => ShowSelectGemWindow(3), () => _equipData.GemMaxCount > 3);
        SelectAttrCommand = new(ShowSelectAttrWindow, () => _equipData.HasSegAttr);
        SaveCommand = new(SaveItem, () => !_isSaving);
        for (byte i = 0; i <= 9; i++)
        {
            StarSection.Add(new($"{i} Sao", i));
        }
        for (byte i = 0; i <= 4; i++)
        {
            GemMaxCountSection.Add(new($"{i} Lỗ", i));
        }
        _equipData.PropertyChanged += EquipData_PropertyChanged;
    }

    /// <summary>
    /// Khởi tạo dữ liệu tương ứng khi chọn ID trang bị mới
    /// </summary>
    /// <param name="itemBase"></param>
    private void LoadNewItemBase(ItemBaseViewModel itemBase)
    {
        _equipData.ItemBaseId = itemBase.ItemBaseId;
        // Trang bị không thuộc tính
        if (!_equipData.HasSegAttr)
        {
            _equipData.Attr0 = 0;
            _equipData.Attr1 = 0;
        }
        if (itemBase.BaseInfo is ItemBaseEquip equipBaseInfo)
        {
            _equipData.RulerId = equipBaseInfo.RulerId;
            _equipData.CurDurPoint = equipBaseInfo.MaxDurPoint;
            _equipData.CurDamagePoint = 0;
            _equipData.MaxDurPoint = equipBaseInfo.MaxDurPoint;
            _equipData.VisualId = equipBaseInfo.EquipVisual;
            _equipData.DarkFlag = 0;
            // Ám khí
            if (equipBaseInfo.EquipPoint == 17)
            {
                _equipData.DarkFlag = 1;
                _equipData.Attr0 = 0;
                // 5 thuộc tính cơ bản cố định
                _equipData.Attr1 = 0x7C00;
                // Ám khí không có người chế tạo
                _equipData.HasCreator = false;
                _equipData.Creator = string.Empty;
            }
        }
    }

    private void EquipData_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Khi số lỗ thay đổi, thông báo cập nhật trạng thái nút chọn ngọc
        if (e.PropertyName == nameof(_equipData.GemMaxCount))
        {
            SelectGem0Command.RaiseCanExecuteChanged();
            SelectGem1Command.RaiseCanExecuteChanged();
            SelectGem2Command.RaiseCanExecuteChanged();
            SelectGem3Command.RaiseCanExecuteChanged();
        }
        else if (e.PropertyName == nameof(_equipData.EquipName))
        {
            RaisePropertyChanged(nameof(WindowTitle));
        }
        else if (e.PropertyName == nameof(_equipData.HasSegAttr))
        {
            SelectAttrCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Hiện cửa sổ sửa ID trang bị
    /// </summary>
    private void ShowSelectEquipWindow()
    {
        var selectorWindow = new ItemSelectorWindow();
        var beforeAction = (ItemSelectorViewModel vm) =>
        {
            vm.WindowTitle = "Chọn Trang Bị";
            vm.InitItemId = _equipData.ItemBaseId;
            vm.ItemList = (from itemBaseInfo in SharedData.ItemBaseMap.Values
                           where itemBaseInfo.TClass == 1
                           select new ItemBaseViewModel(itemBaseInfo)).ToList();
        };
        if (ShowDialog(selectorWindow, beforeAction) == true)
        {
            var selectedItem = selectorWindow.SelectedItem;
            if (selectedItem != null)
            {
                LoadNewItemBase(selectedItem);
            }
        }
    }

    /// <summary>
    /// Hiện cửa sổ sửa ngoại hình
    /// </summary>
    private void ShowSelectVisualWindow()
    {
        var selectorWindow = new ItemSelectorWindow();
        var visualItemId = _equipData.ParseVisualItemId(_equipData.VisualId);
        var beforeAction = (ItemSelectorViewModel vm) =>
        {
            vm.WindowTitle = "Chọn Ngoại Hình";
            vm.InitItemId = visualItemId ?? 0;
            vm.ItemList = (from itemBaseInfo in SharedData.ItemBaseMap.Values
                           where itemBaseInfo.TClass == 1
                           let equipBaseInfo = (ItemBaseEquip)itemBaseInfo
                           // Khi thay đổi ngoại hình, giới hạn vị trí trang bị phải giống nhau
                           where equipBaseInfo.EquipPoint == _equipData.EquipPoint
                           select new ItemBaseViewModel(itemBaseInfo)).ToList();
        };
        if (ShowDialog(selectorWindow, beforeAction) == true)
        {
            var selectedItem = selectorWindow.SelectedItem;
            if (selectedItem != null)
            {

                if (selectedItem.BaseInfo is ItemBaseEquip equipBaseInfo)
                {
                    _equipData.VisualId = equipBaseInfo.EquipVisual;
                }
            }
        }
    }

    /// <summary>
    /// Hiện cửa sổ sửa loại thuộc tính
    /// </summary>
    private void ShowSelectAttrWindow()
    {
        if (_equipData.DarkFlag != 0)
        {
            ShowDarkEquipEditor();
            return;
        }
        var selectorWindow = new AttrSelectorWindow();
        var beforeAction = (AttrSelectorViewModel vm) =>
        {
            if (_equipData.SegAttrs != null)
            {
                vm.EquipValueAttrs = _equipData.SegAttrs;
                vm.Attr0 = _equipData.Attr0;
                vm.Attr1 = _equipData.Attr1;
            }
        };
        if (ShowDialog(selectorWindow, beforeAction) == true)
        {
            _equipData.Attr0 = selectorWindow.Attr0;
            _equipData.Attr1 = selectorWindow.Attr1;
        }
    }
    /// <summary>
    /// Hiện cửa sổ sửa ám khí
    /// </summary>
    private void ShowDarkEquipEditor()
    {
        var editorWindow = new DarkDataEditorWindow();
        var beforeAction = (DarkDataEditorViewModel vm) =>
        {
            vm.InitHexData = _equipData.Creator;
        };
        if (ShowDialog(editorWindow, beforeAction) == true)
        {
            _equipData.Creator = editorWindow.HexData;
        }
    }

    /// <summary>
    /// Hiện cửa sổ chọn ngọc
    /// </summary>
    private void ShowSelectGemWindow(int gemIndex)
    {
        int gemId;
        switch (gemIndex)
        {
            case 0:
                gemId = _equipData.Gem0; break;
            case 1:
                gemId = _equipData.Gem1; break;
            case 2:
                gemId = _equipData.Gem2; break;
            case 3:
                gemId = _equipData.Gem3; break;
            default:
                ShowErrorMessage("Có lỗi xảy ra", $"Vị trí Bảo Thạch không hợp lệ: {gemIndex}");
                return;
        }
        var selectorWindow = new LvItemSelectorWindow();
        var beforeAction = (LvItemSelectorViewModel vm) =>
        {
            vm.WindowTitle = "Chọn Bảo Thạch";
            vm.InitItemId = gemId;
            vm.ItemList = (from itemBaseInfo in SharedData.ItemBaseMap.Values
                           where itemBaseInfo.TClass == 5
                           select new ItemBaseViewModel(itemBaseInfo)).ToList();
            vm.LoadLastData();
        };
        if (ShowDialog(selectorWindow, beforeAction) == true)
        {
            var selectedItem = selectorWindow.SelectedItem;
            if (selectedItem != null)
            {
                var selectedGemId = selectedItem.ItemBaseId;
                switch (gemIndex)
                {
                    case 0:
                        _equipData.Gem0 = selectedGemId; break;
                    case 1:
                        _equipData.Gem1 = selectedGemId; break;
                    case 2:
                        _equipData.Gem2 = selectedGemId; break;
                    case 3:
                        _equipData.Gem3 = selectedGemId; break;
                }
            }
        }
    }

    private async void SaveItem()
    {
        if (Connection is null)
        {
            return;
        }
        IsSaving = true;
        var itemBaseId = _equipData.ItemBaseId;
        byte[] pData = new byte[17 * 4];
        if (_inputItemLog is null)
        {
            if (_equipData.DarkFlag == 0)
            {
                _equipData.HasCreator = true;
                _equipData.Creator = "Lưu Quang";

            }
        }
        EquipDataService.Write(_equipData, pData, Connection.GameServerType);
        if (_inputItemLog is null)
        {
            await InsertItemAsync(Connection, itemBaseId, pData, _equipData.Creator);
        }
        else
        {
            await UpdateItemAsync(Connection, _inputItemLog, itemBaseId, pData, _equipData.Creator);
        }
    }

    private async Task InsertItemAsync(DbConnection connection, int itemBaseId, byte[] pData, string creator)
    {
        if (_itemsContainer is null)
        {
            return;
        }
        var serverType = connection.GameServerType;
        ItemLogViewModel itemLog = new(new()
        {
            CharGuid = _itemsContainer.CharGuid,
            ItemBaseId = itemBaseId,
            PData = pData,
            Creator = creator
        }, serverType);
        try
        {
            await Task.Run(async () =>
            {
                await ItemDbService.InsertItemAsync(connection, _itemsContainer.PosOffset, _itemsContainer.BagMaxSize, itemLog);
            });
            _itemsContainer.InsertNewItem(itemLog);
            ShowMessage("Phát thành công", $"Phát Trang Bị thành công, vị trí={itemLog.Pos}");
            OwnedWindow?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Phát thất bại", ex, true);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task UpdateItemAsync(DbConnection connection, ItemLogViewModel itemLog, int itemBaseId, byte[] pData, string creator)
    {
        try
        {
            await Task.Run(async () =>
            {
                await ItemDbService.UpdateItemAsync(connection, itemLog.Id, itemBaseId, pData, creator);
            });
            itemLog.ItemBaseId = itemBaseId;
            itemLog.PData = pData;
            itemLog.Creator = creator;
            ShowMessage("Chỉnh sửa thành công", "Chỉnh sửa Trang Bị thành công");
            OwnedWindow?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Chỉnh sửa thất bại", ex, true);
        }
        finally
        {
            IsSaving = false;
        }
    }
}
