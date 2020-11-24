using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

using static System.Reflection.Assembly;

namespace FiSim.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() {
            // Trigger self check
            var contentPath = Config.ContentPath;
            var engineExe = Config.EngineExe;
        }
    }
}