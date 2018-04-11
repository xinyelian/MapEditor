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
    /// OptionW.xaml 的交互逻辑
    /// </summary>
    public partial class OptionWindow : Window
    {
        public static bool changed = false;

        public OptionWindow()
        {
            InitializeComponent();
            changed = false;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (changed)
            {
                string name = TxtName.Text.Trim();
                if (name != "")
                    MapHandle.Instance.MapName = name;

                string cs = TxtCellSize.Text.Trim();
                if (cs != "")
                    MapHandle.Instance.OriginCellSize = int.Parse(cs);

                string sc = TxtScale.Text.Trim();
                if (sc != "")
                    MapHandle.Instance.ScaleRate = int.Parse(sc);
            }
            Close();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            changed = true;
        }

        private void OptWnd_Loaded(object sender, RoutedEventArgs e)
        {
            TxtName.Text = MapHandle.Instance.MapName;
            TxtWidth.Text = MapHandle.Instance.Width.ToString();
            TxtHeight.Text = MapHandle.Instance.Height.ToString();
            TxtCellSize.Text = MapHandle.Instance.OriginCellSize.ToString();
            TxtScale.Text = MapHandle.Instance.ScaleRate.ToString();
        }
    }
}
