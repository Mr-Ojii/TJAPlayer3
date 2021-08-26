using System;
using System.IO;
using System.Diagnostics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using OpenTK.Graphics.OpenGL;

namespace FDK
{
    public class CUseOnlyTexture : IDisposable
    {
        private int? texture = null;
        public Size textureSize
        {
            get;
            private set;
        } = Size.Empty;

        public CUseOnlyTexture(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Texture File Not Found.({fileName})\n");
            MakeTexture(Image.Load<Rgba32>(fileName));
        }

        public CUseOnlyTexture(Image<Rgba32> image)
        {
            MakeTexture(image);
        }

        private void MakeTexture(Image<Rgba32> image)
        {
            image.Mutate(ctx => ctx.Flip(FlipMode.Vertical));
            try
            {
                this.textureSize = image.Size();

                this.texture = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 3);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinLod, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLod, 3);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.LinearSharpenSgis);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);


                if (image.TryGetSinglePixelSpan(out Span<Rgba32> span))
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, span.ToArray());
                }
                else
                {
                    throw new CTextureCreateFailedException("Failed to GetPixelData.\n");
                }

                GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
            catch (Exception e)
            {
                this.Dispose();
                Trace.TraceError(e.ToString());
                throw new CTextureCreateFailedException(string.Format("Failed to create texture. \n"));
            }
        }

        public void UpdateTexture(IntPtr bitmap, Size size)
        {
            if (this.texture != null && this.textureSize == size)
            {
                GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, size.Width, size.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmap);

                GL.Hint(HintTarget.GenerateMipmapHint, HintMode.Nicest);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }
        }

        public void Bind()
        {
            if (this.texture != null)
                GL.BindTexture(TextureTarget.Texture2D, (int)this.texture);
        }
        public void UnBind()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Dispose()
        {
            if (texture != null)
            {
                GL.DeleteTexture((int)texture);
                texture = null;
            }
        }
    }
}