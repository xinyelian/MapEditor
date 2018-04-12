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
                    MapHandle.Instance.MapData.Name = name;

                string cs = TxtCellSize.Text.Trim();
                if (cs != "")
                    MapHandle.Instance.MapData.CellSize = int.Parse(cs);

                string sc = CbxScale.Text.Trim();
                if (sc != "")
                    MapHandle.Instance.ProjData.ScaleRate = int.Parse(sc);
            }
            Close();
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            changed = true;
        }

        private void OptWnd_Loaded(object sender, RoutedEventArgs e)
        {
            TxtName.Text = MapHandle.Instance.MapData.Name;
            TxtWidth.Text = MapHandle.Instance.ImgWidth.ToString();
            TxtHeight.Text = MapHandle.Instance.ImgHeight.ToString();
            TxtCellSize.Text = MapHandle.Instance.MapData.CellSize.ToString();
            CbxScale.Text = MapHandle.Instance.ProjData.ScaleRate.ToString();
        }
    }
}
