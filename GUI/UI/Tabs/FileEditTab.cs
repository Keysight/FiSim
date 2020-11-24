using System;
using System.IO;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;

namespace FiSim.GUI.UI.Tabs {
    public class FileEditTab : TabItem {
        public readonly FileInfo FileInfo;

        public readonly bool IsReadOnly;

        public readonly TextEditor TextEditor = new TextEditor();

        public bool IsModified => TextEditor.IsModified;

        public FileEditTab(TabControl tabControl, string tabName, FileInfo sourceFile, bool isReadOnly) {
            HeaderText = new TextBlock { Text = "test" };

            var closeButton = new Image {
                Source = new BitmapImage(new Uri("pack://application:,,,/FiSim.GUI;component/Images/Close.png")), Width = 10, Height = 10
            };

            closeButton.MouseDown += (sender, args) => {
                tabControl.Items.Remove(this);
            };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };

            stackPanel.Children.Add(HeaderText);
            stackPanel.Children.Add(new TextBlock {Width = 4});
            stackPanel.Children.Add(closeButton);

            Header = stackPanel;
            FileInfo = sourceFile;
            IsReadOnly = isReadOnly;

            _initLayout();
            
            if (sourceFile.Name.EndsWith(".c") || sourceFile.Name.EndsWith(".h"))
                TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C++");
            else if (sourceFile.Name.EndsWith(".py"))
                TextEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Python");

            TextEditor.Load(sourceFile.Open(FileMode.Open));
            
            var isModifiedWatcherThread = new Thread(() => {
                while (true) {
                    Dispatcher.Invoke(() => {
                        if (TextEditor.IsModified) {
                            HeaderText.Text = tabName + " *";
                        }
                        else {
                            HeaderText.Text = tabName;
                        }
                    });

                    Thread.Sleep(100);
                }
            }) { IsBackground = true };

            isModifiedWatcherThread.Start();
        }

        public TextBlock HeaderText { get; set; }

        public void Save() {
            if (IsModified) {
                File.WriteAllText(FileInfo.FullName, TextEditor.Document.Text);

                TextEditor.IsModified = false;
            }
        }

        private void _initLayout() {
            TextEditor.IsReadOnly = IsReadOnly;

            TextEditor.ShowLineNumbers = true;

            TextEditor.Options.EnableEmailHyperlinks = false;
            TextEditor.Options.EnableHyperlinks = false;

            TextEditor.GotFocus += delegate { TextEditor.Options.HighlightCurrentLine = true; };

            TextEditor.LostFocus += delegate { TextEditor.Options.HighlightCurrentLine = false; };

            SearchPanel.Install(TextEditor);

            Content = TextEditor;
        }
    }
}