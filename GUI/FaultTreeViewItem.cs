using System.Collections.Generic;
using System.Windows.Controls;

using FiSim;

namespace FiSim.GUI {
    internal class FaultTreeViewItem : TreeViewItem {
        public ulong FaultAddress { get; set; }

        public string FaultDescription { get; internal set; }

        public List<IFaultDefinition> Faults { get; } = new List<IFaultDefinition>();
    }
}