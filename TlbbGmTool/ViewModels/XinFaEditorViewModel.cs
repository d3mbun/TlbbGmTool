using System;
using System.Threading.Tasks;
using liuguang.TlbbGmTool.Common;

namespace liuguang.TlbbGmTool.ViewModels;

public class XinFaEditorViewModel : ViewModelBase
{
    #region Fields
    private bool _isSaving = false;
    private XinFaLogViewModel? _inputXinFaLog;
    private XinFaLogViewModel _xinFaLog = new(new());
    /// <summary>
    /// Kết nối CSDL
    /// </summary>
    public DbConnection? Connection;
    #endregion
    #region Properties
    public XinFaLogViewModel XinFaLog
    {
        get => _xinFaLog;
        set
        {
            _inputXinFaLog = value;
            _xinFaLog.CopyFrom(value);
        }
    }
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

    public XinFaEditorViewModel()
    {
        SaveCommand = new(SaveXinFa);
    }
    private async void SaveXinFa()
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
                await DoSaveXinFaAsync(Connection, _xinFaLog);
            });
            _inputXinFaLog?.CopyFrom(_xinFaLog);
            ShowMessage("Lưu thành công", "Lưu cấp độ Tâm Pháp thành công");
            OwnedWindow?.Close();
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Lưu thất bại", ex);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task DoSaveXinFaAsync(DbConnection connection, XinFaLogViewModel xinFaLog)
    {
        const string sql = "UPDATE t_xinfa SET xinfalvl=@level WHERE aid=@aid";
        var mySqlCommand = new MySqlCommand(sql, connection.Conn);
        mySqlCommand.Parameters.Add(new MySqlParameter("@level", MySqlDbType.Int32)
        {
            Value = xinFaLog.XinFaLevel
        });
        mySqlCommand.Parameters.Add(new MySqlParameter("@aid", MySqlDbType.Int32)
        {
            Value = xinFaLog.Id
        });
        // Chuyển đổi CSDL
        await connection.SwitchGameDbAsync();
        //
        await mySqlCommand.ExecuteNonQueryAsync();
    }
}
