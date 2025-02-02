﻿using SkiaSharp;
using SkiaSharp.Views.Tizen;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Tizen.NUI;
using NView = Tizen.NUI.BaseComponents.View;

namespace Tizen.UIExtensions.NUI
{
    /// <summary>
    /// A container view that clipping children view
    /// </summary>
    public class SKClipperView : NView
    {
        static readonly string VERTEX_SHADER =
            "attribute mediump vec2 aPosition;\n" +
            "varying mediump vec2 vTexCoord;\n" +
            "uniform highp mat4 uMvpMatrix;\n" +
            "uniform mediump vec3 uSize;\n" +
            "varying mediump vec2 sTexCoordRect;\n" +
            "void main()\n" +
            "{\n" +
            "   gl_Position = uMvpMatrix * vec4(aPosition * uSize.xy, 0.0, 1.0);\n" +
            "   vTexCoord = aPosition + vec2(0.5);\n" +
            "}\n";

        static readonly string FRAGMENT_SHADER = "" +
            "#extension GL_OES_EGL_image_external:require\n" +
            "uniform lowp vec4 uColor;\n" +
            "varying mediump vec2 vTexCoord;\n" +
            "uniform samplerExternalOES sTexture;\n" +
            "\n" +
            "void main(){\n" +
            "  mediump vec4 texColor = texture2D(sTexture, vTexCoord) * uColor;\n" +
            "  if (texColor.r < 1 || texColor.g < 1 || texColor.b < 1) discard;\n" +
            "  gl_FragColor = texColor;\n" +
            "}\n" +
            "";

        PropertyNotification _resized;
        Renderer _renderer;
        Geometry _geometry;
        Shader _shader;

        NativeImageSource? _buffer;
        Texture? _texture;
        TextureSet? _textureSet;

        int _bufferWidth = 0;
        int _bufferHeight = 0;
        int _bufferStride = 0;

        bool _redrawRequest;

        SynchronizationContext MainloopContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SKClipperView"/> class.
        /// </summary>
        public SKClipperView()
        {
            ClippingMode = ClippingModeType.ClipChildren;
            MainloopContext = SynchronizationContext.Current ?? throw new InvalidOperationException("Must create on main thread");
            _geometry = CreateQuadGeometry();
            _shader = new Shader(VERTEX_SHADER, FRAGMENT_SHADER);
            _resized = AddPropertyNotification("Size", PropertyCondition.Step(0.1f));
            _resized.Notified += OnResized;

            RemoveRenderer(0);

            _buffer = new NativeImageSource(1, 1, NativeImageSource.ColorDepth.Default);
            _texture = new Texture(_buffer);
            _textureSet = new TextureSet();
            _textureSet.SetTexture(0u, _texture);
            _renderer = new Renderer(_geometry, _shader);
            _renderer.SetTextures(_textureSet);
            AddRenderer(_renderer);

            OnResized();
        }

        /// <summary>
        /// Occurs when need to draw clipping area. A white area will be shown, others will be clipped
        /// </summary>
        public event EventHandler<SKPaintSurfaceEventArgs>? DrawClippingArea;

        /// <summary>
        /// Invalidate clipping area
        /// </summary>
        public void Invalidate()
        {
            if (!_redrawRequest)
            {
                _redrawRequest = true;
                MainloopContext.Post((s)=>
                {
                    _redrawRequest = false;
                    if (!Disposed && _buffer != null)
                    {
                        OnDrawFrame();
                    }
                }, null);
            }
        }

        protected void OnDrawFrame()
        {
            if (Size.Width == 0 || Size.Height == 0)
                return;

            UpdateSurface();

            var buffer = _buffer!.AcquireBuffer(ref _bufferWidth, ref _bufferHeight, ref _bufferStride);
            var info = new SKImageInfo(_bufferWidth, _bufferHeight);
            using (var surface = SKSurface.Create(info, buffer, _bufferStride))
            {
                // draw using SkiaSharp
                OnDrawFrame(new SKPaintSurfaceEventArgs(surface, info));
                surface.Canvas.Flush();
            }
            _buffer.ReleaseBuffer();

            UpdateBuffer();
        }

        void UpdateBuffer()
        {
            _texture?.Dispose();
            _textureSet?.Dispose();
            _texture = new Texture(_buffer);
            _textureSet = new TextureSet();
            _textureSet.SetTexture(0u, _texture);
            _renderer.SetTextures(_textureSet);
        }

        protected virtual void OnDrawFrame(SKPaintSurfaceEventArgs e)
        {
            DrawClippingArea?.Invoke(this, e);
        }

        protected virtual void OnResized()
        {
            if (Size.Width == 0 || Size.Height == 0)
                return;

            UpdateSurface();
            OnDrawFrame();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _buffer?.Dispose();
                _texture?.Dispose();
                _textureSet?.Dispose();
                _renderer?.Dispose();
            }
            base.Dispose(disposing);
        }

        void UpdateSurface()
        {
            _buffer?.Dispose();
            _buffer = new NativeImageSource((uint)Size.Width, (uint)Size.Height, NativeImageSource.ColorDepth.Default);
        }

        void OnResized(object source, PropertyNotification.NotifyEventArgs e)
        {
            OnResized();
        }

        static Geometry CreateQuadGeometry()
        {
            PropertyBuffer vertexData = CreateVertextBuffer();

            TexturedQuadVertex vertex1 = new TexturedQuadVertex();
            TexturedQuadVertex vertex2 = new TexturedQuadVertex();
            TexturedQuadVertex vertex3 = new TexturedQuadVertex();
            TexturedQuadVertex vertex4 = new TexturedQuadVertex();
            vertex1.position = new Vec2(-0.5f, -0.5f);
            vertex2.position = new Vec2(-0.5f, 0.5f);
            vertex3.position = new Vec2(0.5f, -0.5f);
            vertex4.position = new Vec2(0.5f, 0.5f);

            TexturedQuadVertex[] texturedQuadVertexData = new TexturedQuadVertex[4] { vertex1, vertex2, vertex3, vertex4 };

            int lenght = Marshal.SizeOf(vertex1);
            IntPtr pA = Marshal.AllocHGlobal(lenght * 4);

            for (int i = 0; i < 4; i++)
            {
                Marshal.StructureToPtr(texturedQuadVertexData[i], pA + i * lenght, true);
            }
            vertexData.SetData(pA, 4);

            Geometry geometry = new Geometry();
            geometry.AddVertexBuffer(vertexData);
            geometry.SetType(Geometry.Type.TRIANGLE_STRIP);
            return geometry;
        }

        static PropertyBuffer CreateVertextBuffer()
        {
            PropertyMap vertexFormat = new PropertyMap();
            vertexFormat.Add("aPosition", new PropertyValue((int)PropertyType.Vector2));
            return new PropertyBuffer(vertexFormat);
        }

        struct TexturedQuadVertex
        {
            public Vec2 position;
        };

        [StructLayout(LayoutKind.Sequential)]
        struct Vec2
        {
            float x;
            float y;
            public Vec2(float xIn, float yIn)
            {
                x = xIn;
                y = yIn;
            }
        }
    }
}
