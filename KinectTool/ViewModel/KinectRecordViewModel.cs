using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using Debugify;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using KinectTool.Drawing;
using KinectTool.Helpers;
using KinectTool.Model.Kinect;
using Microsoft.Kinect;

namespace KinectTool.ViewModel
{
    
    /// <summary>
    /// Records the person staning infront of the kinect and saves it to a serialized file
    /// </summary>
    public class KinectRecordViewModel : ViewModelBase
    {
       /*COMMANDS*/

        private RelayCommand _startRecordingCommand;
        public RelayCommand StartRecordingCommand
        {
            get
            {
                return _startRecordingCommand ??
                       (_startRecordingCommand = new RelayCommand(StartRecording));
            }
        }

        private RelayCommand _stopRecordingCommand;
        public RelayCommand StopRecordingCommand
        {
            get
            {
                return _stopRecordingCommand ??
                       (_stopRecordingCommand = new RelayCommand(StopRecording));
            }
        }

        /*BINDINGS*/
        public DrawingImage KinectDrawingImage { get; set; }

        /// <summary>
        /// Are we Recording
        /// </summary>
        public bool IsRecording { get; set; }

        /// <summary>
        /// Name of the recording
        /// </summary>
        public string FileName { get; set; }

        //Visuallizing the frames we get from the kinectSensor
        private readonly FrameVisualizer _frameDrawer;

        //the recording you want to save
        private KinectRecording _recording;
        private const string FOLDER = "Recordings";
        private int _recordCount = 0;

        public KinectRecordViewModel()
        {
            //Set up the place to draw
            var dwGroup = new DrawingGroup();
            KinectDrawingImage = new DrawingImage(dwGroup);

            
            _frameDrawer = new FrameVisualizer(dwGroup);
            _frameDrawer.DrawMode = DrawMode.Visual2D;
            FileName = "Recording";

            //Create the recorder and hook up the actual Record function to be called whenever new data is available
            KinectManager.KinectTask += PollSkeleton;

            //Start, will not do anything when no kinect is detected
            KinectManager.StartKinect();
        }

        /// <summary>
        /// Hook up to the Kinectmanager to get data from the kinectSensor
        /// </summary>
        public void PollSkeleton(SkeletonFrameReadyEventArgs e)
        {
            //Get data from the current Frame
            using (var sensorData = e.OpenSkeletonFrame())
            {
                if (sensorData == null)
                {
                    Debug.Log(LogLevel.Error, "No sensor data available");
                    return;
                }

                //Copy data from the skeleton
                var skeletons = new Skeleton[sensorData.SkeletonArrayLength];
                sensorData.CopySkeletonDataTo(skeletons);

                //Find the tracked skeleton and return the current frame
                foreach (var frame in from skeleton in skeletons where skeleton.TrackingState == SkeletonTrackingState.Tracked select new KinectFrame(skeleton))
                {
                    //record the frame, filter out the bad frames in the recording itself
                    if (IsRecording)
                    {
                        Record(frame);
                        Debug.Log(LogLevel.Info, "Recording");
                    }
                    _frameDrawer.Draw(frame);
                }
            }
        }

        public void StartRecording()
        {
            IsRecording = true;
            Debug.Log(LogLevel.Info, "Started recording Kinect input data");
            _recording = new KinectRecording(FileName);
        }

        public void StopRecording()
        {
            IsRecording = false;
            SaveRecording(FileName);
            Debug.Log(LogLevel.Info, "Stopped recording Kinect input data");
        }

        public void Record(KinectFrame currFrame)
        {
            _recording.AddFrame(currFrame);
        }

        private void SaveRecording(string outputFile)
        {
            if (_recording == null)
                return;

            //Create directory if it doesn't exist
            if (!Directory.Exists(FOLDER))
                Directory.CreateDirectory(FOLDER);

            var filePath = FOLDER + "/" + outputFile + _recordCount + ".kr";

            //Don't overwrite previous recordings
            while (File.Exists(filePath))
            {
                _recordCount++;
                filePath = FOLDER + "/" + outputFile + _recordCount + ".kr";
            }

            //open the filestream
            using (var output = new FileStream(@filePath, FileMode.Create))
            {
                //Open binartFormatter
                var bf = new BinaryFormatter();
                bf.Serialize(output, _recording);
                _recording = null;
                _recordCount++;
            }
        }
    }
}