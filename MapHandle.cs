using LitJson;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace MapEditor
{
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

        public bool Edited; // 是否編輯過
        public string MapName;
        public int OriginCellSize = 32;
        public int ScaleRate = 100;
        public BitmapImage MapImg = null;
        public string ImgPath = "";
        public Dictionary<string, Cell> Data = new Dictionary<string, Cell>();

        public int Width
        {
            get
            {
                return (int)MapImg.Width;
            }
        }

        public int Height
        {
            get
            {
                return (int)MapImg.Height;
            }
        }

        public int CellSize
        {
            get
            {
                return OriginCellSize * ScaleRate / 100;
            }
        }

        // 导出地图数据
        public string Export()
        {
            if (Data.Count == 0)
                return null;

            JsonData jsData = new JsonData
            {
                ["Cells"] = JsonMapper.ToObject(JsonMapper.ToJson(Data)),

                // 编辑器数据
                ["Editor"] = new JsonData()
            };
            jsData["Editor"]["MapImgPath"] = MapImg.UriSource.AbsolutePath;
            jsData["Editor"]["OriginCellSize"] = OriginCellSize;
            jsData["Editor"]["ScaleRate"] = ScaleRate;

            // 其他需要数据
            jsData["Name"] = MapName;
            jsData["CellSize"] = CellSize;
            jsData["Width"] = Width;
            jsData["Height"] = Height;

            return jsData.ToJson();
        }

        // 导入地图数据
        public bool Import(JsonData jsData)
        {
            try
            {
                Data = JsonMapper.ToObject<Dictionary<string, Cell>>(jsData["Cells"].ToJson());
                MapName = jsData["Name"].ToString();
                ImgPath = jsData["Editor"]["MapImgPath"].ToString();
                OriginCellSize = int.Parse(jsData["Editor"]["OriginCellSize"].ToString());
                ScaleRate = int.Parse(jsData["Editor"]["ScaleRate"].ToString());

                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public void AddCell(Cell cell)
        {
            if (Data.ContainsKey(cell.Key))
                return;

            Data[cell.Key] = cell;
            Edited = true;
        }

        public void RemoveCell(string key)
        {
            if (!Data.ContainsKey(key))
                return;

            Data.Remove(key);
            Edited = true;
        }
    }
}
