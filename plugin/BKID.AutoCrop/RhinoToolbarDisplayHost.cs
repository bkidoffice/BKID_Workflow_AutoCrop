using System;
using System.Collections.Generic;
using System.IO;
using Rhino;
using Rhino.UI;

namespace BKID.AutoCrop
{
    internal sealed class RhinoToolbarDisplayHost : IToolbarDisplayHost
    {
        public IList<IToolbarCollectionView> Loaded
        {
            get
            {
                var result = new List<IToolbarCollectionView>();
                foreach (ToolbarFile file in RhinoApp.ToolbarFiles) result.Add(new Collection(file));
                return result;
            }
        }
        public IToolbarCollectionView Open(string path)
        {
            var file = RhinoApp.ToolbarFiles.Open(path);
            return file == null ? null : new Collection(file);
        }
        private sealed class Collection : IToolbarCollectionView
        {
            private readonly ToolbarFile file;
            internal Collection(ToolbarFile value) { file = value; }
            public Guid Id { get { return file.Id; } }
            public bool IsDedicated { get { return file.GroupCount == 1 && file.ToolbarCount == 1; } }
            public IList<IToolbarGroupView> Groups
            {
                get
                {
                    var result = new List<IToolbarGroupView>();
                    for (int i = 0; i < file.GroupCount; i++) result.Add(new Group(file.GetGroup(i)));
                    return result;
                }
            }
            public void SaveAndClose()
            {
                string path = file.Path;
                if (!file.Save()) throw new IOException("Cannot save duplicate AutoCrop toolbar: " + path);
                file.Close(false);
                if (RhinoApp.ToolbarFiles.FindByPath(path) != null)
                    throw new IOException("Cannot close duplicate AutoCrop toolbar: " + path);
            }
        }
        private sealed class Group : IToolbarGroupView
        {
            private readonly ToolbarGroup group;
            internal Group(ToolbarGroup value) { group = value; }
            public Guid Id { get { return group.Id; } }
            public bool Visible { get { return group.Visible; } set { group.Visible = value; } }
            public bool IsDocked { get { return group.IsDocked; } }
        }
    }
}
