using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.Kernel;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenFileDialog = Rhino.UI.OpenFileDialog;

namespace WatermarkPainter
{
    internal static class MenuCreater
    {
        public static void CreateMenu(GH_DocumentEditor editor)
        {
            ToolStripMenuItem displayItem = (ToolStripMenuItem)editor.MainMenuStrip.Items[3];
            displayItem.DropDownItems.Insert(3, MenuItem());
        }

        static ToolStripMenuItem MenuItem()
        {
            ToolStripMenuItem major = new ToolStripMenuItem("Watermark") { ToolTipText = "Change the settings of watermark." };

            var file = new ToolStripMenuItem("File Location");
            file.ToolTipText = WPainter.ImagePath;
            file.Click += (s, e) =>
            {
                var open = new OpenFileDialog();
                open.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
                open.Title = "A Image File";
                open.FileName = WPainter.ImagePath;
                open.MultiSelect = false;
                if (open.ShowOpenDialog())
                {
                    WPainter.ImagePath = open.FileName;
                };
            };
            major.DropDownItems.Add(file);

            major.DropDownItems.Add(CreateEnum("Alignment", WPainter.Alignment, v => WPainter.Alignment = v));

            CreateNumberBox(major, "Image Ratio", WPainter.ImageRatio, (v) => WPainter.ImageRatio = (float)v, WPainter.ImageRatioDefault, 5, 0, 3);

            GH_DocumentObject.Menu_AppendSeparator(major.DropDown);

            CreateNumberBox(major, "FPS", WPainter.GifFps, (v) => WPainter.GifFps = (int)v, WPainter.GifFpsDefault, 60, 5);

            return major;
        }

        private static ToolStripMenuItem CreateEnum<T>(string itemName, T origin, Action<T> save) where T : Enum
        {
            var result = new ToolStripMenuItem(itemName);

            result.DropDown.Closing -= DropDown_Closing;
            result.DropDown.Closing += DropDown_Closing;

            var names = Enum.GetNames(typeof(T));
            var values = (int[])Enum.GetValues(typeof(T));

            if (names.Length != values.Length) return result;

            for (int i = 0; i < names.Length; i++)
            {
                var v = values[i];
                var n = names[i];

                var item = new ToolStripMenuItem(n);
                item.Checked = (int)(object)origin == v;
                item.Tag = v;
                item.Click += (s, e)=>
                {
                    var m = (ToolStripMenuItem)s;
                    foreach (ToolStripMenuItem dp in result.DropDownItems)
                    {
                        dp.Checked = false;
                    }
                    m.Checked = true;
                    save?.Invoke((T)m.Tag);
                };
                result.DropDownItems.Add(item);
            }

            return result;
        }

        private static void CreateNumberBox(ToolStripMenuItem item, string itemName, double originValue, Action<double> valueChange, double valueDefault, double Max, double Min, int decimalPlace = 0)
        {
            item.DropDown.Closing -= DropDown_Closing;
            item.DropDown.Closing += DropDown_Closing;

            CreateTextLabel(item, itemName, $"Value from {Min} to {Max}");

            GH_DigitScroller slider = new GH_DigitScroller
            {
                MinimumValue = (decimal)Min,
                MaximumValue = (decimal)Max,
                DecimalPlaces = decimalPlace,
                Value = (decimal)originValue,
                Size = new Size(150, 24),
            };
            slider.ValueChanged += Slider_ValueChanged;

            void Slider_ValueChanged(object sender, GH_DigitScrollerEventArgs e)
            {
                double result = (double)e.Value;
                result = result >= Min ? result : Min;
                result = result <= Max ? result : Max;
                slider.Value = (decimal)result;

                valueChange.Invoke(result);

            }

            GH_DocumentObject.Menu_AppendCustomItem(item.DropDown, slider);

            //Add a Reset Item.
            ToolStripMenuItem resetItem = new ToolStripMenuItem("Reset Value", Properties.Resources.ResetIcons_24);
            resetItem.Click += (sender, e) =>
            {
                slider.Value = (decimal)valueDefault;
                valueChange.Invoke(valueDefault);
            };
            item.DropDownItems.Add(resetItem);
        }

        private static ToolStripLabel CreateTextLabel(ToolStripMenuItem item, string name, string tooltips = null)
        {
            ToolStripLabel textBox = new ToolStripLabel(name);
            textBox.TextAlign = ContentAlignment.MiddleCenter;
            textBox.Font = new Font(textBox.Font, FontStyle.Bold);
            if (!string.IsNullOrEmpty(tooltips))
                textBox.ToolTipText = tooltips;
            item.DropDownItems.Add(textBox);
            return textBox;
        }

        private static void DropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        }
    }
}
