using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.Services;
using liuguang.TlbbGmTool.ViewModels.Data;

namespace liuguang.TlbbGmTool.ViewModels;
public class PetEditorViewModel : ViewModelBase
{
    #region Fields
    private bool _isSaving = false;
    private PetLogViewModel? _inputPetInfo;
    private PetLogViewModel _petInfo = new(new());
    private List<ComboBoxNode<int>> _aiTypeSelection = new() {
        new("Nhát gan",0),
        new("Cẩn trọng",1),
        new("Trung thành",2),
        new("Nhanh nhẹn",3),
        new("Dũng cảm",4)
    };
    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection? Connection;
    public ObservableCollection<PetLogViewModel>? PetList { get; set; }
    public bool IsCreateMode { get; set; } = false;
    #endregion
    #region Properties
    public string WindowTitle => IsCreateMode ? $"Tạo mới {_petInfo.PetName}" : $"Chỉnh sửa {_petInfo.PetName} (ID: {_petInfo.Id})";
    public PetLogViewModel PetInfo
    {
        get => _petInfo;
        set
        {
            _inputPetInfo = value;
            _petInfo.CopyFrom(value);
            value.PropertyChanged += PetInfo_PropertyChanged;
            RaisePropertyChanged(nameof(WindowTitle));
        }
    }
    public List<ComboBoxNode<int>> AiTypeSelection => _aiTypeSelection;

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
    #endregion

    public PetEditorViewModel()
    {
        SaveCommand = new(SavePet, () => !_isSaving);
    }

    private void PetInfo_PropertyChanged(object? sender, PropertyChangedEventArgs evt)
    {
        PetLogViewModel? value;
        if (evt.PropertyName == nameof(value.PetName))
        {
            RaisePropertyChanged(nameof(WindowTitle));
        }
    }

    private async void SavePet()
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
                if (IsCreateMode)
                {
                    await DoInsertPetAsync(Connection, _petInfo);
                }
                else
                {
                    await DoSavePetAsync(Connection, _petInfo);
                }
            });
            _inputPetInfo?.CopyFrom(_petInfo);
            if (IsCreateMode && PetList != null && _inputPetInfo != null)
            {
                PetList.Add(_inputPetInfo);
                IsCreateMode = false;
                RaisePropertyChanged(nameof(WindowTitle));
            }
            ShowMessage("Lưu thành công", "Lưu thông tin Trân Thú thành công");
            OwnedWindow?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lưu thông tin Trân Thú thất bại", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task DoSavePetAsync(DbConnection connection, PetLogViewModel petInfo)
    {
        var sql = "UPDATE t_pet SET";
        // Các trường kiểu int
        var intDictionary = new Dictionary<string, int>()
        {
            ["level"] = petInfo.Level,
            ["needlevel"] = petInfo.NeedLevel,
            ["aitype"] = petInfo.AiType,
            ["pettype"] = petInfo.PetType,
            ["genera"] = petInfo.Genera,
            ["life"] = petInfo.Life,
            ["enjoy"] = petInfo.Enjoy,
            ["savvy"] = petInfo.Savvy,
            ["gengu"] = petInfo.Gengu,
            ["growrate"] = petInfo.GrowRate,
            ["repoint"] = petInfo.Repoint,
            ["exp"] = petInfo.Exp,
            ["str"] = petInfo.Str,
            ["spr"] = petInfo.Spr,
            ["con"] = petInfo.Con,
            ["ipr"] = petInfo.Ipr,
            ["dex"] = petInfo.Dex,
            ["strper"] = petInfo.StrPer,
            ["sprper"] = petInfo.SprPer,
            ["conper"] = petInfo.ConPer,
            ["iprper"] = petInfo.IprPer,
            ["dexper"] = petInfo.DexPer,
        };
        var fieldNames = intDictionary.Keys.ToList();
        fieldNames.Add("petname");
        // fieldA=@fieldA
        var updateCondition = (from fieldName in fieldNames
                               select $"{fieldName}=@{fieldName}");
        sql += " " + string.Join(", ", updateCondition) + " WHERE aid=@aid";
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        foreach (var keyPair in intDictionary)
        {
            mySqlCommand.Parameters.Add(new MySqlParameter("@" + keyPair.Key, MySqlDbType.Int32)
            {
                Value = keyPair.Value
            });

        }
        mySqlCommand.Parameters.Add(new MySqlParameter("@petname", MySqlDbType.String)
        {
            Value = DbStringService.ToDbString(petInfo.PetName)
        });
        mySqlCommand.Parameters.Add(new MySqlParameter("@aid", MySqlDbType.Int32)
        {
            Value = petInfo.Id
        });
        await connection.SwitchGameDbAsync();
        //exec
        await mySqlCommand.ExecuteNonQueryAsync();
    }

    private async Task DoInsertPetAsync(DbConnection connection, PetLogViewModel petInfo)
    {
        // Chuyển đổi CSDL
        await connection.SwitchGameDbAsync();

        // Lấy lpetguid lớn nhất hiện tại
        int maxLPetGuid = 0;
        var cmdMax = new MySqlCommand("SELECT MAX(lpetguid) FROM t_pet", connection.Conn);
        var resultMax = await cmdMax.ExecuteScalarAsync();
        if (resultMax != null && resultMax != DBNull.Value)
        {
            maxLPetGuid = Convert.ToInt32(resultMax);
        }
        int nextLPetGuid = maxLPetGuid + 1;

        var sql = "INSERT INTO t_pet (charguid, hpetguid, lpetguid, dataxid, petname, level, needlevel, aitype, atttype, pettype, genera, life, hp, enjoy, savvy, gengu, growrate, repoint, exp, str, spr, con, ipr, dex, strper, sprper, conper, iprper, dexper, pclvl, skill) VALUES (@charguid, @hpetguid, @lpetguid, @dataxid, @petname, @level, @needlevel, @aitype, @atttype, @pettype, @genera, @life, @hp, @enjoy, @savvy, @gengu, @growrate, @repoint, @exp, @str, @spr, @con, @ipr, @dex, @strper, @sprper, @conper, @iprper, @dexper, @pclvl, @skill); SELECT LAST_INSERT_ID();";
        
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        var intDictionary = new Dictionary<string, int>()
        {
            ["charguid"] = petInfo.CharGuid,
            ["hpetguid"] = petInfo.CharGuid,
            ["lpetguid"] = nextLPetGuid,
            ["dataxid"] = petInfo.Genera,
            ["level"] = petInfo.Level,
            ["needlevel"] = petInfo.NeedLevel,
            ["aitype"] = petInfo.AiType,
            ["atttype"] = -1,
            ["pettype"] = petInfo.PetType,
            ["genera"] = petInfo.Genera,
            ["life"] = petInfo.Life,
            ["hp"] = 100,
            ["enjoy"] = petInfo.Enjoy,
            ["savvy"] = petInfo.Savvy,
            ["gengu"] = petInfo.Gengu,
            ["growrate"] = petInfo.GrowRate,
            ["repoint"] = petInfo.Repoint,
            ["exp"] = petInfo.Exp,
            ["str"] = petInfo.Str,
            ["spr"] = petInfo.Spr,
            ["con"] = petInfo.Con,
            ["ipr"] = petInfo.Ipr,
            ["dex"] = petInfo.Dex,
            ["strper"] = petInfo.StrPer,
            ["sprper"] = petInfo.SprPer,
            ["conper"] = petInfo.ConPer,
            ["iprper"] = petInfo.IprPer,
            ["dexper"] = petInfo.DexPer,
            ["pclvl"] = -1,
        };

        foreach (var keyPair in intDictionary)
        {
            mySqlCommand.Parameters.Add(new MySqlParameter("@" + keyPair.Key, MySqlDbType.Int32)
            {
                Value = keyPair.Value
            });
        }
        mySqlCommand.Parameters.Add(new MySqlParameter("@petname", MySqlDbType.String)
        {
            Value = DbStringService.ToDbString(petInfo.PetName)
        });
        mySqlCommand.Parameters.Add(new MySqlParameter("@skill", MySqlDbType.String)
        {
            Value = petInfo.Skill
        });

        // exec and get inserted id
        var newId = Convert.ToInt32(await mySqlCommand.ExecuteScalarAsync());
        petInfo.Id = newId;
    }
}
