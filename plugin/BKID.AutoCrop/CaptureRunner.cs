using System;
using System.Globalization;

namespace BKID.AutoCrop
{
    // Explicit per-call argument; never rewrite the capture module's defaults.
    internal static class CaptureRunner
    {
        internal static string Build(int scale)
        {
            if (scale < 2 || scale > 4) throw new ArgumentOutOfRangeException("scale");
            return "# -*- coding: utf-8 -*-\n" +
                   "import os\nimport runpy\nimport traceback\n" +
                   "try:\n" +
                   "    scope = runpy.run_path(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'BKID_AutoCrop.py'))\n" +
                   "    scope['run'](capture_scale=" + scale.ToString(CultureInfo.InvariantCulture) + ")\n" +
                   "except Exception:\n" +
                   "    import Rhino\n" +
                   "    Rhino.RhinoApp.WriteLine(traceback.format_exc())\n";
        }
    }
}
