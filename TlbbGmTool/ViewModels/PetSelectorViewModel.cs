using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Models;
using liuguang.TlbbGmTool.ViewModels.Data;
using liuguang.TlbbGmTool.Views.Pet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace liuguang.TlbbGmTool.ViewModels;

public class PetSelectorViewModel : ViewModelBase
{
    private List<PetAttrBase> _petList = new();
    private List<PetAttrBase> _filterPetList = new();

    private string _searchText = string.Empty;
    private int? _minLevel;
    private int? _maxLevel;
    private readonly PaginationViewModel _pagination = new();
    private const int _pageLimit = 20;

    public string WindowTitle => "Trình chọn Trân Thú";

    public List<PetAttrBase> PetList
    {
        set
        {
            _petList = value;
            DoFilterPetList();
        }
    }

    public PaginationViewModel Pagination => _pagination;

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                DoFilterPetList();
            }
        }
    }

    public string MinLevel
    {
        get => _minLevel?.ToString() ?? string.Empty;
        set
        {
            if (int.TryParse(value, out var v)) { _minLevel = v; }
            else { _minLevel = null; }
            DoFilterPetList();
        }
    }

    public string MaxLevel
    {
        get => _maxLevel?.ToString() ?? string.Empty;
        set
        {
            if (int.TryParse(value, out var v)) { _maxLevel = v; }
            else { _maxLevel = null; }
            DoFilterPetList();
        }
    }

    public IEnumerable<PetAttrBase> CurrentPagePetList
    {
        get
        {
            var offset = (_pagination.Page - 1) * _pageLimit;
            return (from petInfo in _filterPetList
                    select petInfo).Skip(offset).Take(_pageLimit);
        }
    }

    public Command ConfirmCommand { get; }

    public PetSelectorViewModel()
    {
        ConfirmCommand = new(ConfirmSelect);
        _pagination.OnPageChanged += () =>
        {
            RaisePropertyChanged(nameof(CurrentPagePetList));
        };
    }

    private void DoFilterPetList()
    {
        _filterPetList = (from petInfo in _petList
                          where (!_minLevel.HasValue) || petInfo.Level >= _minLevel.Value
                          where (!_maxLevel.HasValue) || petInfo.Level <= _maxLevel.Value
                          where petInfo.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                          select petInfo).ToList();
        _pagination.SetCount(_filterPetList.Count, _pageLimit);
        RaisePropertyChanged(nameof(CurrentPagePetList));
    }

    private void ConfirmSelect(object? parameter)
    {
        if (parameter is not PetAttrBase petInfo)
        {
            return;
        }
        if (OwnedWindow is not PetSelectorWindow currentWindow)
        {
            return;
        }
        currentWindow.SelectedPet = petInfo;
        currentWindow.DialogResult = true;
        currentWindow.Close();
    }
}
