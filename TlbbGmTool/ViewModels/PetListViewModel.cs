using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.ViewModels.Data;
using liuguang.TlbbGmTool.Views.Pet;

namespace liuguang.TlbbGmTool.ViewModels;

public class PetListViewModel : ViewModelBase
{
    #region Fields
    public int CharGuid;
    /// <summary>
        // Chuyển đổi CSDL
    /// </summary>
    public DbConnection? Connection;

    #endregion

    #region Properties

    public ObservableCollection<PetLogViewModel> PetList { get; } = new();

    public Command EditPetCommand { get; }
    public Command EditPetSkillCommand { get; }
    public Command DeletePetCommand { get; }
    public Command AddPetCommand { get; }

    #endregion

    public PetListViewModel()
    {
        EditPetCommand = new(ShowPetEditor);
        EditPetSkillCommand = new(ShowPetSkillEditor);
        DeletePetCommand = new(AskDeletePet);
        AddPetCommand = new(ShowAddPet);
    }

    public async Task LoadPetListAsync()
    {
        if (Connection is null)
        {
            return;
        }
        try
        {
            var petList = await Task.Run(async () =>
            {
                return await DoLoadPetListAsync(Connection, CharGuid);
            });
            PetList.Clear();
            foreach (var petLog in petList)
            {
                PetList.Add(petLog);
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lỗi khi tải dữ liệu", ex);
        }
    }

    private async Task<List<PetLogViewModel>> DoLoadPetListAsync(DbConnection connection, int charGuid)
    {
        var xinFaList = new List<PetLogViewModel>();
        const string sql = "SELECT * FROM t_pet WHERE charguid=@charguid ORDER BY aid ASC";
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        mySqlCommand.Parameters.Add(new MySqlParameter("@charguid", MySqlDbType.Int32)
        {
            Value = charGuid
        });
        // Chuyển đổi CSDL
        await connection.SwitchGameDbAsync();
        using var reader = await mySqlCommand.ExecuteReaderAsync();
        if (reader is MySqlDataReader rd)
        {
            while (await rd.ReadAsync())
            {
                xinFaList.Add(new(new()
                {
                    Id = rd.GetInt32("aid"),
                    CharGuid = rd.GetInt32("charguid"),
                    PetName = DbStringService.ToCommonString(rd.GetString("petname")),
                    Level = rd.GetInt32("level"),
                    NeedLevel = rd.GetInt32("needlevel"),
                    AiType = rd.GetInt32("aitype"),
                    PetType = rd.GetInt32("pettype"),
                    Genera = rd.GetInt32("genera"),
                    Life = rd.GetInt32("life"),
                    Enjoy = rd.GetInt32("enjoy"),
                    Savvy = rd.GetInt32("savvy"),
                    Gengu = rd.GetInt32("gengu"),
                    GrowRate = rd.GetInt32("growrate"),
                    Repoint = rd.GetInt32("repoint"),
                    Exp = rd.GetInt32("exp"),
                    Str = rd.GetInt32("str"),
                    Spr = rd.GetInt32("spr"),
                    Con = rd.GetInt32("con"),
                    Ipr = rd.GetInt32("ipr"),
                    Dex = rd.GetInt32("dex"),
                    StrPer = rd.GetInt32("strper"),
                    SprPer = rd.GetInt32("sprper"),
                    ConPer = rd.GetInt32("conper"),
                    IprPer = rd.GetInt32("iprper"),
                    DexPer = rd.GetInt32("dexper"),
                    Skill = rd.GetString("skill"),
                }));
            }
        }
        return xinFaList;
    }

    private void ShowPetEditor(object? parameter)
    {
        if (parameter is PetLogViewModel petInfo)
        {
            ShowDialog(new PetEditorWindow(), (PetEditorViewModel vm) =>
            {
                vm.PetList = PetList;
                vm.PetInfo = petInfo;
                vm.Connection = Connection;
            });
        }
    }

    private void ShowAddPet(object? parameter)
    {
        var selectorWindow = new PetSelectorWindow();
        var beforeAction = (PetSelectorViewModel vm) =>
        {
            vm.PetList = SharedData.PetAttrMap.Values.ToList();
        };

        if (ShowDialog(selectorWindow, beforeAction) == true)
        {
            var selectedPet = selectorWindow.SelectedPet;
            if (selectedPet != null)
            {
                var newPetInfo = new PetLogViewModel(new()
                {
                    CharGuid = CharGuid,
                    PetName = selectedPet.Name,
                    Level = 1,
                    NeedLevel = selectedPet.Level,
                    AiType = 1,
                    PetType = 1,
                    Genera = selectedPet.Id,
                    Life = selectedPet.MaxLife > 0 ? selectedPet.MaxLife : 10000,
                    Enjoy = 100,
                    Savvy = 0,
                    Gengu = 0,
                    GrowRate = 1000,
                    Repoint = 0,
                    Exp = 0,
                    Str = 10,
                    Spr = 10,
                    Con = 10,
                    Ipr = 10,
                    Dex = 10,
                    StrPer = selectedPet.StrPer,
                    ConPer = selectedPet.ConPer,
                    SprPer = selectedPet.SprPer,
                    DexPer = selectedPet.DexPer,
                    IprPer = selectedPet.IprPer,
                    Skill = ""
                });

                ShowDialog(new PetEditorWindow(), (PetEditorViewModel vm) =>
                {
                    vm.PetList = PetList;
                    vm.PetInfo = newPetInfo;
                    vm.Connection = Connection;
                    vm.IsCreateMode = true;
                });
            }
        }
    }

    private void ShowPetSkillEditor(object? parameter)
    {
        if (parameter is PetLogViewModel petInfo)
        {
            ShowDialog(new PetSkillEditorWindow(), (PetSkillEditorViewModel vm) =>
            {
                vm.Connection = Connection;
                vm.PetInfo = petInfo;
            });
        }
    }

    private async void AskDeletePet(object? parameter)
    {
        if (parameter is not PetLogViewModel petInfo)
        {
            return;
        }
        if (Connection is null)
        {
            return;
        }
        if (!Confirm("Xác nhận xóa", $"Bạn có chắc chắn muốn xóa Trân Thú {petInfo.PetName} (ID: {petInfo.Id}) không?"))
        {
            return;
        }
        try
        {
            await Task.Run(async () =>
            {
                await DeletePetAsync(Connection, petInfo);
            });
            PetList.Remove(petInfo);
            ShowMessage("Xóa thành công", $"Xóa Trân Thú {petInfo.PetName} (ID: {petInfo.Id}) thành công");
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Xóa thất bại", ex);
        }
    }

    private async Task DeletePetAsync(DbConnection connection, PetLogViewModel petInfo)
    {
        const string sql = "DELETE FROM t_pet WHERE aid=@aid";
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        mySqlCommand.Parameters.Add(new MySqlParameter("@aid", MySqlDbType.Int32)
        {
            Value = petInfo.Id,
        });
        // 切换数据库
        await connection.SwitchGameDbAsync();
        //
        await mySqlCommand.ExecuteNonQueryAsync();
    }
}
