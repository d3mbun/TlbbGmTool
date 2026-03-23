using liuguang.TlbbGmTool.Common;
using liuguang.TlbbGmTool.ViewModels.Data;

namespace liuguang.TlbbGmTool.Services;
public static class GemDataService
{
    /// <summary>
    /// Đọc dữ liệu, bỏ vào gemData
    /// </summary>
    /// <param name="itemBaseId"></param>
    /// <param name="pData"></param>
    /// <param name="gemData"></param>
    /// <param name="serverType">Loại server</param>
    public static void Read(int itemBaseId, byte[] pData, GemDataViewModel gemData, ServerType serverType)
    {
        gemData.ItemBaseId = itemBaseId;
        int offset = 0;
        var readNextByte = () =>
        {
            var value = pData[offset];
            offset++;
            return value;
        };
        var readNextUInt = () =>
        {
            var value = DataService.ReadUInt(pData, offset);
            offset += 4;
            return value;
        };
        var readNextUshort = () =>
        {
            var value = DataService.ReadUshort(pData, offset);
            offset += 2;
            return value;
        };
        gemData.RulerId = readNextByte();
        if (serverType == ServerType.Common)
        {
            //Bỏ qua byte có giá trị cố định là 0
            offset++;
        }
        gemData.BasePrice = readNextUInt();
        gemData.AttrType = readNextByte();
        if (serverType == ServerType.HuaiJiu)
        {
            //Bỏ qua byte có giá trị cố định là 0
            offset++;
        }
        gemData.AttrValue = readNextUshort();
        if (serverType == ServerType.HuaiJiu)
        {
            //Nhảy qua 2 byte
            offset += 2;
            gemData.Count = readNextByte();
        }
        else
        {
            gemData.Count = 1;
        }
    }
    /// <summary>
    /// Ghi dữ liệu vào pData
    /// </summary>
    /// <param name="gemData"></param>
    /// <param name="pData">Mảng byte có độ dài 17*4</param>
    /// <param name="serverType">Loại máy chủ</param>
    /// <param name="serverType">端类型</param>
    public static void Write(GemDataViewModel gemData, byte[] pData, ServerType serverType)
    {
        int offset = 0;
        var writeNextByte = (byte value) =>
        {
            pData[offset] = value;
            offset++;
        };
        var writeNextUInt = (uint value) =>
        {
            DataService.WriteData(pData, offset, value);
            offset += 4;
        };
        var writeNextUshort = (ushort value) =>
        {
            DataService.WriteData(pData, offset, value);
            offset += 2;
        };
        //
        writeNextByte(gemData.RulerId);
        if (serverType == ServerType.Common)
        {
            //Bỏ qua byte có giá trị cố định là 0
            offset++;
        }
        writeNextUInt(gemData.BasePrice);
        writeNextByte(gemData.AttrType);
        if (serverType == ServerType.HuaiJiu)
        {
            //Bỏ qua byte có giá trị cố định là 0
            offset++;
        }
        writeNextUshort(gemData.AttrValue);
        if (serverType == ServerType.HuaiJiu)
        {
            ushort extraData = 0x01FA;
            writeNextUshort(extraData);
            writeNextByte(gemData.Count);
        }
    }
}
