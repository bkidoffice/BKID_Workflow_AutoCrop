using System;
using System.Collections.Generic;
using System.Linq;

namespace BKID.AutoCrop
{
    internal interface IToolbarGroupView
    {
        Guid Id { get; }
        bool Visible { get; set; }
        bool IsDocked { get; }
    }
    internal interface IToolbarCollectionView
    {
        Guid Id { get; }
        IList<IToolbarGroupView> Groups { get; }
        bool IsDedicated { get; }
        void SaveAndClose();
    }
    internal interface IToolbarDisplayHost
    {
        IList<IToolbarCollectionView> Loaded { get; }
        IToolbarCollectionView Open(string path);
    }

    internal sealed class StartupGate
    {
        private bool entered;
        internal bool TryEnter()
        {
            if (entered) return false;
            entered = true;
            return true;
        }
        internal void Exit() { entered = false; }
    }

    internal static class ToolbarDisplay
    {
        internal static void Show(IToolbarDisplayHost host, string path, Guid fileId, Guid groupId, bool showIfHidden = true)
        {
            var loaded = host.Loaded;
            var candidates = loaded.SelectMany(file => file.Groups.Where(group => group.Id == groupId)
                .Select(group => new { File = file, Group = group }))
                .OrderByDescending(item => item.Group.IsDocked)
                .ThenByDescending(item => item.Group.Visible).ToList();
            IToolbarGroupView selected;
            if (candidates.Count > 0)
            {
                var keep = candidates[0];
                // Close only redundant, dedicated collections belonging to this plug-in.
                foreach (var extra in candidates.Skip(1))
                {
                    if (extra.File.Id == fileId && extra.File.IsDedicated && !ReferenceEquals(extra.File, keep.File))
                        extra.File.SaveAndClose();
                    else if (!ReferenceEquals(extra.File, keep.File) && extra.Group.Visible)
                        extra.Group.Visible = false; // Only this plug-in's group, not the shared collection.
                }
                selected = keep.Group;
            }
            else
            {
                // Rhino can auto-load a user UI copy under a different path.
                // An in-progress import must not be opened again by path.
                if (!showIfHidden) return;
                if (loaded.Any(file => file.Id == fileId))
                    throw new InvalidOperationException("AutoCrop toolbar import is not ready yet.");
                var opened = host.Open(path);
                if (opened == null) throw new InvalidOperationException("Rhino could not open the AutoCrop toolbar.");
                selected = opened.Groups.FirstOrDefault(group => group.Id == groupId);
                if (selected == null) throw new InvalidOperationException("AutoCrop toolbar group is not ready yet.");
            }
            // Some Rhino UI versions create another floating container on a repeated show.
            if (!selected.Visible && showIfHidden) selected.Visible = true;
            if (!selected.Visible && showIfHidden) throw new InvalidOperationException("Rhino did not display the AutoCrop toolbar.");
        }
    }
}
