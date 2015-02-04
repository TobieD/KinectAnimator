using System;
using KinectTool.Drawing.Components;
using KinectTool.Drawing.Effect;
using KinectTool.Drawing.Prefabs;
using SharpDX;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpWpf;
using Device1 = SharpDX.Direct3D10.Device1;

namespace KinectTool.Drawing
{
    public class Dx10Viewport : IDX10Viewport
    {
        private Device1 _device;
        private RenderTargetView _renderTargetView;
        private DX10RenderCanvas _renderControl;

        //ViewPort needs all of this

        //Shader
        public IEffect Shader { get; set; }

        //Camera
        public ICamera Camera { get; set; }

        //Meshes
        private CustomMesh _Mesh;

        public OnDrop OnDrop { get; set; }
        public Action OnLoad { get; set; }


        public void Initialize(Device1 device, RenderTargetView renderTarget, DX10RenderCanvas canvasControl)
        {
            _device = device;
            _renderTargetView = renderTarget;
            _renderControl = canvasControl;
            _renderControl.ClearColor = Color.CornflowerBlue; 
            _renderControl.OnDrop += OnDrop; 

            //Set Shader (IEffect)
            Shader = new PosColorNormSkinnedEffect();
            Shader.Create(device);

            Camera = new BaseCamera();
            Camera = new FlyingCamera();
            
            if (OnLoad != null)
                OnLoad();
        }
        
        public void Deinitialize()
        {

        }

        public void Update(float deltaT)
        {
            if ( Shader == null || _Mesh == null) return;

            if(!_Mesh.IsLoaded)
                return;

            _Mesh.Update();

            Camera.Update();
            Camera.AspectRatio = (float) _renderControl.ActualWidth/(float) _renderControl.ActualHeight;
            
            //Camera.TargetPosition = _Mesh.Position;
            Shader.SetWorld(_Mesh.World);
            Shader.SetWorldViewProjection(_Mesh.World * Camera.View * Camera.Projection);

            if (!_Mesh.Animated) return;

            var skinnedShader = Shader as PosColorNormSkinnedEffect;
            skinnedShader.SetBoneTransforms(_Mesh.Animator.Transforms);
            _Mesh.Animator.Animate();
        }

        public void Render(float deltaT)
        {
            if (_device == null || Shader == null || _Mesh == null) return;

            if (!_Mesh.IsLoaded)
                    return; ;

            _device.InputAssembler.InputLayout = Shader.InputLayout;
            _device.InputAssembler.PrimitiveTopology = _Mesh.PrimitiveTopology;
            _device.InputAssembler.SetIndexBuffer(_Mesh.IndexBuffer, Format.R32_UInt, 0);
            _device.InputAssembler.SetVertexBuffers(0,
            new VertexBufferBinding(_Mesh.VertexBuffer, _Mesh.VertexStride, 0));

            for (var i = 0; i < Shader.Technique.Description.PassCount; ++i)
            {
                Shader.Technique.GetPassByIndex(i).Apply();
                _device.DrawIndexed(_Mesh.MeshData.IndexCount, 0, 0);
            }
        }

        public void AddMesh(CustomMesh mesh)
        {
            if(!mesh.IsCreated)
                mesh.Create(_device);

            _Mesh = mesh;

            //Change Shader based on model type
            Shader = (_Mesh.Animated) ? (IEffect)new PosColorNormSkinnedEffect() : new PosColorNormEffect();
            Shader.Create(_device);
        }

        
    }
}
