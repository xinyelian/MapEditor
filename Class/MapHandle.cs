using LitJson;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace MapEditor
{
    // 项目数据
    public class ProjData
    {
        public MapData MapData;
        public string MapImgPath;   // 地图图片路径
        public int ScaleRate;       // 地图缩放比例 50 表示 50%
    }

    // 地图数据
    public class MapData
    {
        public string Name;
        public int Width;
        public int Height;
        public int CellSize;
        public Dictionary<string, Cell> Cells;
    }

    public class MapHandle
    {
        private static MapHandle instance = new MapHandle();
        private MapHandle() { }
        public static MapHandle Instance
        {
            get
            {
                return instance;
            }

        }

        public ProjData ProjData;

        public bool Edited; // 是否編輯過
        public BitmapImage MapImg = null;

        public MapData MapData
        {
            get
            {
                if (ProjData == null)
                    return null;

                return ProjData.MapData;
            }
        }

        // 地图图片宽度
        public int ImgWidth
        {
            get
            {
                return (int)MapImg.Width;
            }
        }

        // 地图图片高度
        public int ImgHeight
        {
            get
            {
                return (int)MapImg.Height;
            }
        }

        // 编辑网格大小
        public int EditCellSize
        {
            get
            {
                return MapData.CellSize * 100 / ProjData.ScaleRate;
            }
        }

        // 游戏地图实际宽度
        public int MapWidth
        {
            get
            {
                return ImgWidth * ProjData.ScaleRate / 100;
            }
        }

        // 游戏地图实际高度
        public int MapHeight
        {
            get
            {
                return ImgHeight * ProjData.ScaleRate / 100;
            }
        }

        // 导出纯地图数据，游戏里面用
        public string ExportMapData()
        {
            if (ProjData == null)
                return null;

            ProjData.MapData.Width = MapWidth;
            ProjData.MapData.Height = MapHeight;
            return JsonMapper.ToJson(ProjData.MapData);
        }

        // 导出工程数据
        public string ExportProj()
        {
            if (ProjData == null)
                return null;

            ProjData.MapData.Width = MapWidth;
            ProjData.MapData.Height = MapHeight;
            return JsonMapper.ToJson(ProjData);
        }

        // 导入工程数据
        public bool ImportProj(string jsData)
        {
            try
            {
                ProjData = JsonMapper.ToObject<ProjData>(jsData);
                Edited = true;
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public void AddCell(Cell cell)
        {
            if (MapData.Cells.ContainsKey(cell.Key))
                return;

            MapData.Cells[cell.Key] = cell;
            Edited = true;
        }

        public void RemoveCell(string key)
        {
            if (!MapData.Cells.ContainsKey(key))
                return;

            MapData.Cells.Remove(key);
            Edited = true;
        }

        // 新建工程
        public void NewProject(string filePath)
        {
            ProjData = new ProjData()
            {
                MapData = new MapData()
                {
                    Name = "map_001",
                    CellSize = 32,   
                    Cells = new Dictionary<string, Cell>()
                },
                ScaleRate = 100,
                MapImgPath = filePath
            };
            Edited = true;
        }
    }
}
