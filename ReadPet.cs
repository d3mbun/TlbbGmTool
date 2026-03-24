using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("GBK");
        string path = @"F:\GAME\TLBB 2007\TLBB 2007\Client\Data\Config\PetAttrTable.txt";
        if (File.Exists(path))
        {
            var lines = File.ReadAllLines(path, encoding);
            for (int i = 0; i < Math.Min(5, lines.Length); i++)
            {
                var parts = lines[i].Split('\t');
                Console.WriteLine($"Row {i}:");
                for (int j = 0; j < Math.Min(10, parts.Length); j++)
                {
                    Console.WriteLine($"  Col {j}: {parts[j]}");
                }
            }
        }
        else
        {
            Console.WriteLine("File not found: " + path);
        }
    }
}
