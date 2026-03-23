using System;
using System.Collections.Generic;
using System.Text;

namespace liuguang.Dbc;

public static class EncodingService
{
    private static readonly Dictionary<byte, char> VISCII_TO_UNICODE_MAP = new Dictionary<byte, char>
    {
        { 0x84, '\u1ea4' }, // Ấ
        { 0x86, '\u1ea8' }, // Ẩ
        { 0xA1, '\u1eaf' }, // ắ
        { 0xA2, '\u1eb1' }, // ằ
        { 0xA3, '\u1eb7' }, // ặ
        { 0xA4, '\u1ea5' }, // ấ
        { 0xA5, '\u1ea7' }, // ầ
        { 0xA6, '\u1ea9' }, // ẩ
        { 0xA7, '\u1ead' }, // ậ
        { 0xA8, '\u1ebd' }, // ẽ
        { 0xA9, '\u1eb9' }, // ẹ
        { 0xAA, '\u1ebf' }, // ế
        { 0xAB, '\u1ec1' }, // ề
        { 0xAC, '\u1ec3' }, // ể
        { 0xAD, '\u1ec5' }, // ễ
        { 0xAE, '\u1ec7' }, // ệ
        { 0x8F, '\u1ed0' }, // Ố
        { 0x90, '\u1ed2' }, // Ồ
        { 0x9C, '\u1ed0' }, // Ố (Duplicate in mapping?)
        { 0xAF, '\u1ed1' }, // ố
        { 0xB0, '\u1ed3' }, // ồ
        { 0xB1, '\u1ed5' }, // ổ
        { 0xB2, '\u1ed7' }, // ỗ
        { 0xB5, '\u1ed9' }, // ộ
        { 0xB6, '\u1edd' }, // ờ
        { 0xB7, '\u1edf' }, // ở
        { 0xBD, '\u01a1' }, // ơ
        { 0xBE, '\u1edb' }, // ớ
        { 0xBF, '\u01af' }, // Ư
        { 0xB8, '\u1ecb' }, // ị
        { 0xBA, '\u1ee8' }, // Ứ
        { 0xD0, '\u0110' }, // Đ
        { 0xF0, '\u0111' }, // đ
        { 0xD1, '\u1ee9' }, // ứ
        { 0xD7, '\u1eeb' }, // ừ
        { 0xD8, '\u1eed' }, // ử
        { 0xDF, '\u01b0' }, // ư
        { 0xF1, '\u1ef1' }, // ự
        { 0xFE, '\u1ee3' }, // ợ
        { 0xD5, '\u1ea1' }, // ạ
        { 0xE4, '\u1ea3' }, // ả
        { 0xE5, '\u0103' }, // ă
        { 0xEB, '\u1ebb' }, // ẻ
        { 0xEE, '\u0129' }, // ĩ
        { 0xEF, '\u1ec9' }, // ỉ
        { 0xDE, '\u1ee1' }, // ỡ
        { 0xF6, '\u1ecf' }, // ỏ
        { 0xF7, '\u1ecd' }, // ọ
        { 0xE6, '\u1eef' }, // ữ
        { 0xF8, '\u1ee5' }, // ụ
        { 0xFB, '\u0169' }, // ũ
        { 0xFC, '\u1ee7' }, // ủ
        { 0xDB, '\u1ef9' }, // ỹ
        { 0xDC, '\u1ef5' }, // ỵ
        { 0xC0, '\u00c0' }, // À
        { 0xC1, '\u00c1' }, // Á
        { 0xC4, '\u1ea2' }, // Ả
        { 0xC6, '\u1eb3' }, // ẳ
        { 0xC7, '\u00c7' }, // Ç
        { 0xC8, '\u00c8' }, // È
        { 0xC9, '\u00c9' }, // É
        { 0xCC, '\u00cc' }, // Ì
        { 0xCD, '\u00cd' }, // Í
        { 0xCF, '\u1ef3' }, // ỳ
        { 0xD2, '\u00d2' }, // Ò
        { 0xD3, '\u00d3' }, // Ó
        { 0xD6, '\u1ef7' }, // ỷ
        { 0xD9, '\u00d9' }, // Ù
        { 0xDA, '\u00da' }, // Ú
        { 0xDD, '\u00dd' }, // Ý
        { 0xE0, '\u00e0' }, // à
        { 0xE1, '\u00e1' }, // á
        { 0xE2, '\u00e2' }, // â
        { 0xE3, '\u00e3' }, // ã
        { 0xE7, '\u1eab' }, // ẫ
        { 0xE8, '\u00e8' }, // è
        { 0xE9, '\u00e9' }, // é
        { 0xEA, '\u00ea' }, // ê
        { 0xEC, '\u00ec' }, // ì
        { 0xED, '\u00ed' }, // í
        { 0xF2, '\u00f2' }, // ò
        { 0xF3, '\u00f3' }, // ó
        { 0xF4, '\u00f4' }, // ô
        { 0xF5, '\u00f5' }, // õ
        { 0xF9, '\u00f9' }, // ù
        { 0xFA, '\u00fa' }, // ú
        { 0xFD, '\u00fd' }  // ý
    };

    private static readonly Encoding Win1252Encoding;

    static EncodingService()
    {
        // On .NET Core, we would need Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        // But the library might be used in a host that already did that.
        // For .NET 4.8 it just works.
        try
        {
            Win1252Encoding = Encoding.GetEncoding("windows-1252");
        }
        catch (ArgumentException)
        {
            // Fallback to ASCII if not available, but should be available on Windows
            Win1252Encoding = Encoding.ASCII;
        }
    }

    public static string DecodeViscii(byte[] data)
    {
        return DecodeViscii(data, 0, data.Length);
    }

    public static string DecodeViscii(byte[] data, int index, int count)
    {
        var sb = new StringBuilder(count);
        for (var i = 0; i < count; i++)
        {
            var b = data[index + i];
            if (VISCII_TO_UNICODE_MAP.TryGetValue(b, out var c))
            {
                sb.Append(c);
            }
            else if (b < 128)
            {
                sb.Append((char)b);
            }
            else
            {
                // Use Windows-1252 for unmapped characters
                var fallback = Win1252Encoding.GetString(new[] { b });
                sb.Append(fallback);
            }
        }
        return sb.ToString();
    }
}
