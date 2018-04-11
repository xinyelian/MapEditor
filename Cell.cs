using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapEditor
{
    public class Cell
    {
        public int x;
        public int y;
        public int type;
        public string ext;  // 拓展数据

        public string Key
        {
            get
            {
                return y + "_" + x;
            }
        }
    }
}
