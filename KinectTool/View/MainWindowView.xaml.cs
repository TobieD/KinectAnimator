using System.ComponentModel;
using System.Windows;
using Debugify;
using Meshy;

namespace KinectTool.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView :Window

    {
        public MainWindowView()
        {
            //Hook up tasks for the debugger
            Debug.OnLog += DebugTask.SimpleLog;
            Debug.OnSave += DebugTask.SaveToFile;
            Dispatcher.UnhandledException += DebugTask.OnCrash; //on crash event
            Debug.Log(LogLevel.Info,"DebugLogger set");

            //Set up Importer
            Importer.SetupImporters();
            Exporter.SetupExporters();
            
            InitializeComponent();
        }

        private void WindowClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            //save the logged history to a file
            #if(DEBUG)
            Debug.Save();
            #endif
        }
    }
}
