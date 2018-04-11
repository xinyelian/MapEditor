using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LitJson;
using System.Collections.Generic;

namespace MapEditor
{
    public partial class MainWindow : Window
    {
        private readonly string defaultFileName = "mapData";
        private string curFilePath = ""; // 当前编辑的文件        

        public bool IsLeftMouseDown;
        public bool IsErase;  // 橡皮擦模式

        private List<int> lostBrushes = new List<int>(); // 丢失的笔刷

        public MainWindow()
        {
            InitializeComponent();

            // 监听笔刷变化，设置右键菜单
            Setting.Instance.OnBrushModified += OnBrushModified;
            Setting.Instance.OnCurBrushChanged += SetMenuCurBrushIcon;
        }      

        ~MainWindow()
        {
            Setting.Instance.OnBrushModified -= OnBrushModified;
            Setting.Instance.OnCurBrushChanged -= SetMenuCurBrushIcon;
        }

        // 画笔更新
        private void OnBrushModified()
        {
            SetContextMenu();
            CreateMap();
        }

        private void MItemNew_Click(object sender, RoutedEventArgs e)
        {

            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "选择地图背景图",
                Filter = "图像文件(*.jpg,*.png)|*.jpg;*.png",
                RestoreDirectory = true, //保存对话框是否记忆上次打开的目录
                CheckPathExists = true,  //检查目录
            };
            bool? ok = dialog.ShowDialog();
            if (ok == false) return;

            MapHandle.Instance.Data.Clear();
            MapHandle.Instance.ImgPath = dialog.FileNames[0];
            MapHandle.Instance.MapName = "map_001";

            curFilePath = "";    // 清除当前文件保存路径

            Init();

            // 设置网格大小
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.ShowDialog();

            CreateMap();
        }

        private void Init()
        {
            MapHandle.Instance.MapImg = new BitmapImage(new Uri(MapHandle.Instance.ImgPath));
            Img.Source = MapHandle.Instance.MapImg;
            Map.Width = Img.Source.Width;
            Map.Height = Img.Source.Height;
            Grid.Width = Img.Source.Width;
            Grid.Height = Img.Source.Height;
        }

        private void CreateMap()
        {
            CreateGrid(); // 创建网格

            lostBrushes.Clear();

            // 画单元格
            Grid.Children.Clear();
            foreach (var item in MapHandle.Instance.Data)
            {
                CreateCell(item.Value);
            }

            if (lostBrushes.Count > 0)
            {
                string lostStr = "";
                for (int i = 0; i < lostBrushes.Count; i++)
                    lostStr += "【" + lostBrushes[i] + "】";
                MessageBox.Show("丢失笔刷类型：" + lostStr + "\n默认使用【白色】绘制，请重新创建丢失笔刷。", "提示");
            }
        }

        // 创建网格
        private void CreateGrid()
        {
            Grid.ColumnDefinitions.Clear();
            Grid.RowDefinitions.Clear();

            for (int x = 0; x < Grid.Width / MapHandle.Instance.CellSize; x++)
            {

                ColumnDefinition col = new ColumnDefinition()
                {
                    Width = new GridLength(MapHandle.Instance.CellSize)
                };

                Grid.ColumnDefinitions.Add(col);

            }

            for (int y = 0; y < Grid.Height / MapHandle.Instance.CellSize; y++)
            {
                RowDefinition row = new RowDefinition()
                {

                    Height = new GridLength(MapHandle.Instance.CellSize),

                };

                Grid.RowDefinitions.Add(row);
            }
        }

        // 创建单元格
        private void CreateCell(Cell cell)
        {             
            Color c = Colors.White;
            Brush brush = Setting.Instance.GetBrush(cell.type.ToString());
            if (brush != null)
                c = (Color)ColorConverter.ConvertFromString(brush.Color);
            else if (!lostBrushes.Contains(cell.type))
                lostBrushes.Add(cell.type);

            Rectangle rectangle = new Rectangle()
            {
                Width = MapHandle.Instance.CellSize,
                Height = MapHandle.Instance.CellSize,
                Fill = new SolidColorBrush(c),
                Stroke = new SolidColorBrush(Colors.Black),
                RadiusX = 4,
                RadiusY = 4,
            };
            
            Grid.Children.Add(rectangle);
            Grid.SetRow((UIElement)rectangle, cell.y);
            Grid.SetColumn((UIElement)rectangle, cell.x);
            MapHandle.Instance.AddCell(cell);
        }

        private void MItemAbt_Click(object sender, RoutedEventArgs e)
        {
            string about = "【如何用】\n1.右键地图选择/管理笔刷。\n2.按住Ctrl键为擦除模式。\n3.点击鼠标左键刷图或擦除（擦除模式时）单元格。\n4.按住鼠标左键为连续模式，移动鼠标即可。\n5.导出文件为json格式,配置存放于[C:\\Users\\...\\AppData\\Local\\MapEditor]。\n\n【开发者】\n张元涛 QQ/WeChat:735162787";
            MessageBox.Show(about,"关于编辑器");
        }

        private void MItemOpt_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.MapImg == null)
                return;

            OptionWindow window = new OptionWindow();
            window.ShowDialog();

            if (OptionWindow.changed)
                CreateMap();
        }

        private void CBoxShowGridLine_Checked(object sender, RoutedEventArgs e)
        {
            Grid.ShowGridLines = true;
        }

        private void CBoxShowGridLine_Unchecked(object sender, RoutedEventArgs e)
        {
            Grid.ShowGridLines = false;
        }     

        // 右键选择设置笔刷
        private void CtxMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string type = item.Tag.ToString();
            Brush brush = Setting.Instance.GetBrush(type);
            if (brush != null)
                Setting.Instance.SetCurBrush(brush.Type);
        }

        // 导出(另存为)
        private void MItemExp_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.Data == null || MapHandle.Instance.Data.Count <= 0)
                return;

            SaveFileDialog sf = new Microsoft.Win32.SaveFileDialog()
            {
                Title = "另存为",
                Filter = "地图数据(*.json)|*.json",
                RestoreDirectory = true, //保存对话框是否记忆上次打开的目录
                CheckPathExists = true,  //检查目录
                FileName = defaultFileName    //默认名
            };

            if (sf.ShowDialog() == true)
            {
                Save(sf.FileName);

                if (curFilePath != sf.FileName)
                    curFilePath = sf.FileName;
            }
        }    

        private void MItemSave_Click(object sender, RoutedEventArgs e)
        {
            ToSave();
        }

        private void ToSave()
        {
            if (curFilePath == "")
                curFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, defaultFileName + ".json");
            Save(curFilePath);
        }

        // 保存
        private void Save(string filePath)
        {
            if (!MapHandle.Instance.Edited || MapHandle.Instance.Data == null || MapHandle.Instance.Data.Count <= 0)
                return;

            string jsonData = MapHandle.Instance.Export();
            if (jsonData == null)
            {
                MessageBox.Show("没有地图数据！", "提示");
                return;
            }

            using (StreamWriter stream = new StreamWriter(filePath))
            {
                stream.Write(jsonData);
            }
        }

        // 导入地图数据
        private void MItemImp_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "导入地图数据",
                Filter = "地图数据(*.json)|*.json",
                RestoreDirectory = true, //保存对话框是否记忆上次打开的目录
                CheckPathExists = true,  //检查目录
            };
            
            if(dialog.ShowDialog() == true)
            {
                curFilePath = dialog.FileNames[0];
                using (StreamReader reader = new StreamReader(curFilePath))
                {
                    string json = reader.ReadToEnd();
                    JsonData jsonData = JsonMapper.ToObject(json);
                    bool ok = MapHandle.Instance.Import(jsonData);
                    if (!ok)
                    {
                        MessageBox.Show("【错误原因】\n1.文件格式错误。\n2.没法兼容旧文件版本。", "导入失败");
                        return;
                    }
                    Init();
                    CreateMap();
                }
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ToSave();
            Setting.Instance.SaveCfg();
        }

        // 重置，清除所有单元格信息
        private void CtxMenuItem_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.Data.Count > 0)
            {
                // 弹窗确认
                MessageBoxResult boxResult = MessageBox.Show("是否清除所有绘制单元格信息？\n（空地图将不会保存，重新导入该地图文件即可恢复。）", "警告", MessageBoxButton.YesNo);
                if (boxResult == MessageBoxResult.Yes)
                {
                    MapHandle.Instance.Data.Clear();
                    CreateMap();

                }
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsLeftMouseDown = true;
            DrawOrErase(sender, e);
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsLeftMouseDown = false;           
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;
            DrawOrErase(sender, e);                
        }

        private void DrawOrErase(object sender, MouseEventArgs e)
        {
            if (!IsLeftMouseDown)
                return;           

            if (IsErase)
            {
                if (e.Source is Rectangle)
                {
                    int row = Grid.GetRow((UIElement)e.Source);
                    int col = Grid.GetColumn((UIElement)e.Source);
                    string key = row + "_" + col;
                    MapHandle.Instance.RemoveCell(key);
                    Grid.Children.Remove((UIElement)e.Source);
                }
            }
            else
            {
                Point p = e.GetPosition((IInputElement)sender);
                int row = (int)p.Y / MapHandle.Instance.CellSize;
                int col = (int)p.X / MapHandle.Instance.CellSize;
                string key = row + "_" + col; 

                if (MapHandle.Instance.Data.ContainsKey(key))
                    return;

                Cell cell = new Cell()
                {
                    x = col,
                    y = row,
                    type = (int)Setting.Instance.CurBrush.Type,
                    ext = String.Empty
                };
                CreateCell(cell);
            }
        }
        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                IsErase = true;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                IsErase = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadEditorCfg();
            SetGridOpacity();
            SetContextMenu();
            SetMenuCurBrushIcon();
        }

        // 加载编辑器设置
        private void LoadEditorCfg()
        {
            Setting.Instance.Init();
            GridOpacitySlider.Value = Setting.Instance.GridOpacity;
        }

        private void SetGridOpacity()
        {
            Grid.Opacity = Setting.Instance.GridOpacity;
        }

        // 设置右键菜单
        private void SetContextMenu()
        {
            if (Setting.Instance.Cfg == null)
                return;

            ContextMenu menu = new ContextMenu();
            MenuItem menuItem = new MenuItem()
            {
                Header = "选择笔刷",
                Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/笔刷.png", UriKind.RelativeOrAbsolute))}
            };
            menuItem.StaysOpenOnClick = true;
            menuItem.IsHitTestVisible = false;
            menu.Items.Add(menuItem);

            Separator separator = new Separator();
            menu.Items.Add(separator);

            // 添加笔刷
            foreach (var item in Setting.Instance.Brushes)
            {
                Brush brush = item.Value;
                menuItem = new MenuItem()
                {
                    Header = brush.Desc,
                    Tag = brush.Type,
                    Margin = new Thickness(30, 0, 0, 0),
                    Padding = new Thickness(-10, 0, 0, 0)
            };
                menuItem.Click += CtxMenuItem_Click;
                menuItem.Icon = new Rectangle() {                   
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(brush.Color))                  
                };
                menu.Items.Add(menuItem);
            }

            separator = new Separator();
            menu.Items.Add(separator);

            menuItem = new MenuItem() {
                Header = "重置地图",
                 Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/重置.png", UriKind.RelativeOrAbsolute)) }
            };
            menuItem.Click += CtxMenuItem_Reset_Click;
            menu.Items.Add(menuItem);

            separator = new Separator();
            menu.Items.Add(separator);

            menuItem = new MenuItem()
            {
                Header = "笔刷管理",
                Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/管理.png", UriKind.RelativeOrAbsolute)) }
            };
            menuItem.Click += Manage_Brush;
            menu.Items.Add(menuItem);

            if(Grid.ContextMenu != null)
                Grid.Children.Remove(Grid.ContextMenu);
            Grid.ContextMenu = menu;
        }

        // 设置菜单当前笔刷Icon
        public void SetMenuCurBrushIcon()
        {
            Rectangle rectangle = new Rectangle()
            {
                Width = 14,
                Height = 14,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Setting.Instance.CurBrush.Color)),
            };
            MItemBrush.Icon = rectangle;
        }

        // 画笔管理
        private void Manage_Brush(object sender, RoutedEventArgs e)
        {
            BrushWindow window = new BrushWindow();
            window.ShowDialog();
        }

        private void MItemOpenCfg_Click(object sender, RoutedEventArgs e)
        {
            if (Setting.Instance.FileName == "")
            {
                MessageBox.Show("没有编辑器配置文件！", "提示");
                return;
            }
            System.Diagnostics.Process.Start(Setting.Instance.FileName);
        }

        private void GridOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Setting.Instance.GridOpacity = GridOpacitySlider.Value;
            SetGridOpacity();
        }
    }
}
