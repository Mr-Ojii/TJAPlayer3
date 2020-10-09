using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;

namespace FDK
{
	public class CAction
    {
        public static void LoadContentAction(Device Device) 
        {
			Device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0f, 0f, (float)(-GameWindowSize.Height / 2 * Math.Sqrt(3.0))), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f)));
			Device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH(C変換.DegreeToRadian((float)60f), ((float)Device.Viewport.Width) / ((float)Device.Viewport.Height), -100f, 100f));
			Device.SetRenderState(RenderState.Lighting, false);
			Device.SetRenderState(RenderState.ZEnable, false);
			Device.SetRenderState(RenderState.AntialiasedLineEnable, false);
			Device.SetRenderState(RenderState.AlphaTestEnable, true);
			Device.SetRenderState(RenderState.AlphaRef, 10);

			Device.SetRenderState(RenderState.MultisampleAntialias, true);
			Device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Linear);
			Device.SetSamplerState(0, SamplerState.MagFilter, TextureFilter.Linear);
			
			Device.SetRenderState<Compare>(RenderState.AlphaFunc, Compare.Greater);
			Device.SetRenderState(RenderState.AlphaBlendEnable, true);
			Device.SetRenderState<Blend>(RenderState.SourceBlend, Blend.SourceAlpha);
			Device.SetRenderState<Blend>(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
			Device.SetTextureStageState(0, TextureStage.AlphaOperation, TextureOperation.Modulate);
			Device.SetTextureStageState(0, TextureStage.AlphaArg1, 2);
			Device.SetTextureStageState(0, TextureStage.AlphaArg2, 1);
		}

		public static void BeginScene(Device Device) 
		{
			Device.BeginScene();
			Device.Clear(ClearFlags.ZBuffer | ClearFlags.Target, Color.Black, 1f, 0);
		}

		public static void EndScene(Device Device) 
		{
			Device.EndScene();
		}

		public static void Flush() 
		{
#if OpenGL
			OpenTK.Graphics.OpenGL.GL.Flush();
#endif
		}
	}
}
