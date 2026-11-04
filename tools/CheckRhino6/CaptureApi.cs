using Rhino;
using Rhino.Display;
internal static class CaptureApi
{
 internal static void Check(RhinoDoc doc, RhinoView view)
 {
  var plane = view.ActiveViewport.GetConstructionPlane();
  plane.ShowZAxis = false;
  view.ActiveViewport.SetConstructionPlane(plane);
  var capture = new ViewCapture();
  capture.TransparentBackground = true;
  capture.ScaleScreenItems = false;
  capture.DrawAxes = false;
  capture.DrawGrid = false;
  capture.DrawGridAxes = false;
  capture.Width = view.ActiveViewport.Size.Width;
  capture.Height = view.ActiveViewport.Size.Height;
  using (var bitmap = capture.CaptureToBitmap(view)) { }
  foreach (var obj in doc.Objects.GetSelectedObjects(false, false))
  {
   int selected = obj.IsSelected(false);
   var parts = obj.GetSelectedSubObjects();
   if(parts != null) foreach(var part in parts) obj.SelectSubObject(part, false, true);
   obj.Select(selected != 0);
  }
 }
}
