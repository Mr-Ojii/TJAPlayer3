using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Policy;
using FFmpeg.AutoGen;

namespace FDK
{
	public unsafe class CAudioDecoder
	{
		public static int AudioDecode(string filename, out byte[] buffer,
			out int nPCMデータの先頭インデックス, out int totalPCMSize, out CWin32.WAVEFORMATEX wfx, bool enablechunk)
		{
			if (!File.Exists(filename))
				throw new FileNotFoundException(filename + " not found...");
			
			AVFormatContext* format_context = null;
			if (ffmpeg.avformat_open_input(&format_context, filename, null, null) != 0)
				throw new FileLoadException("avformat_open_input failed\n");
			
			// get stream info
			if (ffmpeg.avformat_find_stream_info(format_context, null) < 0)
				throw new FileLoadException("avformat_find_stream_info failed\n");

			// find audio stream
			AVStream* audio_stream = null;
			for (int i = 0; i < (int)format_context->nb_streams; i++)
			{
				if (format_context->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO)
				{
					audio_stream = format_context->streams[i];
					break;
				}
			}
			if (audio_stream == null)
				throw new FileLoadException("No audio stream ...\n");

			// find decoder
			AVCodec* codec = ffmpeg.avcodec_find_decoder(audio_stream->codecpar->codec_id);
			if (codec == null)
				throw new NotSupportedException("No supported decoder ...\n");

			// alloc codec context
			AVCodecContext* codec_context = ffmpeg.avcodec_alloc_context3(codec);
			if (codec_context == null)
				throw new OutOfMemoryException("avcodec_alloc_context3 failed\n");

			// open codec
			if (ffmpeg.avcodec_parameters_to_context(codec_context, audio_stream->codecpar) < 0)
			{
				Trace.WriteLine("avcodec_parameters_to_context failed\n");
			}
			if (ffmpeg.avcodec_open2(codec_context, codec, null) != 0)
			{
				Trace.WriteLine("avcodec_open2 failed\n");
			}

			// フレームの確保
			AVFrame* frame = ffmpeg.av_frame_alloc();
			AVPacket* packet = ffmpeg.av_packet_alloc();
			ffmpeg.av_init_packet(packet);
			SwrContext* swr = null;
			byte* swr_buf = null;
			int swr_buf_len = 0;
			int ret;
			int nFrame = 0;
			int nSample = 0;
			int pos = 0;

			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					if (enablechunk) 
					{
						bw.Write(new byte[] { 0x52, 0x49, 0x46, 0x46 });        // 'RIFF'
						bw.Write((UInt32)0);                                    // ファイルサイズ - 8 [byte]；今は不明なので後で上書きする。
						bw.Write(new byte[] { 0x57, 0x41, 0x56, 0x45 });        // 'WAVE'
						bw.Write(new byte[] { 0x66, 0x6D, 0x74, 0x20 });        // 'fmt '
						bw.Write((UInt32)(16));  // fmtチャンクのサイズ[byte]
						bw.Write((UInt16)1);					             // フォーマットID（リニアPCMなら1）
						bw.Write((UInt16)codec_context->channels);                     // チャンネル数
						bw.Write((UInt32)codec_context->sample_rate);                   // サンプリングレート
						bw.Write((UInt32)(16 / 8 * codec_context->channels * codec_context->sample_rate));            // データ速度
						bw.Write((UInt16)(codec_context->channels * 16 / 8));                   // ブロックサイズ
						bw.Write((UInt16)16);                    // サンプルあたりのビット数
						bw.Write(new byte[] { 0x64, 0x61, 0x74, 0x61 });        // 'data'
						pos = (int)ms.Position;
						bw.Write((UInt32)0);
					}
					while (true)
					{
						if (ffmpeg.av_read_frame(format_context, packet) < 0)
						{
							Trace.TraceError("av_read_frame eof or error.\n");
							break; // eof or error
						}
						if (packet->stream_index == audio_stream->index)
						{
							if (ffmpeg.avcodec_send_packet(codec_context, packet) < 0)
							{
								Trace.TraceError("avcodec_send_packet error\n");
							}
							if ((ret = ffmpeg.avcodec_receive_frame(codec_context, frame)) < 0)
							{
								if (ret != ffmpeg.AVERROR(ffmpeg.EAGAIN))
								{
									Trace.TraceError("avcodec_receive_frame error.\n");
									break;
								}
							}
							else
							{
								nFrame++;
								nSample += frame->nb_samples;

								if ((AVSampleFormat)frame->format != AVSampleFormat.AV_SAMPLE_FMT_S16)
								{
									if (swr == null)
									{
										swr = ffmpeg.swr_alloc();
										if (swr == null)
										{
											Trace.TraceError("swr_alloc error.\n");
											break;
										}
										ffmpeg.av_opt_set_int(swr, "in_channel_layout", (long)frame->channel_layout, 0);
										ffmpeg.av_opt_set_int(swr, "out_channel_layout", (long)frame->channel_layout, 0);
										ffmpeg.av_opt_set_int(swr, "in_sample_rate", frame->sample_rate, 0);
										ffmpeg.av_opt_set_int(swr, "out_sample_rate", frame->sample_rate, 0);
										ffmpeg.av_opt_set_sample_fmt(swr, "in_sample_fmt", (AVSampleFormat)frame->format, 0);
										ffmpeg.av_opt_set_sample_fmt(swr, "out_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_S16, 0);
										ret = ffmpeg.swr_init(swr);
										if (ret < 0)
										{
											Trace.TraceError("swr_init error.\n");
											break;
										}
										swr_buf_len = ffmpeg.av_samples_get_buffer_size(null, frame->channels, frame->sample_rate, AVSampleFormat.AV_SAMPLE_FMT_S16, 1);
										swr_buf = (byte*)ffmpeg.av_malloc((ulong)swr_buf_len);
									}

									ret = ffmpeg.swr_convert(swr, &swr_buf, frame->nb_samples, frame->extended_data, frame->nb_samples);
									for (int index = 0; index < frame->nb_samples * (16 / 8) * frame->channels; index++)
									{
										bw.Write(swr_buf[index]);
									}
								}
								else 
								{
									for (int index = 0; index < frame->nb_samples * (16 / 8) * frame->channels; index++)
									{
										bw.Write((*(frame->extended_data))[index]);
									}
								}


							}
						}
						ffmpeg.av_packet_unref(packet);
					}
					if (enablechunk)
					{
						bw.Seek(4, SeekOrigin.Begin);
						bw.Write((UInt32)ms.Length - 8);                    // ファイルサイズ - 8 [byte]

						bw.Seek(pos, SeekOrigin.Begin);
						bw.Write((UInt32)ms.Length - (pos + 4)); // dataチャンクサイズ [byte]
					}

					bw.Close();

					Debug.Print("Frames=" + nFrame + "\n" + "Samples=" + nSample);
					buffer = ms.ToArray();
				}
			}

			nPCMデータの先頭インデックス = pos;
			totalPCMSize = buffer.Length;
			wfx = new CWin32.WAVEFORMATEX(1, (ushort)codec_context->channels, (uint)codec_context->sample_rate, (uint)(16 / 8 * codec_context->channels * codec_context->sample_rate), (ushort)(codec_context->channels * 16 / 8), 16);

			ffmpeg.av_free((void*)swr_buf);
			ffmpeg.av_frame_free(&frame);
			ffmpeg.avcodec_free_context(&codec_context);
			ffmpeg.avformat_close_input(&format_context);

			return 0;

		}
	}
}
