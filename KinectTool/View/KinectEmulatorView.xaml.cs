using System.Windows;
using System.Windows.Controls;
using KinectTool.ViewModel;

namespace KinectTool.View
{
    /// <summary>
    /// Interaction logic for KinectRecorderView.xaml
    /// </summary>
    public partial class KinectEmulatorView : UserControl
    {
        public KinectEmulatorView()
        {
            InitializeComponent();

            var dataContext = this.DataContext as ViewModelLocator;
            Dx10RenderCanvas.Viewport = dataContext.Emulator.Viewport;
            Dx10RenderCanvas.OnDrop = dataContext.Emulator.OnDrop;
            RenderCanvas.Drop += (sender, e) => dataContext.Emulator.OnDrop(e);
        }

        
    }
} 
