using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MapEditor
{
    /// <summary>
    /// BrushWindow.xaml 的交互逻辑
    /// </summary>
    public partial class BrushWindow : Window    {

        public BrushWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Setting.Instance.Cfg == null)
                return;

            RefreshList();
        }

        // 刷新列表
        private void RefreshList()
        {
            Brush[] brushes = new Brush[Setting.Instance.Brushes.Count];
            Setting.Instance.Brushes.Values.CopyTo(brushes, 0);
            Lst.ItemsSource = brushes;
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int type = (int)button.CommandParameter;
            DelBrush(type);
        }

        private void Del_SelectedItem_Click(object sender, RoutedEventArgs e)
        {
            if (Lst.SelectedItem == null)
            {
                MessageBox.Show("请选择需要删除的行！","提示");
                return;
            }

            Brush b = Lst.SelectedItem as Brush;
            DelBrush(b.Type);
        }

        // 删除笔刷
        private void DelBrush(int type)
        {
            Setting.Instance.RemoveBrush(type.ToString());
            RefreshList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            AddBrush();
        }

        // 新增笔刷
        private void AddBrush()
        {
            NewBrushWindow window = new NewBrushWindow();
            window.ShowDialog();
            Brush newBrush = window.Brush;
            if (newBrush == null)
                return;

            Setting.Instance.AddBrush(newBrush);
            RefreshList();
        }

        // 设为当前使用笔刷
        private void Use_Click(object sender, RoutedEventArgs e)
        {
            if (Lst.SelectedItem == null)
            {
                MessageBox.Show("请选择需要使用的行！", "提示");
                return;
            }

            Setting.Instance.SetCurBrush(Lst.SelectedItem as Brush);
        }

        // 编辑笔刷颜色
        private void EditColor(object sender, MouseButtonEventArgs e)
        {
            // 颜色板
            if (MyColorDialog.Show() == System.Windows.Forms.DialogResult.OK)
            {
                string color = System.Drawing.ColorTranslator.ToHtml(MyColorDialog.Dialog.Color);
                Rectangle rectangle = sender as Rectangle;
                string type = rectangle.Tag.ToString();
                if (Setting.Instance.Brushes.ContainsKey(type))
                {
                    Setting.Instance.ModifyColor(type, color);
                    rectangle.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
                }
            }
        }

        private void New_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddBrush();
        }

        private void Save_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Setting.Instance.SaveCfg();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Setting.Instance.SaveCfg();
        }
    }
}
