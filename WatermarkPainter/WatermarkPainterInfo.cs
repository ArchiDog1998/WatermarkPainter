using Grasshopper.GUI;
using Grasshopper.Kernel;
using HarmonyLib;
using SimpleGrasshopper.Util;
using System;
using System.Drawing;

namespace WatermarkPainter;

public class WatermarkPainterInfo : GH_AssemblyInfo
{
    public override string Name => "WatermarkPainter";

    //Return a 24x24 pixel bitmap to represent this GHA library.
    public override Bitmap Icon => typeof(WatermarkPainterInfo).Assembly.GetBitmap("WatermarkIcon128.png");

    //Return a short string describing the purpose of this GHA library.
    public override string Description => "Add the watermark to your gh canvas.";

    public override Guid Id => new ("f4a5f9af-a365-4ba0-86cb-cf7bdf4d2dd6");

    //Return a string identifying you or your company.
    public override string AuthorName => "秋水";

    //Return a string representing your preferred contact details.
    public override string AuthorContact => "1123993881@qq.com";
}

partial class SimpleAssemblyPriority
{
    protected override void DoWithEditor(GH_DocumentEditor editor)
    {
        var harmony = new Harmony("grasshopper.WatermarkPainter.patch");
        harmony.PatchAll();

        Data.ImagePath = Data.ImagePath;
        base.DoWithEditor(editor);
    }
}