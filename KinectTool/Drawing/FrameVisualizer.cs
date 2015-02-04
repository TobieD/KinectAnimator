using System;
using System.ComponentModel;
using System.Windows.Media;
using Debugify;
using KinectTool.Drawing.Components;
using KinectTool.Model.Kinect;

namespace KinectTool.Drawing
{
    public enum DrawMode
    {
        None = 0,
        [Description("2D")]
        Visual2D = 1,
        [Description("3D")]
        Visual3D = 2
    }

    /// <summary>
    /// Handles drawing of a Fame
    /// </summary>
    public class FrameVisualizer
    {
        /// <summary>
        /// How we want to visualize the frame
        /// </summary>
        public DrawMode DrawMode { get; set; }
        private IFrameVisual _drawInstance; //what we use to draw


        public FrameVisualizer(DrawingGroup drawingGroup)
        {
            DrawMode = DrawMode.Visual2D;

            _drawInstance = new KinectJointVisual(drawingGroup);
        }

        /// <summary>
        /// Draws the frame given
        /// </summary>
        public void Draw(KinectFrame frameToDraw)
        {
            if (frameToDraw == null)
            {
                Debug.Log(LogLevel.Warning,"No Frame Available!");
                return;
            }

            if (_drawInstance == null)
               return;

            _drawInstance.SetFrame(frameToDraw);
            _drawInstance.Draw();
            //_drawGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, 640, 480 ));    
    

        }
    }
}
