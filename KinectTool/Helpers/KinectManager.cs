using System;
using System.Linq;
using Debugify;
using Microsoft.Kinect;

namespace KinectTool.Helpers
{
    /// <summary>
    /// Handles initialization and usage of the kinect
    /// </summary>
    public static  class KinectManager
    {
        //Task done every time new kinectData is ready
        public delegate void KinectTaskDelegate(SkeletonFrameReadyEventArgs e);
        
        /// <summary>
        /// The actual sensor for gettting data
        /// </summary>
        private static KinectSensor _Sensor;
        public static KinectSensor Instance
        {
            get { return _Sensor; }
            private set { _Sensor = value; }
        }

        //Recording
        /// Emulate or Record
        public static event KinectTaskDelegate KinectTask;
        
        public static void StartKinect()
        {
            FindActiveSensor();
        }

        public static void StopKinect()
        {
            if (_Sensor == null)
            {
                Debug.Log(LogLevel.Info, "No kinect detected");
                return;
            }
           
            _Sensor.Stop();
        }
        
        /// <summary>
        /// Look through sensors and start the first connectd one
        /// </summary>
        private static void FindActiveSensor()
        {
            if (_Sensor != null)
                Debug.Log(LogLevel.Warning, "Kinect already detected");

            //loop all available sensors
            foreach (var potentialSensor in KinectSensor.KinectSensors.Where(potentialSensor => potentialSensor.Status == KinectStatus.Connected))
            {
                _Sensor = potentialSensor;
                break; //out of for loop
            }

            //if a kinect is found, start it
            if (_Sensor != null)
            {
                //Enable the wanted streams
                _Sensor.SkeletonStream.Enable();
                //_Sensor.ColorStream.Enable();
                //_Sensor.DepthStream.Enable();

                //Invoke the onFrameReady Event
                _Sensor.SkeletonFrameReady += OnFrameReady;

                //It could happen the kinect is already in use in another process
                //fix: stop process manually in Task manager
                try
                {
                    _Sensor.Stop();
                    _Sensor.Start();
                    Debug.Log(LogLevel.Info, "Kinect has started");
                }
                catch (Exception)
                {
                    Debug.Log(LogLevel.Error, "Kinect already running in other process");
                }
            }
            else
            {
                Debug.Log(LogLevel.Info, "No Kinect is found");
            }
        }

        /// <summary>
        /// Called everytime new data from the kinect is available
        /// </summary>
        private static void OnFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            //Check if you want the kinect to do something each new frame
            if (KinectTask == null)
            {
                Debug.Log(LogLevel.Warning,"Kinect has no tasks available");
                return;
            }

            //do the desired task
            KinectTask(e);
        }
    }
}
