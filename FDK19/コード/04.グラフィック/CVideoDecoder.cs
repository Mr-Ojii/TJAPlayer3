using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFmpeg.AutoGen;
using SharpDX.Direct3D9;
using System.Threading;

namespace FDK
{
	public unsafe class CVideoDecoder : IDisposable
	{
		public CVideoDecoder(Device device, string filename)
		{
			this.device = device;
			if (!File.Exists(filename))
				throw new FileNotFoundException(filename + " not found...");

			format_context = ffmpeg.avformat_alloc_context();
			fixed (AVFormatContext** format_contexttmp = &format_context)
			{
				if (ffmpeg.avformat_open_input(format_contexttmp, filename, null, null) != 0)
					throw new FileLoadException("avformat_open_input failed\n");

				if (ffmpeg.avformat_find_stream_info(*format_contexttmp, null) < 0)
					throw new FileLoadException("avformat_find_stream_info failed\n");

				// find audio stream
				for (int i = 0; i < (int)format_context->nb_streams; i++)
				{
					if (format_context->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO)
					{
						video_stream = format_context->streams[i];
						break;
					}
				}
				if (video_stream == null)
					throw new FileLoadException("No video stream ...\n");

				// find decoder
				codec = ffmpeg.avcodec_find_decoder(video_stream->codecpar->codec_id);
				if (codec == null)
					throw new NotSupportedException("No supported decoder ...\n");

				codec_context = ffmpeg.avcodec_alloc_context3(codec);

				if (ffmpeg.avcodec_parameters_to_context(codec_context, video_stream->codecpar) < 0)
					Trace.WriteLine("avcodec_parameters_to_context failed\n");
				
				if (ffmpeg.avcodec_open2(codec_context, codec, null) != 0)
					Trace.WriteLine("avcodec_open2 failed\n");

				FrameSize = new Size(codec_context->width, codec_context->height);
				pixelFormat = codec_context->pix_fmt;
				framecount = codec_context->frame_number;
				Framerate = video_stream->avg_frame_rate;

				frame = ffmpeg.av_frame_alloc();
				packet = ffmpeg.av_packet_alloc();

				convert_context = ffmpeg.sws_getContext(FrameSize.Width, FrameSize.Height, pixelFormat,
				FrameSize.Width,
				FrameSize.Height, 
				AVPixelFormat.AV_PIX_FMT_BGR24,
				ffmpeg.SWS_FAST_BILINEAR, null, null, null);
				if (convert_context == null) throw new ApplicationException("Could not initialize the conversion context.");
				decodedframes = new Queue<CDecodedFrame>();
				CTimer = new CTimer(CTimer.E種別.MultiMedia);

				_convertedFrameBufferPtr = Marshal.AllocHGlobal(ffmpeg.av_image_get_buffer_size(CVPxfmt, codec_context->width, codec_context->height, 1));

				_dstData = new byte_ptrArray4();
				_dstLinesize = new int_array4();
				ffmpeg.av_image_fill_arrays(ref _dstData, ref _dstLinesize, (byte*)_convertedFrameBufferPtr, CVPxfmt, codec_context->width, codec_context->height, 1);

			}
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(_convertedFrameBufferPtr);
			ffmpeg.sws_freeContext(convert_context);
			ffmpeg.av_frame_unref(frame);
			ffmpeg.av_free(frame);

			ffmpeg.av_packet_unref(packet);
			ffmpeg.av_free(packet); 

			ffmpeg.avcodec_close(codec_context);
			fixed (AVFormatContext** format_contexttmp = &format_context)
			{
				ffmpeg.avformat_close_input(format_contexttmp);
			}
			if (lastTexture != null)
				lastTexture.Dispose();
			decodedframes.Clear();
		}


		public void Start()
		{
			this.EnqueueFrames();
			CTimer.tリセット();
			
			this.bPlaying = true;
		}

		public void SkipStart(long timestamp) 
		{
			this.Seek(timestamp);
			this.PauseControl();
		}

		public void PauseControl()
		{
			if (this.bPlaying)
			{
				CTimer.t一時停止();
				this.bPlaying = false;
			}
			else
			{
				CTimer.t再開();
				this.bPlaying = true;
			}
		}

		public void Stop() 
		{
			CTimer.t一時停止();
			this.bPlaying = false;
		}

		public void InitRead() 
		{
			if (!bqueueinitialized)
				this.EnqueueFrames();
			else
				Trace.TraceError("The class has already been initialized.");
		}

		public void Reset() 
		{
			this.Seek(1);
			CTimer.tリセット();
			decodedframes.Clear();
			this.EnqueueFrames();
		}

		private void Seek(long timestamp)
		{
			cts.Cancel();
			ffmpeg.av_seek_frame(format_context, video_stream->index, timestamp, ffmpeg.AVSEEK_FLAG_BACKWARD);
			ffmpeg.avcodec_flush_buffers(codec_context);
			CTimer.n現在時刻ms = timestamp;
			decodedframes.Clear();
			this.EnqueueFrames();
		}

		public void GetNowFrame(ref CTexture Texture)
		{
			CTimer.t更新();
			while (nextframetime <= (CTimer.n現在時刻ms * fPlaySpeed))
			{
				if (decodedframes.Count != 0)
				{			
					using (CDecodedFrame cdecodedframe = decodedframes.Dequeue())
					{
						if (cdecodedframe.Time <= (CTimer.n現在時刻ms * fPlaySpeed))
							continue;
						CTexture txtmp = GeneFrmTx(cdecodedframe.Bitmap);
						this.EnqueueFrames();
						if (lastTexture != null)
							lastTexture.Dispose();
						nextframetime = cdecodedframe.Time;
						lastTexture = txtmp;
					}
				}
				else
                {
					break;
				}
			}

			if (lastTexture == null)
				lastTexture = GeneFrmTx(new Bitmap(1, 1));

			if (Texture == lastTexture)
				return;

			Texture = lastTexture;
			
		}

		private void EnqueueFrames()
		{
			if (DS != DecodingState.Running)
			{
				cts = new CancellationTokenSource();
				decodingTask = Task.Factory.StartNew(() =>
				{
					if (decodedframes.Count < ((int)(Framerate.num / Framerate.den)) * 2)
					{
						while (EnqueueOneFrame() && decodedframes.Count < ((int)(Framerate.num / Framerate.den)) * 2) ;
					}
					DS = DecodingState.Stopped;
				});
				Exception e = decodingTask.Exception;
                if (e!=null)
				{
					Trace.TraceError(e.ToString());
					DS = DecodingState.Stopped;
				}
			}
		}

		private bool EnqueueOneFrame() {
			DS = DecodingState.Running;
			AVFrame outframe;
			ffmpeg.av_frame_unref(frame);
			int error;
			do
			{
				try
				{
					do
					{
						cts.Token.ThrowIfCancellationRequested();
						error = ffmpeg.av_read_frame(format_context, packet);
						if (error == ffmpeg.AVERROR_EOF)
						{
							return false;
						}

						if (error != 0)
							Trace.TraceError("av_read_frame eof or error.\n");

					} while (packet->stream_index != video_stream->index);

					if (ffmpeg.avcodec_send_packet(codec_context, packet) < 0)
						Trace.TraceError("avcodec_send_packet error\n");
				}
				finally
				{
					ffmpeg.av_packet_unref(packet);
				}

				error = ffmpeg.avcodec_receive_frame(codec_context, frame);
			} while (error == ffmpeg.AVERROR(ffmpeg.EAGAIN));
			if (error != 0)
				Trace.TraceError("error.\n");
						
			ffmpeg.sws_scale(convert_context, frame->data, frame->linesize, 0, frame->height, _dstData, _dstLinesize);

			var data = new byte_ptrArray8();
			data.UpdateFrom(_dstData);
			var linesize = new int_array8();
			linesize.UpdateFrom(_dstLinesize);

			outframe = new AVFrame
			{
				data = data,
				linesize = linesize,
				width = FrameSize.Width,
				height = FrameSize.Height
			};
			

			decodedframes.Enqueue(new CDecodedFrame { Time = (frame->best_effort_timestamp - video_stream->start_time) * ((double)video_stream->time_base.num / (double)video_stream->time_base.den) * 1000, Bitmap = new Bitmap(outframe.width, outframe.height, outframe.linesize[0], PixelFormat.Format24bppRgb, (IntPtr)outframe.data[0]) });

			
			ffmpeg.av_frame_unref(frame);
			ffmpeg.av_frame_unref(&outframe);
			ffmpeg.av_free(&outframe);
			return true;
		}

		private CTexture GeneFrmTx(Bitmap bitmap) {
			return new CTexture(this.device, bitmap, Format.R8G8B8, false);
		}

		public Size FrameSize;

		private enum DecodingState 
		{
			Stopped,
			Running
		}

		#region[private]
		//for read & decode
		private float fPlaySpeed = 1.0f;
		private static AVFormatContext* format_context;
		private AVStream* video_stream;
		private AVCodec* codec;
		private AVCodecContext* codec_context;
		private AVFrame* frame;
		private AVPacket* packet;
		private Queue<CDecodedFrame> decodedframes;
		private int framecount;
		private Task decodingTask;
		private Device device;
		private CancellationTokenSource cts;
		private DecodingState DS = DecodingState.Stopped;

		//for play
		private bool bPlaying = false;
		private CTimer CTimer;
		private AVRational Framerate;
		private CTexture lastTexture;
		private double nextframetime = 0;
		private bool bqueueinitialized = false;

		//for convert
		private SwsContext* convert_context;
		private AVPixelFormat pixelFormat;
		private readonly byte_ptrArray4 _dstData;
		private readonly int_array4 _dstLinesize;
		private readonly IntPtr _convertedFrameBufferPtr;
		private const AVPixelFormat CVPxfmt = AVPixelFormat.AV_PIX_FMT_RGB24;
		#endregion
	}
}
