using System.Windows.Controls;
using GBCLV2.Modules;

namespace GBCLV2.Controls
{
    public partial class OtherSettings : Grid
    {
        public OtherSettings()
        {
            InitializeComponent();
            this.DataContext = Config.Args;
        }
    }
}
