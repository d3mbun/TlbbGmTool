using liuguang.TlbbGmTool.ViewModels.Data;

namespace liuguang.TlbbGmTool.Services;
public static class DarkDataService
{
    /// <summary>
    /// Đọc dữ liệu, bỏ vào darkData
    /// </summary>
    /// <param name="itemBaseId"></param>
    /// <param name="pData"></param>
    /// <param name="darkData"></param>
    public static void Read(byte[] pData, DarkDataViewModel darkData)
    {
        int offset = 0;
        var readNextInt = () =>
        {
            var value = DataService.ReadInt(pData, offset);
            offset += 4;
            return value;
        };
        var readNextShort = () =>
        {
            var value = DataService.ReadShort(pData, offset);
            offset += 2;
            return value;
        };
        darkData.NowExp = readNextInt();
        //3 trạng thái kỹ năng
        darkData.Impact0 = readNextShort();
        darkData.Impact1 = readNextShort();
        darkData.Impact2 = readNextShort();
        //5 loại thuộc tính cộng thêm
        darkData.AppendAttr0 = readNextShort();
        darkData.AppendAttr1 = readNextShort();
        darkData.AppendAttr2 = readNextShort();
        darkData.AppendAttr3 = readNextShort();
        darkData.AppendAttr4 = readNextShort();
        //
        darkData.UseTimes = readNextShort();
        darkData.Quality = readNextShort();
        darkData.Level = pData[offset];
    }
    /// <summary>
    /// Ghi dữ liệu vào pData
    /// </summary>
    /// <param name="darkData"></param>
    /// <param name="pData">Mảng byte 28 byte</param>
    public static void Write(DarkDataViewModel darkData, byte[] pData)
    {
        int offset = 0;
        var writeNextInt = (int value) =>
        {
            DataService.WriteData(pData, offset, value);
            offset += 4;
        };
        var writeNextShort = (short value) =>
        {
            DataService.WriteData(pData, offset, value);
            offset += 2;
        };
        //
        writeNextInt(darkData.NowExp);
        // 3 trạng thái kỹ năng
        writeNextShort(darkData.Impact0);
        writeNextShort(darkData.Impact1);
        writeNextShort(darkData.Impact2);
        // 5 loại thuộc tính cộng thêm
        writeNextShort(darkData.AppendAttr0);
        writeNextShort(darkData.AppendAttr1);
        writeNextShort(darkData.AppendAttr2);
        writeNextShort(darkData.AppendAttr3);
        writeNextShort(darkData.AppendAttr4);
        //
        writeNextShort(darkData.UseTimes);
        writeNextShort(darkData.Quality);
        pData[offset] = darkData.Level;
    }
}
