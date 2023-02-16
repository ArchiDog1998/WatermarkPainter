using Grasshopper;
using Grasshopper.GUI.Canvas;
using HarmonyLib;
using Rhino.Geometry;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;

namespace WatermarkPainter
{
    [HarmonyPatch(typeof(GH_Painter))]
    internal static class WPainter
    {
        const string PLUGINNAME = "WatermarkPainter.";
        static DateTime startTime = DateTime.Now;

        public const float ImageRatioDefault = 1f;
        public static float ImageRatio
        {
            get => (float)Instances.Settings.GetValue(PLUGINNAME + nameof(ImageRatio), ImageRatioDefault);
            set
            {
                Instances.Settings.SetValue(PLUGINNAME + nameof(ImageRatio), value);
                Refresh();
            }
        }

        public static string ImagePath
        {
            get => Instances.Settings.GetValue(PLUGINNAME + nameof(ImagePath), string.Empty);
            set
            {
                if(ImagePath == value) return;
                LoadImage(value);
                Instances.Settings.SetValue(PLUGINNAME + nameof(ImagePath), value);
                Refresh();
            }
        }

        public static ContentAlignment Alignment 
        {
            get => (ContentAlignment)Instances.Settings.GetValue(PLUGINNAME + nameof(Alignment), (int)ContentAlignment.BottomRight);
            set
            {
                Instances.Settings.SetValue(PLUGINNAME + nameof(Alignment), (int)value);
                Refresh();
            }
        }

        public const int GifFpsDefault = 15;
        public static int GifFps
        {
            get => Instances.Settings.GetValue(PLUGINNAME + nameof(GifFps), 15);
            set => Instances.Settings.SetValue(PLUGINNAME + nameof(GifFps), value);
        }

        static int _frameCount;
        static Image _readImage;
        public static Image DrawImage 
        {
            get
            {
                if(_frameCount > 1)
                {
                    var i = (int)((DateTime.Now - startTime).TotalSeconds * GifFps) % _frameCount;
                    _readImage.SelectActiveFrame(FrameDimension.Time, i);
                }
                return _readImage;
            }
        }

        static Timer _refresher;
        internal static void Init()
        {
            new Harmony("grasshopper.WatermarkPainter.patch").PatchAll();
            LoadImage(ImagePath);
        }


        private static void LoadImage(string path)
        {
            if (File.Exists(path))
            {
                _readImage = Image.FromFile(path);
                _frameCount = _readImage.GetFrameCount(new FrameDimension(_readImage.FrameDimensionsList[0]));
            }
        }

        [HarmonyPatch("DrawCanvasFill")]
        [HarmonyPostfix]
        internal static void PaintWatermark(ref GH_Canvas ___m_canvas)
        {
            if(DrawImage == null || ImageRatio == 0) return;

            ___m_canvas.Graphics.DrawImage(DrawImage, ___m_canvas.Viewport.UnprojectRectangle(GetControlRect(___m_canvas)),
                new RectangleF(0, 0, DrawImage.Width, DrawImage.Height), GraphicsUnit.Pixel);

            if(_frameCount > 1)
            {
                Instances.ActiveCanvas.ScheduleRegen(1000 / GifFps);
            }
        }

        static RectangleF GetControlRect(GH_Canvas canvas)
        {
            var width = DrawImage.Width * ImageRatio;
            var height = DrawImage.Height * ImageRatio;

            var controlW = canvas.Width;
            var controlH = canvas.Height;

            switch (Alignment)
            {
                default:
                case ContentAlignment.TopLeft:
                    return new RectangleF(0, 0, width, height);

                case ContentAlignment.TopRight:
                    return new RectangleF(controlW - width, 0, width, height);

                case ContentAlignment.TopCenter:
                    return new RectangleF((controlW - width) / 2, 0, width, height);

                case ContentAlignment.BottomLeft:
                    return new RectangleF(0, controlH - height, width, height);

                case ContentAlignment.BottomRight:
                    return new RectangleF(controlW - width, controlH - height, width, height);

                case ContentAlignment.BottomCenter:
                    return new RectangleF((controlW - width) / 2, controlH - height, width, height);

                case ContentAlignment.MiddleLeft:
                    return new RectangleF(0, (controlH - height) / 2, width, height);

                case ContentAlignment.MiddleRight:
                    return new RectangleF(controlW - width, (controlH - height) / 2, width, height);

                case ContentAlignment.MiddleCenter:
                    return new RectangleF(0, 0, controlW, controlH);
            }
        }

        static void Refresh()
        {
            Instances.ActiveCanvas.ScheduleRegen(10);
        }
    }
}
