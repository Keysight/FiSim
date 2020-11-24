using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using FiSim.GUI.UI.Windows;

namespace FiSim.GUI.UI.Views {
    public class FileTreeBrowserView : TreeView {
        internal void Init(MainWindow mainWindow, string rootFolder) {
            if (!Directory.Exists(rootFolder)) {
                MessageBox.Show($"Critical error: Root folder \"{rootFolder}\" missing", "BootSim");
                Environment.Exit(0);
            }
            
            _loadSubDirsAndFiles(mainWindow, Items, rootFolder);
        }

        void _loadSubDirsAndFiles(MainWindow mainWindow, ItemCollection items, string folder) {
            foreach (var dir in Directory.EnumerateDirectories(folder)) {
                bool isHidden = false;
                
                foreach (var hiddenFile in Config.HiddenFiles) {
                    if (dir.EndsWith(hiddenFile)) {
                        isHidden = true;
                        break;
                    }
                }

                if (!isHidden) {
                    var newItem = new FileTreeBrowserViewDirectoryItem {Header = dir.Replace(folder, "").Substring(1)};

                    _loadSubDirsAndFiles(mainWindow, newItem.Items, dir);

                    items.Add(newItem);
                }
            }

            foreach (var file in Directory.EnumerateFiles(folder)) {
                bool isHidden = false;
                
                foreach (var hiddenFile in Config.HiddenFiles) {
                    if (file.EndsWith(hiddenFile)) {
                        isHidden = true;
                        break;
                    }
                }

                if (!isHidden) {
                    var newItem = new FileTreeBrowserViewFileItem {Header = file.Replace(folder, "").Substring(1)};

                    //if (file.EndsWith(".c") || file.EndsWith(".h") || file.EndsWith(".S") || file.EndsWith(".sh") || file.EndsWith(".lds") || file.EndsWith(".map")) {
                    if (!file.EndsWith(".bin") && !file.EndsWith(".elf") && !file.EndsWith(".o") && !file.EndsWith(".idb")) {
                        newItem.MouseDoubleClick += (sender, e) => {
                            mainWindow.OpenFileTab((string) newItem.Header, new FileInfo(file));
                        };
                    }

                    items.Add(newItem);
                }
            }
        }
    }

    public class FileTreeBrowserViewDirectoryItem : TreeViewItem {
    }

    public class FileTreeBrowserViewFileItem : TreeViewItem {
    }
}