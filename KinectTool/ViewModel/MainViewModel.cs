using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KinectTool.Redux;
using KinectTool.View;

namespace KinectTool.ViewModel
{
    public class MainViewModel:ViewModelBase
    {
        //Called when the Record button is pressed
        public RelayCommand OpenRecordCommand { get; private set; }

        public RelayCommand UndoCommand { get; set; }
        public RelayCommand RedoCommand { get; set; }

        public RelayCommand EnableExpertMode { get; set; }

        private bool _useImages = false;
        public bool UseImageForButtons {
            get { return _useImages; }
            set
            {
                _useImages = value;
                RaisePropertyChanged();
            } 
        }

        public MainViewModel()
        {
            OpenRecordCommand = new RelayCommand(OpenRecordWindow);

            UndoCommand = new RelayCommand(ReduxManager.Undo);
            RedoCommand = new RelayCommand(ReduxManager.Redo);
            EnableExpertMode = new RelayCommand(()=> UseImageForButtons = !UseImageForButtons);
        }


        //Opens the record window
        private void OpenRecordWindow()
        {
            var recordWindow = new KinectRecordingView();
            recordWindow.ShowDialog();
        }


        
    }
}
