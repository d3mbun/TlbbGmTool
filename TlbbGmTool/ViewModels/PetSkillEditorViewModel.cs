using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.ViewModels.Data;

namespace liuguang.TlbbGmTool.ViewModels;
public class PetSkillEditorViewModel : ViewModelBase
{
    #region Fields
    private bool _isSaving = false;
    private PetLogViewModel? _inputPetInfo;
    private PetLogViewModel _petInfo = new(new());
    private List<ComboBoxNode<int>> _skillTypeSelection = new() {
        new("Tất cả",0),
        new("Chủ động",1),
        new("Bị động",2),
        new("buff",3)
    };
    private string _searchText = string.Empty;
    private int _searchSkillType = 0;
    private PetSkillViewModel? _selectedSkill;
    private SortedDictionary<int, PetSkillViewModel> _allSkills;
    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection? Connection;
    #endregion
    #region Properties
    public string WindowTitle => $"Chỉnh sửa danh sách kỹ năng {_petInfo.PetName} (ID: {_petInfo.Id})";
    public PetLogViewModel PetInfo
    {
        set
        {
            _inputPetInfo = value;
            _petInfo.CopyFrom(value);
            RaisePropertyChanged(nameof(WindowTitle));
            LoadSkillList(value.Skill);
            NotifyReloadSkillSelection();
        }
    }
    public List<ComboBoxNode<int>> SkillTypeSelection => _skillTypeSelection;

    public int SearchSkillType
    {
        get => _searchSkillType;
        set
        {
            if (SetProperty(ref _searchSkillType, value))
            {
                NotifyReloadSkillSelection();
            }
        }
    }
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                NotifyReloadSkillSelection();
            }
        }
    }

    public PetSkillViewModel? SelectedSkill
    {
        get => _selectedSkill;
        set => SetProperty(ref _selectedSkill, value);
    }

    public List<PetSkillViewModel> SkillSelection
    {
        get
        {
            return (from skillItem in _allSkills.Values
                        // Lọc theo loại
                    where _searchSkillType == 0 || skillItem.SkillType == (_searchSkillType - 1)
                    // Lọc theo từ khóa
                    where skillItem.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0
                    // Loại trừ những cái đã tồn tại
                    where !SkillList.Contains(skillItem)
                    select skillItem).ToList();
        }
    }

    public ObservableCollection<PetSkillViewModel> SkillList { get; } = new();

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

    public Command SaveCommand { get; }
    public Command AddPetSkillCommand { get; }
    public Command DeletePetSkillCommand { get; }
    #endregion

    public PetSkillEditorViewModel()
    {
        SaveCommand = new(SavePetSkill, () => !_isSaving);
        AddPetSkillCommand = new(AddSkillToList, CanAddSkill);
        DeletePetSkillCommand = new(DeletePetSkill);
        _allSkills = new();
        foreach (var keyPair in SharedData.PetSkillMap)
        {
            _allSkills[keyPair.Key] = new(keyPair.Value);
        }
    }

    /// <summary>
    /// Tải lại danh sách
    /// </summary>
    private void NotifyReloadSkillSelection()
    {
        RaisePropertyChanged(nameof(SkillSelection));
        // Mặc định chọn cái đầu tiên
        SelectedSkill = SkillSelection.First();
        AddPetSkillCommand.RaiseCanExecuteChanged();
    }

    private void LoadSkillList(string skillHex)
    {
        if (Connection is null)
        {
            return;
        }
        var serverType = Connection.GameServerType;
        var pData = DataService.ConvertToPData(skillHex);
        for (var i = 0; i < 13; i++)
        {
            var offset = i;
            byte flag;
            if (serverType == ServerType.Common)
            {
                offset *= 3;
                flag = pData[offset];
                offset++;
            }
            else
            {
                offset *= 5;
                flag = pData[offset + 4];
            }
            if (flag == 0)
            {
                continue;
            }
            int skillId;
            if (serverType == ServerType.Common)
            {
                skillId = DataService.ReadShort(pData, offset);
            }
            else
            {
                skillId = DataService.ReadInt(pData, offset);
            }
            if (_allSkills.TryGetValue(skillId, out var skillItem))
            {
                SkillList.Add(skillItem);
            }
        }
    }

    private async void SavePetSkill()
    {
        if (Connection is null)
        {
            return;
        }
        IsSaving = true;
        try
        {
            await Task.Run(async () =>
            {
                await DoSavePetSkillAsync(Connection, _petInfo);
            });
            _inputPetInfo?.CopyFrom(_petInfo);
            ShowMessage("Lưu thành công", "Lưu kỹ năng Trân Thú thành công");
            OwnedWindow?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lưu kỹ năng Trân Thú thất bại", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task DoSavePetSkillAsync(DbConnection connection, PetLogViewModel petInfo)
    {
        int nodeLength;
        var serverType = connection.GameServerType;
        if (serverType == ServerType.Common)
        {
            nodeLength = 3;
        }
        else
        {
            nodeLength = 5;
        }

        var pData = new byte[13 * nodeLength];
        var offset = 0;
        // Ghi mã kỹ năng (ID)
        foreach (var skillInfo in SkillList)
        {
            if (serverType == ServerType.Common)
            {
                pData[offset] = 1;
                offset++;
                DataService.WriteData(pData, offset, (short)skillInfo.Id);
                offset += 2;
            }
            else
            {
                DataService.WriteData(pData, offset, skillInfo.Id);
                offset += 4;
                pData[offset] = 1;
                offset++;
            }
        }
        // Điền dữ liệu còn lại
        while (offset < pData.Length)
        {

            if (serverType == ServerType.Common)
            {
                short padValue = -1;
                pData[offset] = 0;
                offset++;
                DataService.WriteData(pData, offset, padValue);
                offset += 2;
            }
            else
            {
                int padValue = -1;
                DataService.WriteData(pData, offset, padValue);
                offset += 4;
                pData[offset] = 0;
                offset++;
            }
        }
        petInfo.Skill = DataService.ConvertToHex(pData);
        //
        const string sql = "UPDATE t_pet SET skill=@skill WHERE aid=@aid";
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        mySqlCommand.Parameters.Add(new MySqlParameter("@skill", MySqlDbType.String)
        {
            Value = petInfo.Skill
        });
        mySqlCommand.Parameters.Add(new MySqlParameter("@aid", MySqlDbType.Int32)
        {
            Value = petInfo.Id
        });
        // Chuyển đổi CSDL
        await connection.SwitchGameDbAsync();
        //exec
        await mySqlCommand.ExecuteNonQueryAsync();
    }

    private bool CanAddSkill()
    {
        if (_isSaving)
        {
            return false;
        }
        if (_selectedSkill == null)
        {
            return false;
        }

        const int maxSkillCount = 12;
        return SkillList.Count < maxSkillCount;
    }

    private void AddSkillToList()
    {
        if (_selectedSkill is null)
        {
            return;
        }

        // Kiểm tra xem có tồn tại không
        foreach (var skillInfo in SkillList)
        {
            if (skillInfo.Id == _selectedSkill.Id)
            {
                return;
            }
        }

        const int maxSkillCount = 12;
        if (SkillList.Count >= maxSkillCount)
        {
            return;
        }

        SkillList.Add(_selectedSkill);
        NotifyReloadSkillSelection();
    }

    private void DeletePetSkill(object? parameter)
    {
        if (parameter is PetSkillViewModel targetSkill)
        {
            SkillList.Remove(targetSkill);
            NotifyReloadSkillSelection();
        }
    }
}
