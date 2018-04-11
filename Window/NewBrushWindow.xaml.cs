using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// NewBrushWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewBrushWindow : Window
    {
        public NewBrushWindow()
        {
            InitializeComponent();
        }

        public Brush Brush = null;
        private string color = System.Drawing.ColorTranslator.ToHtml(System.Drawing.Color.Black);

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            string errMsg = Check();
            if (errMsg != null)
            {
                MessageBox.Show(errMsg, "提示");
                return;
            }

            Brush = new Brush()
            {
                Color = color,
                Type = int.Parse(TxtType.Text.Trim()),
                Desc = TxtDesc.Text
            };

            if (Setting.Instance.Brushes.ContainsKey(Brush.Type.ToString()))
            {
                MessageBox.Show("已存在该类型为【" + Brush.Type + "】的笔刷！", "提示");
                return;
            }
            
            Close();
        }

        private string Check()
        {
            string type = TxtType.Text.Trim();
            if (type == "")
                return "类型不能为空！";

            try
            {
                int intType = int.Parse(type);
            }
            catch (FormatException)
            {
                return "类型只能是整数！";
            }

            string desc = TxtDesc.Text.Trim();
            if (desc == "")
                return "描述不能为空！";

            return null;
        }

        private void ColorPicker_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MyColorDialog.Show() == System.Windows.Forms.DialogResult.OK)
                color = System.Drawing.ColorTranslator.ToHtml(MyColorDialog.Dialog.Color);

            ColorPicker.Fill = new SolidColorBrush()
            {
                Color = (Color)ColorConverter.ConvertFromString(color)
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtType.Text = Setting.Instance.GetAutoType().ToString();
        }
    }
}
