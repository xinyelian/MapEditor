using System.Windows.Forms;

namespace MapEditor
{
    public class MyColorDialog
    {
        public static ColorDialog Dialog = new ColorDialog();

        public static DialogResult Show()
        {
            Dialog.CustomColors = Setting.Instance.CustomColors;            

            DialogResult result = Dialog.ShowDialog();

            // 检查自定义颜色是否变化
            bool changed = false;
            if (Dialog.CustomColors.Length != Setting.Instance.CustomColors.Length)
                changed = true;
            else
            {
                for (int i = 0; i < Dialog.CustomColors.Length; i++)
                {
                    if (Dialog.CustomColors[i] != Setting.Instance.CustomColors[i]) {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
                Setting.Instance.SetCustomColors(Dialog.CustomColors);
            
            return result;
        }
    }
}
