using System;
using System.Collections.Generic;
using System.Linq;
using Debugify;
using KinectTool.Drawing.Prefabs;
using Meshy;
using SharpDX;

namespace KinectTool.Drawing.Animation
{
    /// <summary>
    /// Animates a set clip 
    /// </summary>
    public class Animator
    {
        #region Properties
        /// <summary>
        /// Are we playing the animation
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        ///speed at which the animation is playing
        /// </summary>
        public float AnimationSpeed { get; set; }

        /// <summary>
        ///Name of the current animation
        /// </summary>
        public string ClipName
        {
            get { return (_clipSet) ? CurrentClip.Name : "No Clip set"; }
        }

        public List<Matrix> Transforms { get; private set; }

        public AnimationClip CurrentClip { get; set; }

        #endregion

        #region fields
        
        private bool _clipSet = false;
        private CustomMesh _mesh;
        private float _tickCount;

        private DateTime _prevFrameTime;

        #endregion

        public Animator(CustomMesh mesh)
        {
            _mesh = mesh;
            Transforms = new List<Matrix>();
            AnimationSpeed = 1.0f;
        }

        public void SetAnimation(string name)
        {
            if (CurrentClip != null &&CurrentClip.Name == name)
                return;

            //search for the clip
            _clipSet = false;
            AnimationClip clip = null;
            var found = false;
            foreach (var animClip in _mesh.AnimationData.Animations.Where(animClip => animClip.Name == name))
            {
                clip = animClip;
                found = true;
            }

            if (found)
                SetAnimation(clip);
            else
                Debug.Log(LogLevel.Warning, string.Format("{0} Animation not found", name));
        }

        public void SetAnimation(AnimationClip c)
        {
            if (c == null)
            {
                _clipSet = false;
                return;
                
            }

            CurrentClip = c;

            //Debug.Log(LogLevel.Info,string.Format("{0}, transformations found",CurrentClip.Keys[0].BoneTransforms.Count));
            _clipSet = true;
        }

        /// <summary>
        /// Start the animation
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
        }

        /// <summary>
        /// Stop the animation
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Reset the currently playing clip
        /// </summary>
        public void Reset()
        {
            _tickCount = 0;
            AnimationSpeed = 1.0f;

            if (_clipSet && CurrentClip != null)
            {
                //first bone transform
                var transform = CurrentClip.Keys[0].BoneTransforms;
                Transforms.Clear();
                Transforms = (transform.ToList());
            }
            else
            {
                //fill with identity matrix
                Transforms.Clear();

                for (var i = 0; i < _mesh.AnimationData.BoneCount; ++i)
                    Transforms.Add(Matrix.Identity);
            }

        }

        /// <summary>
        /// Loop through all the keyframes
        /// </summary>
        public void Animate()
        {
            //Do nothing when not playing or no clip is set
            if (!IsPlaying && !_clipSet)
                return;

            var passedTime = (float)(DateTime.Now - _prevFrameTime).Milliseconds/1000;
            //Debug.Log(LogLevel.Info, string.Format("Time since Last Frame: {0}",passedTime));
          
            //Make sure that the passedTicks stay between the m_CurrentClip.Duration bounds
            double passedTicks = passedTime  * CurrentClip.TicksPerSecond * AnimationSpeed;
            passedTicks = Math.IEEERemainder(passedTicks, CurrentClip.Duration);

            //Debug.Log(LogLevel.Info, string.Format("Ticks: {0}", passedTicks));

            _tickCount += (float)passedTicks;

            //Reset when reaching the end
            if (_tickCount > CurrentClip.Duration)
                _tickCount -= CurrentClip.Duration;

            //Find the enclosing keys
            //Iterate all the keys of the clip and find the following keys:
		    //keyA > Closest Key with Tick before/smaller than m_TickCount
		    //keyB > Closest Key with Tick after/bigger than m_TickCount
            AnimationKey keyA = null, keyB = null;
            var prevKey = CurrentClip.Keys[0]; //start at first1
            var found = false;

            foreach (var k in CurrentClip.Keys)
            {
                if (!found && k.Tick > _tickCount && _tickCount > 0.0f && k.Tick != _tickCount)
                {
                    keyA = prevKey;
                    keyB = k;
                    found = true;
                }
                prevKey = k;
            }

            if(keyA == null || keyB == null)
                return;

            //lerp between keys
            var blendA = _tickCount - keyA.Tick;
            var blendB = 1.0f - blendA;
            blendB = _tickCount - blendB;
            var lerpAmount = blendA / blendB;

            Transforms.Clear();

            for (var i = 0; i < _mesh.AnimationData.BoneCount; ++i)
            {
                if(i >= keyA.BoneTransforms.Count ||i>= keyB.BoneTransforms.Count)
                   break;
               
                Transforms.Add(Matrix.Lerp(keyA.BoneTransforms[i],keyB.BoneTransforms[i],lerpAmount));
            }


            _prevFrameTime = DateTime.Now;
        }
    }
}
