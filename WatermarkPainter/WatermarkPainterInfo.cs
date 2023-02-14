using Eto.Forms;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace WatermarkPainter
{
    public class WatermarkPainterInfo : GH_AssemblyInfo
    {
        public override string Name => "WatermarkPainter";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "Add the watermark to your gh canvas.";

        public override Guid Id => new Guid("f4a5f9af-a365-4ba0-86cb-cf7bdf4d2dd6");

        //Return a string identifying you or your company.
        public override string AuthorName => "秋水";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "1123993881@qq.com";
    }

    public class WatermarkPainterAssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            Instances.CanvasCreated += Instances_CanvasCreated;
            return GH_LoadingInstruction.Proceed;
        }

        void Instances_CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= Instances_CanvasCreated;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                Instances.ActiveCanvas.DocumentChanged += ActiveCanvas_DocumentChanged;
                return;
            }
            DoingSomethingFirst(editor);
        }

        void ActiveCanvas_DocumentChanged(GH_Canvas sender, GH_CanvasDocumentChangedEventArgs e)
        {
            Instances.ActiveCanvas.DocumentChanged -= ActiveCanvas_DocumentChanged;

            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                MessageBox.Show("Watermark Painter can't find the menu!");
                return;
            }
            DoingSomethingFirst(editor);
        }

        void DoingSomethingFirst(GH_DocumentEditor editor)
        {
            WPainter.Init();
            MenuCreater.CreateMenu(editor);
        }
    }
}