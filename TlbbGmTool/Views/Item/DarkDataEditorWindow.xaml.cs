using System.Windows;

namespace liuguang.TlbbGmTool.Views.Item;
/// <summary>
/// Logic tương tác cho DarkDataEditorWindow.xaml
/// </summary>
public partial class DarkDataEditorWindow : Window
{
    public string HexData { get; set; } = string.Empty;
    public DarkDataEditorWindow()
    {
        InitializeComponent();
    }
}
