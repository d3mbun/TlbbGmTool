using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.ViewModels.Data;
using liuguang.TlbbGmTool.Views.Item;
using System;
using System.Collections.Generic;
using System.Linq;

namespace liuguang.TlbbGmTool.ViewModels;

/// <summary>
/// Trình chọn kỹ năng ám khí
/// </summary>
public class DarkImpactSelectorViewModel : ViewModelBase
{
    #region Fields
    /// <summary>
    /// Toàn bộ danh sách kỹ năng
    /// </summary>
    private List<ComboBoxNode<int>> _itemList = new();
    /// <summary>
    /// Danh sách kỹ năng phù hợp điều kiện lọc
    /// </summary>
    private List<ComboBoxNode<int>> _filterItemList = new();
    private int _initItemId;
    private string _searchText = string.Empty;
    private readonly PaginationViewModel _pagination = new();

    /// <summary>
    /// Số lượng hiển thị tối đa mỗi trang
    /// </summary>
    private const int _pageLimit = 20;
    #endregion
    #region Properties
    public List<ComboBoxNode<int>> ItemList
    {
        set
        {
            _itemList = value;
            DoFilterItemList();
        }
    }
    public PaginationViewModel Pagination => _pagination;
    public int InitItemId
    {
        set => _initItemId = value;
    }
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                DoFilterItemList();
            }
        }
    }

    public IEnumerable<ComboBoxNode<int>> CurrentPageItemList
    {
        get
        {
            var offset = (_pagination.Page - 1) * _pageLimit;
            return (from itemInfo in _filterItemList
                    select itemInfo).Skip(offset).Take(_pageLimit);
        }
    }
    #endregion
    #region Commands
    public Command ConfirmCommand { get; }

    #endregion

    public DarkImpactSelectorViewModel()
    {
        ConfirmCommand = new(ConfirmSelect, CanConfirmSelect);
        _pagination.OnPageChanged += () =>
        {
            RaisePropertyChanged(nameof(CurrentPageItemList));
        };
    }

    private void DoFilterItemList()
    {
        _filterItemList = (from itemInfo in _itemList
                           where itemInfo.Title.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                           select itemInfo).ToList();
        _pagination.SetCount(_filterItemList.Count, _pageLimit);
        RaisePropertyChanged(nameof(CurrentPageItemList));
    }
    private bool CanConfirmSelect(object? parameter)
    {
        if (parameter is ComboBoxNode<int> itemInfo)
        {
            return itemInfo.Value != _initItemId;

        }
        return false;
    }

    private void ConfirmSelect(object? parameter)
    {
        if (parameter is not ComboBoxNode<int> itemInfo)
        {
            return;
        }
        if (OwnedWindow is not DarkImpactSelectorWindow currentWindow)
        {
            return;
        }
        currentWindow.SelectedItem = itemInfo;
        currentWindow.DialogResult = true;
        currentWindow.Close();
    }
}
