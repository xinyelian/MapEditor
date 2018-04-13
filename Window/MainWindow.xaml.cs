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
        private string projFilePath = ""; // 当前项目文件        

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

            // 使用一张图片来新建项目
            NewProject(dialog.FileNames[0]);

            // 设置参数
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.ShowDialog();

            // 创建地图
            CreateMap();
        }

        // 新建项目
        private void NewProject(string imgFilePath)
        {            
            MapHandle.Instance.NewProject(imgFilePath); // 新建工程数据                             
            InitMap();                                  // 初始化地图背景，网格大小等
            projFilePath = "";                          // 清除当前文件保存路径              
        }

        private void InitMap()
        {
            MapHandle.Instance.MapImg = new BitmapImage(new Uri(MapHandle.Instance.ProjData.MapImgPath));
            Img.Source = MapHandle.Instance.MapImg;
            Map.Width = Img.Source.Width;
            Map.Height = Img.Source.Height;
            Grid.Width = Img.Source.Width;
            Grid.Height = Img.Source.Height;
        }

        // 创建地图
        private void CreateMap()
        {
            CreateGrid(); // 创建网格

            lostBrushes.Clear();

            // 画单元格
            Grid.Children.Clear();
            foreach (var item in MapHandle.Instance.MapData.Cells)
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

            for (int x = 0; x < Grid.Width / MapHandle.Instance.EditCellSize; x++)
            {

                ColumnDefinition col = new ColumnDefinition()
                {
                    Width = new GridLength(MapHandle.Instance.EditCellSize)
                };

                Grid.ColumnDefinitions.Add(col);

            }

            for (int y = 0; y < Grid.Height / MapHandle.Instance.EditCellSize; y++)
            {
                RowDefinition row = new RowDefinition()
                {

                    Height = new GridLength(MapHandle.Instance.EditCellSize),

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
                Width = MapHandle.Instance.EditCellSize,
                Height = MapHandle.Instance.EditCellSize,
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
            AboutWindow window = new AboutWindow();
            window.ShowDialog();
        }

        private void MItemOpt_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.ProjData == null)
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
            if (MapHandle.Instance.MapData == null)
                return;

            SaveFileDialog sf = new SaveFileDialog()
            {
                Title = "项目另存为",
                Filter = "项目文件(*.json)|*.json",
                RestoreDirectory = true, //保存对话框是否记忆上次打开的目录
                CheckPathExists = true,  //检查目录
                FileName = MapHandle.Instance.MapData.Name + "_proj"    //默认名
            };

            if (sf.ShowDialog() == true)
            {
                projFilePath = sf.FileName;
                SaveProj();
            }
        }    

        private void MItemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveProj();
        }

        // 保存工程
        private void SaveProj()
        {
            if (!MapHandle.Instance.Edited || MapHandle.Instance.ProjData == null)
                return;
            
            string projJson = MapHandle.Instance.ExportProj();
            if (projJson == null || projJson == "")
            {
                MessageBox.Show("没有地图工程数据！", "提示");
                return;
            }

            // 保存工程数据
            if (projFilePath == "")
                projFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, MapHandle.Instance.MapData.Name + "_proj.json");
            using (StreamWriter stream = new StreamWriter(projFilePath))
            {
                stream.Write(projJson);
                stream.Close();

                MapHandle.Instance.Edited = false;
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
                projFilePath = dialog.FileNames[0];

                if (!projFilePath.Contains("_proj"))
                {
                    MessageBox.Show("不是有效的项目文件类型(_proj)。", "导入失败");
                    return;
                }

                using (StreamReader reader = new StreamReader(projFilePath))
                {
                    string json = reader.ReadToEnd();
                    bool ok = MapHandle.Instance.ImportProj(json);
                    if (!ok)
                    {
                        MessageBox.Show("【错误原因】\n1.文件格式错误。\n2.没法兼容旧文件版本。", "导入失败");
                        return;
                    }
                                        
                    InitMap();
                    CreateMap();
                }
            }
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveProj();
            Setting.Instance.SaveCfg();
        }

        // 重置，清除所有单元格信息
        private void CtxMenuItem_Reset_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.MapData.Cells.Count > 0)
            {
                // 弹窗确认
                MessageBoxResult boxResult = MessageBox.Show("是否清除所有绘制单元格信息？\n【保存】之前重新导入该地图文件即可恢复。", "警告", MessageBoxButton.OKCancel);
                if (boxResult == MessageBoxResult.OK)
                {
                    MapHandle.Instance.MapData.Cells.Clear();
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
                int row = (int)p.Y / MapHandle.Instance.EditCellSize;
                int col = (int)p.X / MapHandle.Instance.EditCellSize;
                string key = row + "_" + col; 

                if (MapHandle.Instance.MapData.Cells.ContainsKey(key))
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
            
            menuItem = new MenuItem()
            {
                Header = "笔刷管理",
                Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/管理.png", UriKind.RelativeOrAbsolute)) }
            };
            menuItem.Click += Manage_Brush;
            menu.Items.Add(menuItem);

            separator = new Separator();
            menu.Items.Add(separator);

            menuItem = new MenuItem()
            {
                Header = "地图设置",
                Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/设置.png", UriKind.RelativeOrAbsolute)) }
            };
            menuItem.Click += MItemOpt_Click;
            menu.Items.Add(menuItem);

            menuItem = new MenuItem()
            {
                Header = "重置地图",
                Icon = new Image() { Source = new BitmapImage(new Uri("/MapEditor;component/Resources/重置.png", UriKind.RelativeOrAbsolute)) }
            };
            menuItem.Click += CtxMenuItem_Reset_Click;
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

        // 打开编辑器配置目录
        private void MItemOpenCfg_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(Setting.Instance.FileName))
            {
                MessageBox.Show("没有编辑器配置文件！", "提示");
                return;
            }

            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
            {
                Arguments = "/e,/select," + Setting.Instance.FileName
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        // 打开项目所在目录
        private void MItemProjFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(projFilePath))
            {
                MessageBox.Show("当前没有项目！", "提示");
                return;
            }

            System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo("Explorer.exe")
            {
                Arguments = "/e,/select," + projFilePath
            };
            System.Diagnostics.Process.Start(processStartInfo);
        }

        private void GridOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Setting.Instance.GridOpacity = GridOpacitySlider.Value;
            SetGridOpacity();
        }

        // 导出地图数据
        private void MItemExpMap_Click(object sender, RoutedEventArgs e)
        {
            if (MapHandle.Instance.ProjData == null)
                return;

            SaveFileDialog sf = new SaveFileDialog()
            {
                Title = "导出地图数据",
                Filter = "地图数据(*.json)|*.json",
                FileName = MapHandle.Instance.MapData.Name + "_data"    //默认名
            };

            if (sf.ShowDialog() == false)
                return;
            
            string mapDataJson = MapHandle.Instance.ExportMapData();
            if (mapDataJson == null || mapDataJson == "")
            {
                MessageBox.Show("没有地图数据！", "提示");
                return;
            }

            // 写入              
            using (StreamWriter stream = new StreamWriter(sf.FileName))
            {
                stream.Write(mapDataJson);
                stream.Close();
            }        
        }
    }
}
