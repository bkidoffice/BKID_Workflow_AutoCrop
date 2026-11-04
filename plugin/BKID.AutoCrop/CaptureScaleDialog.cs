using Eto.Drawing;
using Eto.Forms;

namespace BKID.AutoCrop
{
    internal sealed class CaptureScaleDialog : Dialog<int>
    {
        internal CaptureScaleDialog(int viewportWidth, int viewportHeight)
        {
            Title = "AutoCrop 해상도 선택";
            Resizable = false;
            Padding = new Padding(16);
            var layout = new DynamicLayout { Spacing = new Size(8, 8) };
            layout.AddRow(new Label { Text = "캡처 배율을 선택하세요." });
            foreach (int factor in new[] { 2, 3, 4 })
            {
                int selected = factor;
                var button = new Button
                {
                    Text = string.Format("×{0}  ({1} × {2} px)", factor,
                                         (long)viewportWidth * factor, (long)viewportHeight * factor),
                    Height = 36
                };
                button.Click += (sender, args) => Close(selected);
                layout.AddRow(button);
            }
            layout.AddRow(new Label { Text = "선택 즉시 저장합니다. 최종 크기는 자동 크롭에 따라 달라집니다." });
            var cancel = new Button { Text = "취소" };
            cancel.Click += (sender, args) => Close(0);
            AbortButton = cancel;
            layout.AddRow(null, cancel);
            Content = layout;
        }
    }
}
