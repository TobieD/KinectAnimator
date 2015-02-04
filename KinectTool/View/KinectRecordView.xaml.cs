using System.Windows;
using KinectTool.Helpers;

namespace KinectTool.View
{
    /// <summary>
    /// Interaction logic for KinectRecordingView.xaml
    /// </summary>
    public partial class KinectRecordingView : Window
    {
        public KinectRecordingView()
        {
            InitializeComponent();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KinectManager.StopKinect();
        }
       
    }
}
