using Grasshopper;
using Grasshopper.GUI.Canvas;
using HarmonyLib;
using System.Drawing;

namespace WatermarkPainter;

[HarmonyPatch(typeof(GH_Painter))]
internal static class WPainter
{
    [HarmonyPatch("DrawCanvasFill")]
    [HarmonyPostfix]
    internal static void PaintWatermark(ref GH_Canvas ___m_canvas)
    {
        if(Data.DrawImage == null || Data.ImageRatio == 0) return;

        ___m_canvas.Graphics.DrawImage(Data.DrawImage, ___m_canvas.Viewport.UnprojectRectangle(GetControlRect(___m_canvas)),
            new RectangleF(0, 0, Data.DrawImage.Width, Data.DrawImage.Height), GraphicsUnit.Pixel);

        if (Data.HasFrameCount)
        {
            Instances.ActiveCanvas.ScheduleRegen(1000 / Data.GifFps);
        }
    }

    static RectangleF GetControlRect(GH_Canvas canvas)
    {
        var width = Data.DrawImage.Width * Data.ImageRatio;
        var height = Data.DrawImage.Height * Data.ImageRatio;

        var controlW = canvas.Width;
        var controlH = canvas.Height;

        return Data.Alignment switch
        {
            ContentAlignment.TopRight => new RectangleF(controlW - width, 0, width, height),
            ContentAlignment.TopCenter => new RectangleF((controlW - width) / 2, 0, width, height),
            ContentAlignment.BottomLeft => new RectangleF(0, controlH - height, width, height),
            ContentAlignment.BottomRight => new RectangleF(controlW - width, controlH - height, width, height),
            ContentAlignment.BottomCenter => new RectangleF((controlW - width) / 2, controlH - height, width, height),
            ContentAlignment.MiddleLeft => new RectangleF(0, (controlH - height) / 2, width, height),
            ContentAlignment.MiddleRight => new RectangleF(controlW - width, (controlH - height) / 2, width, height),
            ContentAlignment.MiddleCenter => new RectangleF(0, 0, controlW, controlH),
            _ => new RectangleF(0, 0, width, height),
        };
    }
}
