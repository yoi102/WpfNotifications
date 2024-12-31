using System.Windows.Controls;

namespace Notifications.Mvvm.Sample.Messages
{
    /// <summary>
    /// UserControlMessage.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlMessage : UserControl
    {
        public UserControlMessage(string string1, string string2)
        {
            InitializeComponent();
            textBox1.Text = string1;
            textBox2.Text = "×" + string2;
        }
    }
}