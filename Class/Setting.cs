using LitJson;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MapEditor
{
    public class Setting
    {
        private static Setting instance = new Setting();
        private Setting() { }
        public static Setting Instance
        {
            get
            {
                return instance;
            }

        }

        public Action OnBrushModified;
        public Action OnCurBrushChanged;

        public EditorConfig Cfg;
        public string FileName;
        private bool isModified = false;    // 配置是否修改

        public Dictionary<string, Brush> Brushes
        {
            get
            {
                return Cfg.brushes;
            }
            set
            {
                Cfg.brushes = value;
            }
        }

        public Brush GetBrush(string type)
        {
            Brushes.TryGetValue(type, out Brush brush);
            return brush;
        }

        public int[] CustomColors
        {
            get
            {
                return Cfg.customColors.ToArray();
            }
            set
            {
                Cfg.customColors = new List<int>(value);
            }
        }

        public void Init()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);
            string cfgFolder = Path.Combine(path, "MapEditor");
            if (!Directory.Exists(cfgFolder))
                Directory.CreateDirectory(cfgFolder);

            FileName = Path.Combine(cfgFolder, "MapEditorCfg.json");

            // 创建默认配置
            if (!File.Exists(FileName))
            {
                InitDefault();
                SaveCfg();

                return;
            }

            try
            {
                using (StreamReader reader = File.OpenText(FileName))
                {
                    string json = reader.ReadToEnd();
                    Cfg = JsonMapper.ToObject<EditorConfig>(json);
                }
            }
            catch (System.Exception)
            {
                System.Windows.MessageBox.Show("配置文件已损坏,将初始化默认配置！", "错误");

                // 删除损害的配置文件，重新初始化
                if (File.Exists(FileName))
                    File.Delete(FileName);

                Init();
            }

            isModified = false;
        }

        // 当前使用的笔刷
        public Brush CurBrush
        {
            get
            {
                return GetBrush(Cfg.curBrushType);
            }
        }

        private void InitDefault()
        {
            Cfg = new EditorConfig
            {
                brushes = new Dictionary<string, Brush>()
            };
            Brush brush = new Brush()
            {
                Type = 1,
                Color = "Red",
                Desc = "不可行走"
            };
            Cfg.brushes[brush.Type.ToString()] = brush;

            // 自定义颜色
            Cfg.customColors = new List<int>() {
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#DC143C")), // Crimson 猩红
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#4169E1")), // RoyalBlue 皇家蓝
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#00BFFF")), // DeepSkyBlue 深天蓝
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#2E8B57")), // SeaGreen 海洋绿
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#32CD32")), // LimeGreen 酸橙绿
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#FFD700")), // Gold 金
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#FFA500")), // Orange 橙色
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#FF4500")), // OrangeRed 橙红色
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#FF6347")), // Tomato 番茄
                    ColorTranslator.ToWin32(ColorTranslator.FromHtml("#B22222")), // FireBrick 砖红
            };

            Cfg.gridOpacity = 0.7; // 默认网格透明度

            SetCurBrush(brush.Type);  // 设置默认当前笔刷
        
            isModified = true;
        }

        // 保存配置
        public void SaveCfg()
        {
            try
            {
                // 有修改才保存
                if (!isModified)
                    return;

                string json = JsonMapper.ToJson(Cfg);
                using (StreamWriter writer = File.CreateText(FileName))
                {
                    writer.WriteAsync(json);
                    writer.Flush();
                }

                isModified = false;
            }
            catch (System.Exception e)
            {
                System.Windows.MessageBox.Show(e.Message, "错误");
            }
        }

        public void AddBrush(Brush brush)
        {
            string key = brush.Type.ToString();
            if (Brushes.ContainsKey(key))
            {
                System.Windows.MessageBox.Show("已存在该类型的笔刷！", "提示");
                return;
            }

            Brushes.Add(key, brush);

            isModified = true;

            OnBrushModified?.Invoke();
        }

        private bool CheckExists(string type)
        {
            if (!Brushes.ContainsKey(type))
            {
                System.Windows.MessageBox.Show("笔刷不存在！", "提示");
                return false;
            }

            return true;
        }

        public void RemoveBrush(int type)
        {
            if (Brushes.Count <= 1)
            {
                System.Windows.MessageBox.Show("至少要保留一个笔刷！", "提示");
                return;
            }

            if (type == CurBrush.Type)
            {
                System.Windows.MessageBox.Show("当前使用笔刷不能被删除！", "提示");
                return;
            }

            if (!CheckExists(type.ToString()))
                return;

            Brushes.Remove(type.ToString());

            isModified = true;

            OnBrushModified?.Invoke();
        }

        public void ModifyColor(string type, string color)
        {
            if (!CheckExists(type))
                return;

            Brush brush = Brushes[type];
            brush.Color = color;

            isModified = true;

            OnBrushModified?.Invoke();

            if(brush.Type == CurBrush.Type)
                OnCurBrushChanged?.Invoke();
        }

        public void SetCustomColors(int[] colors)
        {
            CustomColors = colors;
            isModified = true;
        }

        public void SetCurBrush(int type)
        {
            Cfg.curBrushType = type.ToString();
            isModified = true;
            OnCurBrushChanged?.Invoke();
        }

        // 自动获取类型，取当前最大类型+1
        public int GetAutoType()
        {
            int type = 0;
            foreach (var item in Brushes.Values)
            {
                if (item.Type > type)
                    type = item.Type;
            }
            return ++type;
        }

        public double GridOpacity
        {
            get
            {
                return Cfg.gridOpacity;
            }
            set
            {
                Cfg.gridOpacity = value;

                isModified = true;
            }
        }
    }

    // 配置数据结构
    public class EditorConfig
    {
        public Dictionary<string, Brush> brushes;
        public string curBrushType;
        public List<int> customColors;
        public double gridOpacity;
    }
}
