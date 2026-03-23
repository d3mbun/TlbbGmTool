using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace liuguang.TlbbGmTool.Services;

/// <summary>
/// Công cụ tải cấu hình common.xml
/// </summary>
public static class CommonConfigService
{
    private static string GetConfigFilePath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "config", "common.xml");
    }

    /// <summary>
    /// Tải cấu hình tên môn phái và tên thuộc tính công, thủ
    /// </summary>
    /// <param name="menpaiMap"></param>
    /// <param name="attr1Map"></param>
    /// <param name="attr2Map"></param>
    /// <returns></returns>
    public static async Task LoadConfigAsync(SortedDictionary<int, string> menpaiMap, SortedDictionary<int, string> attr1Map, SortedDictionary<int, string> attr2Map)
    {
        var configFilePath = GetConfigFilePath();
        if (!File.Exists(configFilePath))
        {
            throw new Exception($"Tệp cấu hình {configFilePath} không tồn tại");
        }

        string fileContent;
        using (var streamReader = File.OpenText(configFilePath))
        {
            try
            {
                fileContent = await streamReader.ReadToEndAsync();
            }
            catch (Exception e)
            {
                throw new Exception($"Lỗi khi đọc tệp cấu hình {configFilePath}, {e.Message}");
            }
        }

        XElement commonXml;
        try
        {
            commonXml = XElement.Parse(fileContent);
        }
        catch (Exception e)
        {
            throw new Exception($"Lỗi khi phân tích tệp cấu hình {configFilePath}, {e.Message}");
        }
        LoadXmlItems(commonXml, "menpai", menpaiMap);
        LoadXmlItems(commonXml, "attr1", attr1Map);
        LoadXmlItems(commonXml, "attr2", attr2Map);
    }

    private static void LoadXmlItems(XElement commonXml, string itemTag, SortedDictionary<int, string> nameMap)
    {
        var parentElement = commonXml.Descendants(itemTag).First();
        if (parentElement is null)
        {
            return;
        }
        int itemValue;
        foreach (var itemElement in parentElement.Descendants("item"))
        {
            itemValue = Convert.ToInt32(itemElement.Attribute("value")?.Value ?? "0");
            nameMap.Add(itemValue, itemElement.Value);
        }
    }
}
