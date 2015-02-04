using System.Windows;
using System.Windows.Media;
using KinectTool.Drawing.Components;
using KinectTool.Model.Kinect;
using Microsoft.Kinect;

namespace KinectTool.Drawing
{
    /// <summary>
    /// Visualize the joints of a kinect Skeleton in 2D
    /// Taken from Kinect Sample
    /// </summary>
    public class KinectJointVisual : IFrameVisual
    {
        //Frame that will be drawn
        private KinectFrame _kinectFrame;
        private readonly DrawingGroup _drawGroup;

        private readonly Brush _trackedJointBrush = new SolidColorBrush(Color.FromArgb(220, 68, 174, 68));
        private readonly Brush _selectedJointBrush = new SolidColorBrush(Color.FromArgb(255, 174, 255, 68));

        /// Brush used for drawing joints that are currently inferred
        private readonly Brush _inferredJointBrush = Brushes.OrangeRed;

        /// Pen used for drawing bones that are currently tracked
        private readonly Pen _trackedBonePen = new Pen(Brushes.Green, 4);
        

        /// Pen used for drawing bones that are currently inferred
        private readonly Pen _inferredBonePen = new Pen(Brushes.LightGoldenrodYellow, 2);

        /// Thickness of drawn joint lines
        private const double JOINT_THICKNESS = 2;
        public KinectJointVisual(DrawingGroup dwGroup)
        {
            _drawGroup = dwGroup;
        }

        public void SetFrame(KinectFrame frame) 
        {
            _kinectFrame = frame;
        }

        public void Draw()
        {
            using (var dc = _drawGroup.Open())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 17, 17, 17)), null, new Rect(0, 0, 640, 480));
                DrawBones(dc);
                DrawJoints(dc);
            }
        }
        private void DrawBones(DrawingContext drawingContext)
        {
            foreach (var bone in _kinectFrame.Bones)
            {
                DrawBone(drawingContext,bone);
            }
           
        }

        private void DrawJoints(DrawingContext dwc)
        {
            //Visualize the joint 
            foreach (var joint in _kinectFrame.Joints)
            {
                Brush drawBrush = null;

                if (joint.Value.TrackState == JointTrackingState.Tracked)
                {
                    drawBrush = _trackedJointBrush;
                }
                else if (joint.Value.TrackState == JointTrackingState.Inferred)
                {
                    drawBrush = _inferredJointBrush;
                }
             
                if (drawBrush != null)
                {
                    dwc.DrawEllipse(drawBrush, null, new Point(joint.Value.Position2D.X,joint.Value.Position2D.Y), JOINT_THICKNESS * 2, JOINT_THICKNESS * 2);
                }

                //dwc.DrawRectangle(new SolidColorBrush(Color.FromArgb(127,233,0,0)),null,joint.Value.HitBox );
            }
        }

        private void DrawBone(DrawingContext dwC, KinectBone bone)
        {
            var joint0 = bone.Joint0;
            var joint1 = bone.Joint1;

            //exit if joints aren't tracked
            if (joint0.TrackState != JointTrackingState.Tracked ||
                joint1.TrackState != JointTrackingState.Tracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (joint0.TrackState == JointTrackingState.Inferred &&
                joint1.TrackState == JointTrackingState.Inferred)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            var drawPen = _inferredBonePen;
            if (joint0.TrackState == JointTrackingState.Tracked && joint1.TrackState == JointTrackingState.Tracked)
            {
                drawPen = _trackedBonePen;
            }

            var point0 = joint0.Position2D;
            var point1 = joint1.Position2D;

            dwC.DrawLine(drawPen, new Point(point0.X, point0.Y), new Point(point1.X, point1.Y));
        }
    }
}
