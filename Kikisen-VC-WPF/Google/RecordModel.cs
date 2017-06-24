using Kikisen_VC_WPF.MS;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;

namespace Kikisen_VC_WPF.Google
{
	class RecordModel : IAudioRecorder
	{
		#region 変数

		private WaveInEvent waveIn;
		private WasapiLoopbackCapture Waloop = null;
		public bool isStoped = false;
		private bool isDisposed = false;
		
		private SpeechStreamer _ms_wloop_ss;
		private WdlResampler resampler;

		public object Waloop1 { get => Waloop; set => Waloop = (WasapiLoopbackCapture)value; }

		#endregion

		#region メソッド

		public void Start() {
			if (this.isDisposed) {
				throw new ObjectDisposedException("RecordModel");
			}
			if (this.waveIn != null) {
				return;
			}
			if (this.Waloop != null) {
				Console.WriteLine("{0}", this.Waloop.CaptureState);
				return;
			}

			int waveInDevices = WaveIn.DeviceCount;
			if (WaveIn.DeviceCount == MainWindow.InputDevice) {
				// ループバックの場合
				MMDevice outdevice = null;
				outdevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); // 既定の出力
				this.Waloop = new WasapiLoopbackCapture(outdevice);
				//this.Waloop = new WasapiLoopbackCapture(new MMDeviceEnumerator().GetDevice(MainWindow.MMDoutputDevice));
				this.Waloop.DataAvailable += this.OnDataAvailable;
				this.Waloop.ShareMode = AudioClientShareMode.Shared;

				resampler = new WdlResampler();
				resampler.SetMode(true, 2, false);
				resampler.SetFilterParms();
				resampler.SetFeedMode(true); // input driven
				resampler.SetRates(this.Waloop.WaveFormat.SampleRate, 16000);

				this.Waloop.StartRecording();

			} else {
				this.waveIn = new WaveInEvent();
				this.waveIn.DataAvailable += this.OnDataAvailable;
				this.waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
				this.waveIn.DeviceNumber = MainWindow.InputDevice; // 録音デバイスの設定

				this.waveIn.StartRecording();
			}
		}

		public void Stop() {
			if (this.isDisposed) {
				//throw new ObjectDisposedException("RecordModel");
			}
			if (this.isStoped) {
				return;
			}

			int waveInDevices = WaveIn.DeviceCount;
			if (this.waveIn != null) {
				this.waveIn.StopRecording();
				this.waveIn.Dispose();
				this.waveIn = null;
			}
			this.isStoped = true;
			if (this.Waloop != null) {
				if (this.Waloop.CaptureState != CaptureState.Stopped) {
					this.Waloop.StopRecording();
					this.Waloop.DataAvailable -= this.OnDataAvailable;
				}
				this.Waloop.Dispose();
				this.Waloop = null;
			}
			this.isStoped = true;
		}

		public void Dispose() {
			if (this.isDisposed) {
				throw new ObjectDisposedException("RecordModel");
			}

			this.Stop();
			GC.SuppressFinalize(this);
			this.isDisposed = true;
		}

		~RecordModel() {
			this.Dispose();
		}

		public byte[] Convert16(byte[] input, int length, WaveFormat format) {
			if (length == 0)
				return new byte[0];
			using (var memStream = new MemoryStream(input, 0, length)) {
				using (var inputStream = new RawSourceWaveStream(memStream, format)) {
					var sampleStream = new NAudio.Wave.SampleProviders.WaveToSampleProvider(inputStream);
					var resamplingProvider = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(sampleStream, resampler, 16000);
					var ieeeToPCM = new NAudio.Wave.SampleProviders.SampleToWaveProvider16(resamplingProvider);
					var sampleStreams = new NAudio.Wave.StereoToMonoProvider16(ieeeToPCM);
					sampleStreams.RightVolume = 0.5f;
					sampleStreams.LeftVolume = 0.5f;
					return readStream(sampleStreams, length);
				}
			}
		}
		private byte[] readStream(IWaveProvider waveStream, int length) {
			byte[] buffer = new byte[length];
			using (var stream = new MemoryStream()) {
				int read;
				while ((read = waveStream.Read(buffer, 0, length)) > 0) {
					stream.Write(buffer, 0, read);
				}
				return stream.ToArray();
			}
		}
		private void OnDataAvailable(object sender, WaveInEventArgs e) {
			if (this.isStoped) return;
			if (this.Waloop != null) {
				byte[] output = Convert16(e.Buffer, e.BytesRecorded, this.Waloop.WaveFormat);
				this.RecordDataAvailabled?.Invoke(this, new RecordDataAvailabledEventArgs(output, output.Length));
			} else {
				this.RecordDataAvailabled?.Invoke(this, new RecordDataAvailabledEventArgs(e.Buffer, e.BytesRecorded));
			}
		}

		#endregion

		#region イベント

		public event RecordDataAvailabledEventHandler RecordDataAvailabled;

		#endregion
	}
}
