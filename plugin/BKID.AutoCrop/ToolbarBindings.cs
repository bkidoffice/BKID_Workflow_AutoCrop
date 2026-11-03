using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace BKID.AutoCrop
{
    internal static class ToolbarBindings
    {
        private static XDocument Read(string text)
        {
            using (var input = new StringReader(text))
            using (var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                return XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        }
        private static bool IsCommand(XElement macro, string command)
        {
            var script = (string)macro.Element("script") ?? "";
            return string.Equals(script.Trim().TrimStart('!').Trim().TrimStart('_'), command, StringComparison.OrdinalIgnoreCase);
        }

        internal static Guid CollectionId(string template)
        {
            return new Guid((string)Read(template).Root.Attribute("guid"));
        }

        internal static bool ContainsCaptureButton(string current, string template)
        {
            var document = Read(current);
            var source = Read(template);
            var ids = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ids.Add((string)source.Root.Element("macros").Elements("macro_item")
                .Single(m => IsCommand(m, "BKIDAutoCropCapture")).Attribute("guid"));
            var macros = document.Root.Element("macros");
            if (macros != null)
                foreach (var macro in macros.Elements("macro_item"))
                    if (macro.Attribute("guid") != null && IsCommand(macro, "BKIDAutoCropCapture")) ids.Add((string)macro.Attribute("guid"));
            return document.Descendants("tool_bar_item").Any(button => button.Element("left_macro_id") != null && ids.Contains((string)button.Element("left_macro_id")));
        }

        private static bool UpdateDescriptions(XElement target, XElement source)
        {
            bool changed = false;
            foreach (string name in new[] { "tooltip", "help_text" })
            {
                var expected = source.Element(name);
                if (expected == null) continue;
                var current = target.Element(name);
                if (current != null && current.Elements().Select(e => e.Name + "=" + e.Value)
                    .SequenceEqual(expected.Elements().Select(e => e.Name + "=" + e.Value))) continue;
                if (current == null) target.Add(new XElement(expected));
                else current.ReplaceWith(new XElement(expected));
                changed = true;
            }
            return changed;
        }

        // Also recognizes buttons copied to a different collection with newly assigned IDs.
        internal static string Patch(string current, string template)
        {
            var document = Read(current);
            var source = Read(template);
            var sourceMacros = source.Root.Element("macros");
            var left = sourceMacros.Elements("macro_item").Single(m => IsCommand(m, "BKIDAutoCropCapture"));
            var right = sourceMacros.Elements("macro_item").Single(m => IsCommand(m, "BKIDAutoCropResolution"));
            var macros = document.Root.Element("macros") ?? new XElement("macros");
            var lookup = new System.Collections.Generic.Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in macros.Elements("macro_item").Where(m => m.Attribute("guid") != null))
                lookup[(string)item.Attribute("guid")] = item;
            bool changed = false;
            bool needsRightMacro = false;
            foreach (var button in document.Descendants("tool_bar_item"))
            {
                string leftId = (string)button.Element("left_macro_id");
                if (leftId == null) continue;
                XElement localLeft;
                lookup.TryGetValue(leftId, out localLeft);
                if (!string.Equals(leftId, (string)left.Attribute("guid"), StringComparison.OrdinalIgnoreCase) &&
                    (localLeft == null || !IsCommand(localLeft, "BKIDAutoCropCapture"))) continue;
                if (localLeft == null)
                {
                    localLeft = new XElement(left);
                    localLeft.SetAttributeValue("guid", leftId);
                    macros.Add(localLeft);
                    lookup[leftId] = localLeft;
                    changed = true;
                }
                changed = UpdateDescriptions(localLeft, left) || changed;
                string rightId = (string)button.Element("right_macro_id");
                XElement localRight;
                if (rightId != null && lookup.TryGetValue(rightId, out localRight) && IsCommand(localRight, "BKIDAutoCropResolution"))
                {
                    changed = UpdateDescriptions(localRight, right) || changed;
                    continue;
                }
                button.SetElementValue("right_macro_id", (string)right.Attribute("guid"));
                changed = true;
                needsRightMacro = true;
            }
            if (!changed) return null;
            if (macros.Parent == null)
            {
                var following = document.Root.Element("bitmaps") ?? document.Root.Element("scripts");
                if (following != null) following.AddBeforeSelf(macros);
                else document.Root.Add(macros);
            }
            if (needsRightMacro)
            {
                XElement previous;
                if (lookup.TryGetValue((string)right.Attribute("guid"), out previous)) previous.ReplaceWith(new XElement(right));
                else macros.Add(new XElement(right));
            }
            return document.ToString(SaveOptions.DisableFormatting);
        }
    }
}
