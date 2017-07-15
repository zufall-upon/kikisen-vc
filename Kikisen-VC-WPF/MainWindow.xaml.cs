using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Media.Imaging;
using System.Speech.Recognition;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using Grpc.Core;
using Google.Cloud.Speech.V1;
using System.Threading.Tasks;
using Kikisen_VC_WPF.Google;
using System.Text.RegularExpressions;
using System.Text;
using System.Net.Http;
using Microsoft.Win32;
using NAudio.CoreAudioApi;
using NAudio.Wave.Compression;
using NAudio.Utils;
using Kikisen_VC_WPF.MS;
using System.Net;
using Google.Apis.Translate.v2;
using Google.Apis.Services;
using Microsoft.CognitiveServices.SpeechRecognition;

namespace Kikisen_VC_WPF
{
	public partial class MainWindow : Window
	{
		private static string _InputDevice = Properties.Settings.Default.InputDevice;
		private static string _OutputDevice = Properties.Settings.Default.OutputDevice;
		private string _RecogAPI = Properties.Settings.Default.RecogAPI;
		private string _SpeechAPI = Properties.Settings.Default.SpeechAPI;
		private string _TranslateAPI = Properties.Settings.Default.TranslateAPI;
		private string _TranslateSetting = Properties.Settings.Default.TranslateSetting;
		private string _Phrases = Properties.Settings.Default.Phrases;
		private string _sayVolume = Properties.Settings.Default.sayVolume;
		private string _sayPitch = Properties.Settings.Default.sayPitch;
		private string _saySpeed = Properties.Settings.Default.saySpeed;
		private string _sayEmotion = Properties.Settings.Default.sayEmotion;
		private string _say_msVolume = Properties.Settings.Default.say_msVolume;
		private string _say_msPitch = Properties.Settings.Default.say_msPitch;
		private string _say_msEmphasis = Properties.Settings.Default.say_msEmphasis;
		private string _say_msRate = Properties.Settings.Default.say_msRate;
		private string _keyVTWAPI = Properties.Settings.Default.keyVTWAPI;
		private string _keyGCSAPIjsonPath = Properties.Settings.Default.keyGCSAPIjsonPath;
		private bool _OutputText = Properties.Settings.Default.OutputText;
		private bool _OutputText_opt1 = Properties.Settings.Default.OutputText_opt1;
		private string _keyGTAPI = Properties.Settings.Default.keyGTAPI;
		private string _keyBingSAPI1 = Properties.Settings.Default.keyBingSpAPI1;
		private bool _readLogFile = Properties.Settings.Default.readLogFile;
		private string _readLogFilePath = Properties.Settings.Default.readLogFilePath;
		private string _readLogFileExcept = Properties.Settings.Default.readLogFileExcept;
		private bool _readLogFileNicknameChk = Properties.Settings.Default.readLogFileNicknameChk;
		private string _readLogFileNicknameString = Properties.Settings.Default.readLogFileNicknameString;
		private bool _readLogFileNicknameChkUseVTWAPI = Properties.Settings.Default.readLogFileNicknameChkUseVTWAPI;

		private static System.Windows.Controls.ComboBox _cmbInputDevice;
		private static System.Windows.Controls.ComboBox _cmbOutputDevice;
		private static string _outdevice;
		private static string _strGCSAPIJson = "";
		private List<string> _lstPhrases = null;
		private List<string> _lstMsActors = null;
		private List<string> _lstVTActors = null;
		private double _threadwaitsec = 1000;
		private CancellationTokenSource _tokenGCSAPIcancelTokenS;
		private bool isNowTestingGCS = false;
		private string _recog_lang_set = "";

		BackgroundWorker Worker;
		Action<string> _funcGoogleCloudSpeechinit;
		Action<string> _funcVoicetextinit;
		Action<string> _funcGoogleTranslatorinit;
		Action<string> _funcBingSpeechinit;
		SpeechRecognitionEngine _ms_recogEngine;
		WaveIn _ms_wi;
		SpeechStreamer _ms_ss;
		WasapiLoopbackCapture _ms_wloop;
		WaveFileWriter _ms_writer;
		SpeechStreamer _ms_wloop_ss;
		WdlResampler resampler;
		int countBytes = 0;
		Byte[] AudioBuffer = new Byte[20000000];
		System.Speech.AudioFormat.SpeechAudioFormatInfo _in_safi;
		System.Speech.AudioFormat.SpeechAudioFormatInfo _out_safi;
		DataRecognitionClient _micClient;
		int _iReadLogFileCount = 0;
		FileSystemWatcher _readLogFileWatcher;
		DateTime _readLogFileLastWriteFileTime = DateTime.Now;
		List<string> _lstNicknames = new List<string>();
		Dictionary<string, Dictionary<string, string>> _lstNicknameOptions = new Dictionary<string, Dictionary<string, string>>();


		public static int InputDevice { get => _cmbInputDevice.Items.IndexOf(_InputDevice); }
		public static int OutputDevice { get => _cmbOutputDevice.Items.IndexOf(_OutputDevice); }
		public static string MMDoutputDevice { get => _outdevice; }

		public MainWindow() {
			InitializeComponent();

			_cmbInputDevice = cmbInputDevice;
			_cmbOutputDevice = cmbOutputDevice;

			chkOutputText.IsChecked = _OutputText;
			chkOutputText_opt1.IsChecked = _OutputText_opt1;
			txtReadLogFileExcept.Text = _readLogFileExcept;
			chkReadLogFile.IsChecked = _readLogFile;
			txtReadLogFile.Text = _readLogFilePath;
			txtReadLogFileNicknameString.Text = _readLogFileNicknameString;
			chkReadLogFileNicknameChk.IsChecked = _readLogFileNicknameChk;
			chkReadLogFileNicknameChkUseVTWAPI.IsChecked = _readLogFileNicknameChkUseVTWAPI;

			_in_safi = new System.Speech.AudioFormat.SpeechAudioFormatInfo(16000, System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, System.Speech.AudioFormat.AudioChannel.Mono);
			_out_safi = new System.Speech.AudioFormat.SpeechAudioFormatInfo(44100, System.Speech.AudioFormat.AudioBitsPerSample.Sixteen, System.Speech.AudioFormat.AudioChannel.Mono);

			//GoogleCloudSpeechAPI　お試し用のJSONファイルを読み込む
			_funcGoogleCloudSpeechinit = delegate (string jsonpath) {
				if (_RecogAPI == "GoogleCloudSpeechAPI" || tabReconAPI_GCS_set.IsSelected) {
					if (FuncGoogleCloudSpeechAPIisEnable(jsonpath).Result) {
						// APIキーが正しい場合、正しいjsonファイルで動作させ、表示を変える
						lblGCSAPI.Text = "認証済み";
						_strGCSAPIJson = File.ReadAllText(jsonpath);
						isNowTestingGCS = false;
					} else {
						// APIキーがないか間違っている場合、お試しモードで動作する、お試し終了後はメッセージで終了する
						//System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
						//using (StreamReader sr = new StreamReader(myAssembly.GetManifestResourceStream("Kikisen_VC_WPF.Google.testkey.json"), Encoding.GetEncoding("ascii"))) {
						//	_strGCSAPIJson = sr.ReadToEnd();
						//	sr.Close();
						//	_keyGCSAPIjsonPath = "";
						//	lblGCSAPI.Text = "未認証";
						//	isNowTestingGCS = true;
						//}
						_keyGCSAPIjsonPath = "";
						lblGCSAPI.Text = "未認証";
						isNowTestingGCS = false;
					}
					// バックグラウンド処理をキャンセルする
					if (this.Worker != null) this.FuncWorkerReset();
					return;
				} else if (jsonpath != "") {
					// APIキーが格納されてれば正しいものとする
					lblGCSAPI.Text = "認証済み";
					_strGCSAPIJson = File.ReadAllText(jsonpath);
				}
			};
			_funcGoogleCloudSpeechinit(_keyGCSAPIjsonPath);

			//BingSpeechAPI
			_funcBingSpeechinit = delegate (string apikey) {
				if (_RecogAPI == "BingSpeechAPI" || tabReconAPI_BS_set.IsSelected) {
					if (FuncBingSpeechAPIisEnable(apikey).Result) {
						// APIキーが正しい場合
						btnBingSpAPIkey.Content = "BingSpeechAPI(認証済)";
						txtbBingSpAPIkey.Password = apikey;
					} else {
						btnBingSpAPIkey.Content = "BingSpeechAPI(未認証)";
						txtbBingSpAPIkey.Password = "";
					}
					// バックグラウンド処理をキャンセルする
					if (this.Worker != null) this.FuncWorkerReset();
				} else if (apikey != "") {
					// APIキーが格納されてれば正しいものとする
					btnBingSpAPIkey.Content = "BingSpeechAPI(認証済)";
					txtbBingSpAPIkey.Password = apikey;
				}
			};
			_funcBingSpeechinit(_keyBingSAPI1);

			_funcGoogleTranslatorinit = delegate (string gtapikey) {
				if (_TranslateAPI == "GoogleTranslatorAPI" || tabTranslator_GTAPI.IsSelected) {
					if (FuncTranslateTextToSpeech("test", gtapikey).Result) {
						// APIキーが正しい場合
						btnGTAPIkey.Content = "GoogleTranslatorAPI(認証済)";
						txtbGTAPIkey.Password = gtapikey;
					} else {
						// APIキーがないか間違っている場合
						btnGTAPIkey.Content = "GoogleTranslatorAPI(未認証)";
						txtbGTAPIkey.Password = "";
					}
					// バックグラウンド処理をキャンセルする
					if (this.Worker != null) this.FuncWorkerReset();
				} else if (gtapikey != "") {
					// APIキーが格納されてれば正しいものとする
					btnGTAPIkey.Content = "GoogleTranslatorAPI(認証済)";
					txtbGTAPIkey.Password = gtapikey;
				}
			};
			_funcGoogleTranslatorinit(_keyGTAPI);

			// SpeechAPIの初期化
			_funcVoicetextinit = delegate (string pw) {
				// MS話者を追加
				var tmpSpeechAPI = "";
				_SpeechAPI = (_SpeechAPI == "") ? "Microsoft Haruka Desktop" : _SpeechAPI;
				tmpSpeechAPI = _SpeechAPI;
				cmbSpeechAPI.Items.Clear();
				_lstMsActors = new List<string>();
				var synthesizer = new System.Speech.Synthesis.SpeechSynthesizer();
				var voices = synthesizer.GetInstalledVoices();
				foreach (var voice in voices) {
					if (!_lstMsActors.Contains(voices[0].VoiceInfo.Name)) {
						cmbSpeechAPI.Items.Add(voices[0].VoiceInfo.Name);
						_lstMsActors.Add(voices[0].VoiceInfo.Name);
					}
				}
				cmbSpeechAPI.Items.Refresh();
				cmbSpeechAPI.SelectedIndex = (0 <= cmbSpeechAPI.Items.IndexOf(_SpeechAPI)) ? cmbSpeechAPI.Items.IndexOf(_SpeechAPI) : 0;

				_say_msVolume = (_say_msVolume == "") ? "80" : _say_msVolume;
				cmbMsVolume.Items.Clear();
				cmbMsVolume.Items.Add("[音量]");
				List<string> lstMsVolume = new List<string>() { "50", "60", "70", "80", "90", "100" };
				foreach (string str in lstMsVolume) {
					cmbMsVolume.Items.Add(str);
				}
				cmbMsVolume.Items.Refresh();
				cmbMsVolume.SelectedIndex = (0 <= cmbMsVolume.Items.IndexOf(_say_msVolume)) ? cmbMsVolume.Items.IndexOf(_say_msVolume) : 1;

				_say_msPitch = (_say_msPitch == "") ? "0" : _say_msPitch;
				cmbMsPitch.Items.Clear();
				cmbMsPitch.Items.Add("[ﾋﾟｯﾁ]");
				List<string> lstMsPitch = new List<string>() { "-10", "-9", "-8", "-7", "-6", "-5", "-4", "-3", "-2", "-1", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
				foreach (string str in lstMsPitch) {
					cmbMsPitch.Items.Add(str);
				}
				cmbMsPitch.Items.Refresh();
				cmbMsPitch.SelectedIndex = (0 <= cmbMsPitch.Items.IndexOf(_say_msPitch)) ? cmbMsPitch.Items.IndexOf(_say_msPitch) : 1;

				_say_msEmphasis = (_say_msEmphasis == "") ? "Moderate" : _say_msEmphasis;
				cmbMsEmphasis.Items.Clear();
				cmbMsEmphasis.Items.Add("[強調]");
				List<string> lstMsEmphasis = new List<string>() { "NotSet", "Strong", "Moderate", "None", "Reduced" };
				foreach (string str in lstMsEmphasis) {
					cmbMsEmphasis.Items.Add(str);
				}
				cmbMsEmphasis.Items.Refresh();
				cmbMsEmphasis.SelectedIndex = (0 <= cmbMsEmphasis.Items.IndexOf(_say_msEmphasis)) ? cmbMsEmphasis.Items.IndexOf(_say_msEmphasis) : 1;

				_say_msRate = (_say_msRate == "") ? "Medium" : _say_msRate;
				cmbMsRate.Items.Clear();
				cmbMsRate.Items.Add("[速度]");
				List<string> lstMsRate = new List<string>() { "NotSet", "ExtraFast", "Fast", "Medium", "Slow", "ExtraSlow" };
				foreach (string str in lstMsRate) {
					cmbMsRate.Items.Add(str);
				}
				cmbMsRate.Items.Refresh();
				cmbMsRate.SelectedIndex = (0 <= cmbMsRate.Items.IndexOf(_say_msRate)) ? cmbMsRate.Items.IndexOf(_say_msRate) : 1;

				if (FuncVoiceTextWebAPIisEnable(pw).Result) {
					// APIキーが正しい場合
					tabVTWAPI.IsEnabled = true;
					btnVTWAPIkey.Content = "HOYA VoiceText (認証済)";
					txtbVTWAPIkey.Password = pw;
					// 役者の初期化
					List<string> lstActors = new List<string>() { "show", "haruka", "hikari", "takeru", "santa", "bear" };
					_lstVTActors = lstActors;
					foreach (string str in lstActors) {
						cmbSpeechAPI.Items.Add(str);
					}
					cmbSpeechAPI.Items.Refresh();
					cmbSpeechAPI.SelectedIndex = (0 <= cmbSpeechAPI.Items.IndexOf(_SpeechAPI)) ? cmbSpeechAPI.Items.IndexOf(_SpeechAPI) : 0;

					_sayPitch = (_sayPitch == "") ? "100" : _sayPitch;
					cmbVTPitch.Items.Clear();
					cmbVTPitch.Items.Add("[ﾋﾟｯﾁ]");
					List<string> lstPitch = new List<string>() { "50", "60", "70", "80", "90", "100", "110", "120", "130", "140", "150", "160", "170", "180", "190", "200" };
					foreach (string str in lstPitch) {
						cmbVTPitch.Items.Add(str);
					}
					cmbVTPitch.Items.Refresh();
					cmbVTPitch.SelectedIndex = (0 <= cmbVTPitch.Items.IndexOf(_sayPitch)) ? cmbVTPitch.Items.IndexOf(_sayPitch) : 1;

					_saySpeed = (_saySpeed == "") ? "100" : _saySpeed;
					cmbVTSpeed.Items.Clear();
					cmbVTSpeed.Items.Add("[ｽﾋﾟｰﾄﾞ]");
					List<string> lstSpeed = new List<string>() { "50", "60", "70", "80", "90", "100", "110", "120", "130", "140", "150", "160", "170", "180", "190", "200", "210", "220", "230", "240", "250", "260", "270", "280", "290", "300", "310", "320", "330", "340", "350", "360", "370", "380", "390", "400" };
					foreach (string str in lstSpeed) {
						cmbVTSpeed.Items.Add(str);
					}
					cmbVTSpeed.Items.Refresh();
					cmbVTSpeed.SelectedIndex = (0 <= cmbVTSpeed.Items.IndexOf(_saySpeed)) ? cmbVTSpeed.Items.IndexOf(_saySpeed) : 1;

					_sayVolume = (_sayVolume == "") ? "60" : _sayVolume;
					cmbVTVolume.Items.Clear();
					cmbVTVolume.Items.Add("[音量]");
					List<string> lstVolume = new List<string>() { "50", "60", "70", "80", "90", "100", "110", "120", "130", "140", "150", "160", "170", "180", "190", "200" };
					foreach (string str in lstVolume) {
						cmbVTVolume.Items.Add(str);
					}
					cmbVTVolume.Items.Refresh();
					cmbVTVolume.SelectedIndex = (0 <= cmbVTVolume.Items.IndexOf(_sayVolume)) ? cmbVTVolume.Items.IndexOf(_sayVolume) : 1;

					_sayEmotion = (_sayEmotion == "") ? "なし" : _sayEmotion;
					cmbVTEmotion.Items.Clear();
					cmbVTEmotion.Items.Add("[感情]");
					List<string> lstEmotion = new List<string>() { "なし", "happiness", "anger", "sadness" };
					foreach (string str in lstEmotion) {
						cmbVTEmotion.Items.Add(str);
					}
					cmbVTEmotion.Items.Refresh();
					cmbVTEmotion.SelectedIndex = (0 <= cmbVTEmotion.Items.IndexOf(_sayEmotion)) ? cmbVTEmotion.Items.IndexOf(_sayEmotion) : 1;

				} else {
					// APIキーが不正の場合
					tabVTWAPI.IsEnabled = false;
					btnVTWAPIkey.Content = "HOYA VoiceText (未認証)";
					txtbVTWAPIkey.Password = "";
				}
				// changeEventが走ってしまうので選び直す
				cmbSpeechAPI.Items.Refresh();
				cmbSpeechAPI.SelectedIndex = (0 <= cmbSpeechAPI.Items.IndexOf(tmpSpeechAPI)) ? cmbSpeechAPI.Items.IndexOf(tmpSpeechAPI) : 0;
			};
			_funcVoicetextinit(_keyVTWAPI);

			// 録音デバイスの初期化
			cmbInputDevice.Items.Clear();
			int waveInDevices = WaveIn.DeviceCount;
			for (int waveInDevice = 0; waveInDevice < waveInDevices; waveInDevice++) {
				WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(waveInDevice);
				cmbInputDevice.Items.Add(deviceInfo.ProductName);
			}
			cmbInputDevice.Items.Add("Wasapi Loopback");
			cmbInputDevice.Items.Refresh();
			cmbInputDevice.SelectedIndex = (0 <= cmbInputDevice.Items.IndexOf(_InputDevice)) ? cmbInputDevice.Items.IndexOf(_InputDevice) : 0;

			// 出力デバイスの初期化
			cmbOutputDevice.Items.Clear();
			int waveOutDevices = WaveOut.DeviceCount;
			for (int waveOutDevice = 0; waveOutDevice < waveOutDevices; waveOutDevice++) {
				WaveOutCapabilities deviceInfo = WaveOut.GetCapabilities(waveOutDevice);
				cmbOutputDevice.Items.Add(deviceInfo.ProductName);
			}
			cmbOutputDevice.Items.Refresh();
			cmbOutputDevice.SelectedIndex = (0 <= cmbOutputDevice.Items.IndexOf(_OutputDevice)) ? cmbOutputDevice.Items.IndexOf(_OutputDevice) : 0;

			// 音声認識APIの初期化
			cmbRecogAPI.Items.Clear();
			cmbRecogAPI.Items.Add("Windows音声認識API");
			cmbRecogAPI.Items.Add("GoogleCloudSpeechAPI");
			cmbRecogAPI.Items.Add("BingSpeechAPI");
			cmbRecogAPI.Items.Refresh();
			cmbRecogAPI.SelectedIndex = (0 <= cmbRecogAPI.Items.IndexOf(_RecogAPI)) ? cmbRecogAPI.Items.IndexOf(_RecogAPI) : 0;

			// 翻訳APIの初期化
			cmbTranslate.Items.Clear();
			cmbTranslate.Items.Add("翻訳なし");
			cmbTranslate.Items.Add("GoogleTranslatorAPI");
			cmbTranslate.Items.Refresh();
			cmbTranslate.SelectedIndex = (0 <= cmbTranslate.Items.IndexOf(_TranslateAPI)) ? cmbTranslate.Items.IndexOf(_TranslateAPI) : 0;

			// 翻訳設定の初期化
			cmbTranslateSetting.Items.Clear();
			cmbTranslateSetting.Items.Add("Jpn→Eng");
			cmbTranslateSetting.Items.Add("Eng→Jpn");
			cmbTranslateSetting.Items.Refresh();
			cmbTranslateSetting.SelectedIndex = (0 <= cmbTranslateSetting.Items.IndexOf(_TranslateSetting)) ? cmbTranslateSetting.Items.IndexOf(_TranslateSetting) : 0;

			// 単語辞書の初期化
			try {
				txtbPhrases.Text = _Phrases;
				_lstPhrases = new List<string>();
				string[] lstPhrases = txtbPhrases.Text.Split('|');
				foreach (string str in lstPhrases) {
					_lstPhrases.Add(str.Trim());
				}
			} catch (Exception) {
				_lstPhrases = new List<string>();
				txtbPhrases.Text = "";
				_Phrases = txtbPhrases.Text;
			}

			// ステータス画像を表示
			this.FuncChangeImgStatus(1);

			// 音声認識APIを走らせる
			this.FuncWorkerReset(false);
		}

		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (this.Worker != null) {
				this.FuncChangeImgStatus(1);
				this.Worker.CancelAsync();
			}
			if (_readLogFileWatcher != null) {
				try {
					_readLogFileWatcher.EnableRaisingEvents = false;
				} finally {
					if (_readLogFileWatcher != null) {
						_readLogFileWatcher.Dispose();
						_readLogFileWatcher = null;
					}
				}
			}
			Properties.Settings.Default.InputDevice = _InputDevice;
			Properties.Settings.Default.OutputDevice = _OutputDevice;
			Properties.Settings.Default.RecogAPI = _RecogAPI;
			Properties.Settings.Default.SpeechAPI = _SpeechAPI;
			Properties.Settings.Default.TranslateAPI = _TranslateAPI;
			Properties.Settings.Default.TranslateSetting = _TranslateSetting;
			Properties.Settings.Default.Phrases = _Phrases;
			Properties.Settings.Default.sayVolume = _sayVolume;
			Properties.Settings.Default.sayPitch = _sayPitch;
			Properties.Settings.Default.saySpeed = _saySpeed;
			Properties.Settings.Default.sayEmotion = _sayEmotion;
			Properties.Settings.Default.say_msVolume = _say_msVolume;
			Properties.Settings.Default.say_msPitch = _say_msPitch;
			Properties.Settings.Default.say_msEmphasis = _say_msEmphasis;
			Properties.Settings.Default.say_msRate = _say_msRate;
			Properties.Settings.Default.keyVTWAPI = _keyVTWAPI;
			Properties.Settings.Default.keyGCSAPIjsonPath = _keyGCSAPIjsonPath;
			Properties.Settings.Default.OutputText = _OutputText;
			Properties.Settings.Default.OutputText_opt1 = _OutputText_opt1;
			Properties.Settings.Default.keyGTAPI = _keyGTAPI;
			Properties.Settings.Default.keyBingSpAPI1 = _keyBingSAPI1;
			Properties.Settings.Default.readLogFile = _readLogFile;
			Properties.Settings.Default.readLogFilePath = _readLogFilePath;
			Properties.Settings.Default.readLogFileExcept = _readLogFileExcept;
			Properties.Settings.Default.readLogFileNicknameChk = _readLogFileNicknameChk;
			Properties.Settings.Default.readLogFileNicknameString = _readLogFileNicknameString;
			Properties.Settings.Default.readLogFileNicknameChkUseVTWAPI = _readLogFileNicknameChkUseVTWAPI;
			Properties.Settings.Default.Save();
		}

		private void FuncCombChangeSave(System.Windows.Controls.ComboBox cmb, ref string _grostring, string propname, bool withTitle = false) {
			if (cmb.SelectedItem == null) return;
			if (withTitle && cmb.SelectedIndex == 0) cmb.SelectedIndex = 1;
			_grostring = cmb.SelectedItem.ToString();
			Properties.Settings.Default[propname] = _grostring;
			Properties.Settings.Default.Save();
		}
		public void FuncWriteLogFile(string strlog) {
			try {
				string appendText = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss :") + strlog + Environment.NewLine;
				File.AppendAllText(Environment.CurrentDirectory + "\\kikisen-vc-log.log", appendText);
			} catch (Exception e) {
				MessageBox.Show("ログ書き込みエラー" + Environment.NewLine + e.ToString());
			}
		}
		public void FuncChangeImgStatus(int imode) {
			var uriSource = new Uri(@"resource/imgGreen.png", UriKind.Relative);
			switch (imode) {
				case 1:
					uriSource = new Uri(@"resource/imgYellow.png", UriKind.Relative);
					break;
				case 2:
					uriSource = new Uri(@"resource/imgRed.png", UriKind.Relative);
					break;
				default:
					uriSource = new Uri(@"resource/imgGreen.png", UriKind.Relative);
					break;
			}
			Dispatcher.BeginInvoke((Action)(() => imgStatus.Source = new BitmapImage(uriSource)));
		}
		public void FuncWorkerReset(bool isNeedCancel = true) {
			if (isNeedCancel) {
				if (this.Worker != null) {
					// バックグラウンド処理をキャンセルする
					this.FuncChangeImgStatus(1);
					this.Worker.CancelAsync();
					do {
						Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec * 2)));
					} while (!this.Worker.CancellationPending);
				}
			}
			this.Worker = new BackgroundWorker();
			if (_RecogAPI == "GoogleCloudSpeechAPI") {
				this.Worker.DoWork += new DoWorkEventHandler(Worker_DoWork_GCS_Recog);
			} else if (_RecogAPI == "BingSpeechAPI") {
				this.Worker.DoWork += new DoWorkEventHandler(Worker_DoWork_BingSpeech_Recog);
			} else {
				this.Worker.DoWork += new DoWorkEventHandler(Worker_DoWork_MS_Recog);
			}
			this.Worker.ProgressChanged += new ProgressChangedEventHandler(Worker_ProgressChanged);
			this.Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
			this.Worker.WorkerReportsProgress = true;
			this.Worker.WorkerSupportsCancellation = true;
			this.Worker.RunWorkerAsync();
		}

		private void cmbInputDevice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbInputDevice, ref _InputDevice, "InputDevice");
			// バックグラウンド処理をキャンセルする
			if (this.Worker != null) this.FuncWorkerReset();
		}
		private void cmbOutputDevice_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbOutputDevice, ref _OutputDevice, "OutputDevice");
			MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
			foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active)) {
				if (device.FriendlyName.Contains(_OutputDevice)) {
					_outdevice = device.ID;
				}
			}
		}
		private void cmbRecogAPI_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbRecogAPI, ref _RecogAPI, "RecogAPI");
			// APIが使えるかチェックする
			_funcGoogleCloudSpeechinit(_keyGCSAPIjsonPath);
			// バックグラウンド処理をキャンセルする
			if (this.Worker != null) this.FuncWorkerReset();
		}
		private void cmbSpeechAPI_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbSpeechAPI, ref _SpeechAPI, "SpeechAPI");
		}
		private void cmbMsVolume_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbMsVolume, ref _say_msVolume, "say_msVolume", true);
		}
		private void cmbMsPitch_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbMsPitch, ref _say_msPitch, "say_msPitch", true);
		}
		private void cmbMsEmphasis_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbMsEmphasis, ref _say_msEmphasis, "say_msEmphasis", true);
		}
		private void cmbMsRate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbMsRate, ref _say_msRate, "say_msRate", true);
		}
		private void cmbVTVolume_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbVTVolume, ref _sayVolume, "sayVolume", true);
		}
		private void cmbVTPitch_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbVTPitch, ref _sayPitch, "sayPitch", true);
		}
		private void cmbVTSpeed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbVTSpeed, ref _saySpeed, "saySpeed", true);
		}
		private void cmbVTEmotion_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbVTEmotion, ref _sayEmotion, "sayEmotion", true);
		}
		private void cmbTranslate_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbTranslate, ref _TranslateAPI, "TranslateAPI");
			switch (_TranslateAPI) {
				case "GoogleTranslatorAPI":
					switch (_TranslateSetting) {
						case "Eng→Jpn":
							_recog_lang_set = "en-US";
							break;
						case "Jpn→Eng":
							_recog_lang_set = "ja-JP";
							break;
					}
					break;
				default:
					switch (_TranslateSetting) {
						case "Eng→Jpn":
							_recog_lang_set = "en-US";
							break;
						case "Jpn→Eng":
							_recog_lang_set = "ja-JP";
							break;
					}
					break;
			}
		}
		private void cmbTranslateSetting_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
			this.FuncCombChangeSave(cmbTranslateSetting, ref _TranslateSetting, "TranslateSetting");
			switch (_TranslateSetting) {
				case "Eng→Jpn":
					_recog_lang_set = "en-US";
					break;
				case "Jpn→Eng":
					_recog_lang_set = "ja-JP";
					break;
			}
			// バックグラウンド処理をキャンセルする
			if (this.Worker != null) this.FuncWorkerReset();
		}
		private void txtbPhrases_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
			var lstPhrases = new List<string>();
			try {
				var tmpStrPhrase = txtbPhrases.Text.Trim();
				string[] aryPhrases = tmpStrPhrase.Split('|');
				foreach (string str in aryPhrases) {
					if (str.Trim() != "") {
						lstPhrases.Add(str.Trim());
					}
				}
				_lstPhrases = lstPhrases;
				_Phrases = String.Join("|", _lstPhrases);
				Properties.Settings.Default.Phrases = _Phrases;
				Properties.Settings.Default.Save();
				txtbRecogStatus.Text = "単語辞書 編集中 (「|」区切り、反映まで最大50秒) 単語数 : " + _lstPhrases.Count + "";
				if (_ms_recogEngine != null) {
					_ms_recogEngine.UnloadAllGrammars();
					DictationGrammar customDictationGrammar = new DictationGrammar("grammar:dictation");
					customDictationGrammar.Name = "custom";
					customDictationGrammar.Enabled = true;
					_ms_recogEngine.LoadGrammar(customDictationGrammar);
					foreach (var str in _lstPhrases) {
						customDictationGrammar.SetDictationContext(str, null);
					}
				}
			} catch (Exception w_e4) {
				txtbRecogStatus.Text = "単語辞書編集中...";
				FuncWriteLogFile(w_e4.ToString());
			}
		}
		private void btnGCSAPI_Click(object sender, RoutedEventArgs e) {
			try {
				OpenFileDialog openFileDialog = new OpenFileDialog();
				bool? result = openFileDialog.ShowDialog();
				if (result == true) {
					var jsonpath = openFileDialog.FileName;
					if (FuncGoogleCloudSpeechAPIisEnable(jsonpath).Result) {
						MessageBox.Show("GoogleCloudSpeechAPIキーの認証に\"成功\"しました。");
						_funcGoogleCloudSpeechinit(jsonpath);
						_keyGCSAPIjsonPath = jsonpath;
						Properties.Settings.Default.keyGCSAPIjsonPath = _keyGCSAPIjsonPath;
						Properties.Settings.Default.Save();
					} else {
						MessageBox.Show("GoogleCloudSpeechAPIキーの認証に失敗しました。");
					}
				}
			} catch (Exception e5) {
				MessageBox.Show(e5.ToString());
			}
		}
		private void btnVTWAPIkey_Click(object sender, RoutedEventArgs e) {
			try {
				string pw = txtbVTWAPIkey.Password;
				if (FuncVoiceTextWebAPIisEnable(pw).Result) {
					MessageBox.Show("HOYA VoiceTextWebAPIキーの認証に\"成功\"しました。");
					_funcVoicetextinit(pw);
					_keyVTWAPI = txtbVTWAPIkey.Password.Trim();
					Properties.Settings.Default.keyVTWAPI = _keyVTWAPI;
					Properties.Settings.Default.Save();
				} else {
					MessageBox.Show("HOYA VoiceTextWebAPIキーの認証に失敗しました。");
				}
			} catch (Exception e5) {
				MessageBox.Show(e5.ToString());
			}
		}
		private void btnGTAPIkey_Click(object sender, RoutedEventArgs e) {
			try {
				string pw = txtbGTAPIkey.Password;
				;
				if (FuncTranslateTextToSpeech("test", pw).Result) {
					MessageBox.Show("Google Translator APIキーの認証に\"成功\"しました。");
					_keyGTAPI = pw.Trim();
					_funcGoogleTranslatorinit(_keyGTAPI);
					Properties.Settings.Default.keyGTAPI = _keyGTAPI;
					Properties.Settings.Default.Save();
				} else {
					MessageBox.Show("Google Translator APIキーの認証に失敗しました。");
				}
			} catch (Exception e5) {
				MessageBox.Show("Google Translator APIキーの認証に失敗しました。");
			}
		}
		private void btnBingSpAPIkey_Click(object sender, RoutedEventArgs e) {
			try {
				string pw = txtbBingSpAPIkey.Password;
				;
				if (FuncBingSpeechAPIisEnable(pw).Result) {
					MessageBox.Show("Bing Speech APIキーの認証に\"成功\"しました。");
					_keyBingSAPI1 = pw.Trim();
					_funcBingSpeechinit(_keyBingSAPI1);
					Properties.Settings.Default.keyBingSpAPI1 = _keyBingSAPI1;
					Properties.Settings.Default.Save();
				} else {
					MessageBox.Show("Bing Speech APIキーの認証に失敗しました。");
				}
			} catch (Exception e5) {
				MessageBox.Show("Bing Speech APIキーの認証に失敗しました。");
			}
		}
		private void chkOutputText_Checked(object sender, RoutedEventArgs e) {
			_OutputText = true;
			Properties.Settings.Default.OutputText = _OutputText;
			Properties.Settings.Default.Save();
		}
		private void chkOutputText_Unchecked(object sender, RoutedEventArgs e) {
			_OutputText = false;
			Properties.Settings.Default.OutputText = _OutputText;
			Properties.Settings.Default.Save();
		}
		private void chkOutputText_opt1_Checked(object sender, RoutedEventArgs e) {
			_OutputText_opt1 = true;
			Properties.Settings.Default.OutputText_opt1 = _OutputText_opt1;
			Properties.Settings.Default.Save();
		}
		private void chkOutputText_opt1_Unchecked(object sender, RoutedEventArgs e) {
			_OutputText_opt1 = false;
			Properties.Settings.Default.OutputText_opt1 = _OutputText_opt1;
			Properties.Settings.Default.Save();
		}
		private void chkReadLogFile_Checked(object sender, RoutedEventArgs e) {
			_readLogFile = true;
			Properties.Settings.Default.readLogFile = _readLogFile;
			Properties.Settings.Default.Save();
			if (0 < _readLogFilePath.Length && File.Exists(_readLogFilePath)) {
				try {
					_readLogFileExcept = txtReadLogFileExcept.Text;
					_readLogFileWatcher = new FileSystemWatcher();
					_readLogFileWatcher.Path = Path.GetDirectoryName(_readLogFilePath);
					_readLogFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
					_readLogFileWatcher.Filter = Path.GetFileName(_readLogFilePath);
					_readLogFileWatcher.Changed += FuncReadLogFile;
					_iReadLogFileCount = 0;
					foreach (var strline in File.ReadLines(_readLogFilePath)) {
						_iReadLogFileCount++;
					}
					//監視を開始する
					_readLogFileWatcher.EnableRaisingEvents = true;
				} catch (Exception) {
				}
			}
		}
		private void chkReadLogFile_Unchecked(object sender, RoutedEventArgs e) {
			_readLogFile = false;
			Properties.Settings.Default.readLogFile = _readLogFile;
			Properties.Settings.Default.Save();
			if (_readLogFileWatcher != null) {
				try {
					//監視を終了
					_readLogFileWatcher.EnableRaisingEvents = false;
					_readLogFileWatcher.Changed -= FuncReadLogFile;
				} finally {
					if (_readLogFileWatcher != null) {
						_readLogFileWatcher.Dispose();
						_readLogFileWatcher = null;
					}
				}
			}
		}
		private void btnReadLogFile_Click(object sender, RoutedEventArgs e) {
			// ログ読み上げファイルの場所を確定する
			try {
				OpenFileDialog openFileDialog = new OpenFileDialog();
				openFileDialog.Filter = "Text Files (.txt, .log)|*.txt;*.log|All Files (*.*)|*.*";
				bool? result = openFileDialog.ShowDialog();
				if (result == true) {
					var readlogfilepath = openFileDialog.FileName;
					MessageBox.Show("読み上げ用ログファイルを確定しました。");
					_readLogFilePath = readlogfilepath;
					txtReadLogFile.Text = _readLogFilePath;
					Properties.Settings.Default.readLogFilePath = _readLogFilePath;
					Properties.Settings.Default.Save();
				} else {
					MessageBox.Show("読み上げ用ログファイルの読み取りに失敗しました。");
				}
			} catch (Exception e5) {
				MessageBox.Show(e5.ToString());
			}
		}
		private void FuncReadLogFile(object sender, FileSystemEventArgs e) {
			DateTime tmpLastWriteTime = File.GetLastWriteTime(e.FullPath);
			if (tmpLastWriteTime != _readLogFileLastWriteFileTime) {
				_readLogFileLastWriteFileTime = tmpLastWriteTime;
			} else {
				return;
			}
			switch (e.ChangeType) {
				case WatcherChangeTypes.Changed:
					var strLogFilePath = e.FullPath;
					if (!File.Exists(strLogFilePath)) return;
					int iTemp = 0 - _iReadLogFileCount;
					foreach (var strline in File.ReadLines(_readLogFilePath)) {
						iTemp++;
						if (0 < iTemp) {
							var msg = strline;
							var tmpnickname = "";
							try {
								msg = strline.Substring(strline.IndexOf(_readLogFileExcept));
								// nicknameを取得して声を変えてみる
								if (_readLogFileNicknameChk) {
									var nicknametext = _readLogFileNicknameString;
									if (0 < nicknametext.Length) {
										if (Regex.IsMatch(nicknametext, @"\*")) {
											string[] sepMark = nicknametext.Split('*');
											tmpnickname = strline.Substring(strline.LastIndexOf(sepMark[0]) + sepMark[0].Length, strline.LastIndexOf(sepMark[1]) - strline.LastIndexOf(sepMark[0]) - sepMark[0].Length);
											if (!_lstNicknames.Contains(tmpnickname)) {
												_lstNicknames.Add(tmpnickname);
												// 音声をランダムで決定する
												var lstRandomSpeechAPI = new List<string>(_lstMsActors);
												if (_readLogFileNicknameChkUseVTWAPI && 0 < _keyVTWAPI.Length) {
													lstRandomSpeechAPI.AddRange(_lstVTActors);
												}
												int tmppickno = new Random(DateTime.Now.Millisecond).Next(lstRandomSpeechAPI.Count);
												var tmppickedSpeechAPI = lstRandomSpeechAPI[tmppickno];
												Dictionary<string, string> tmpdic = new Dictionary<string, string>();
												if (_lstMsActors.Contains(tmppickedSpeechAPI)) {
													tmpdic.Add("_SpeechAPI", _lstMsActors[0]);
													tmpdic.Add("_say_msPitch", cmbMsPitch.Items[new Random().Next(1,cmbMsPitch.Items.Count)].ToString());
													//tmpdic.Add("_say_msEmphasis", cmbMsEmphasis.Items[new Random().Next(1,cmbMsEmphasis.Items.Count)].ToString());
													tmpdic.Add("_say_msRate", cmbMsRate.Items[new Random().Next(1,cmbMsRate.Items.Count)].ToString());
													tmpdic.Add("_sayPitch", "");
													tmpdic.Add("_saySpeed", "");
													tmpdic.Add("_sayEmotion", "");
												} else {
													tmpdic.Add("_SpeechAPI", _lstVTActors[new Random().Next(_lstVTActors.Count)]);
													tmpdic.Add("_say_msPitch", "");
													tmpdic.Add("_say_msEmphasis", "");
													tmpdic.Add("_say_msRate", "");
													tmpdic.Add("_sayPitch", cmbVTPitch.Items[new Random().Next(1,cmbVTPitch.Items.Count)].ToString());
													//tmpdic.Add("_saySpeed", cmbVTSpeed.Items[new Random().Next(1,cmbVTSpeed.Items.Count)].ToString());
													tmpdic.Add("_sayEmotion", cmbVTEmotion.Items[new Random().Next(1,cmbVTEmotion.Items.Count)].ToString());
												}
												_lstNicknameOptions.Add(tmpnickname, tmpdic);
											}
										}
									}
								}
							} catch (Exception) {
							}
							if (_readLogFileNicknameChk) {
								if (tmpnickname != "") {
									var dic = _lstNicknameOptions[tmpnickname];
									FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), msg, dic["_SpeechAPI"], _say_msVolume, dic["_say_msPitch"], _say_msEmphasis, dic["_say_msRate"], dic["_sayPitch"], _saySpeed, _sayVolume, dic["_sayEmotion"]);
								} else {
									FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), msg, "Microsoft Haruka Desktop", _say_msVolume, "1", "Reduced", "Slow", _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
								}
							} else {
								FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), msg, "Microsoft Haruka Desktop", _say_msVolume, "1", "Reduced", "Slow", _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
							}
							_iReadLogFileCount++;
						}
					}
					break;
				default:
					break;
			}
		}
		private void txtReadLogFileExcept_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
			_readLogFileExcept = txtReadLogFileExcept.Text;
			Properties.Settings.Default.readLogFileExcept = _readLogFileExcept;
			Properties.Settings.Default.Save();
		}
		private void chkReadLogFileNicknameChk_Checked(object sender, RoutedEventArgs e) {
			_readLogFileNicknameChk = true;
			Properties.Settings.Default.readLogFileNicknameChk = _readLogFileNicknameChk;
			Properties.Settings.Default.Save();
		}
		private void chkReadLogFileNicknameChk_Unchecked(object sender, RoutedEventArgs e) {
			_readLogFileNicknameChk = false;
			Properties.Settings.Default.readLogFileNicknameChk = _readLogFileNicknameChk;
			Properties.Settings.Default.Save();
		}
		private void txtReadLogFileNicknameString_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
			_readLogFileNicknameString = txtReadLogFileNicknameString.Text;
			Properties.Settings.Default.readLogFileNicknameString = _readLogFileNicknameString;
			Properties.Settings.Default.Save();
		}
		private void chkReadLogFileNicknameChkUseVTWAPI_Checked(object sender, RoutedEventArgs e) {
			_readLogFileNicknameChkUseVTWAPI = true;
			Properties.Settings.Default.readLogFileNicknameChkUseVTWAPI = _readLogFileNicknameChkUseVTWAPI;
			Properties.Settings.Default.Save();
		}
		private void chkReadLogFileNicknameChkUseVTWAPI_Unchecked(object sender, RoutedEventArgs e) {
			_readLogFileNicknameChkUseVTWAPI = false;
			Properties.Settings.Default.readLogFileNicknameChkUseVTWAPI = _readLogFileNicknameChkUseVTWAPI;
			Properties.Settings.Default.Save();
		}



		void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
			//txtbRecogStatus.Text = e.ProgressPercentage.ToString();
		}
		void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
			if (e.Cancelled) {
				this.FuncChangeImgStatus(2);
			} else {
				this.FuncChangeImgStatus(1);
			}
		}
		// MS音声認識API
		void Worker_DoWork_MS_Recog(object sender, DoWorkEventArgs e) {
			Dispatcher.BeginInvoke((Action)(() => {
				txtbRecogStatus.Text = "Waiting...";
				this.FuncChangeImgStatus(0);
			}));
			try {
				int iTmpThreadNo = 10;
				this.Worker.ReportProgress(iTmpThreadNo);

				_ms_wi = null;
				_ms_ss = null;
				_ms_wloop = null;
				_ms_wloop_ss = null;
				if (WaveIn.DeviceCount == MainWindow.InputDevice) {
					// ループバックの場合
					MMDevice outdevice = null;
					outdevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); // 既定の出力
					_ms_wloop = new WasapiLoopbackCapture(outdevice);
					//_ms_wloop = new WasapiLoopbackCapture(new MMDeviceEnumerator().GetDevice(MainWindow.MMDoutputDevice));
					//WaveFormat format = new WaveFormat(8000, 16, 1);
					//_ms_writer = new WaveFileWriter(Environment.CurrentDirectory + "\\kikisen-vc.wav", format);
					_ms_wloop.DataAvailable += _ms_wi_DataAvailable;
					_ms_wloop.ShareMode = AudioClientShareMode.Shared;
					_ms_wloop_ss = new SpeechStreamer(100000);

					resampler = new WdlResampler();
					resampler.SetMode(true, 2, false);
					resampler.SetFilterParms();
					resampler.SetFeedMode(true); // input driven
					resampler.SetRates(_ms_wloop.WaveFormat.SampleRate, 16000);

					_ms_wloop.StartRecording();

				} else {
					_ms_wi = new WaveIn(WaveCallbackInfo.FunctionCallback());
					_ms_wi.WaveFormat = new WaveFormat(16000, 16, 1);
					_ms_wi.DeviceNumber = cmbInputDevice.Items.IndexOf(_InputDevice);
					_ms_wi.DataAvailable += _ms_wi_DataAvailable;
					_ms_wi.StartRecording();
					_ms_ss = new SpeechStreamer(100000);
				}

				var culture = (_recog_lang_set == "en-US") ? "en-US" : _recog_lang_set;
				_ms_recogEngine = new SpeechRecognitionEngine(new CultureInfo(culture));
				DictationGrammar customDictationGrammar = new DictationGrammar("grammar:dictation");
				customDictationGrammar.Name = "custom";
				customDictationGrammar.Enabled = true;
				_ms_recogEngine.LoadGrammar(customDictationGrammar);
				foreach (var str in _lstPhrases) {
					customDictationGrammar.SetDictationContext(str, null);
				}

				if (_ms_wloop != null) {
					_ms_recogEngine.SetInputToAudioStream(_ms_wloop_ss, _in_safi);
				} else {
					_ms_recogEngine.SetInputToAudioStream(_ms_ss, _in_safi);
				}
				_ms_recogEngine.SpeechRecognized += _ms_recogEngine_SpeechRecognized;
				_ms_recogEngine.SpeechHypothesized += _ms_recogEngine_SpeechHypothesized;
				_ms_recogEngine.RecognizeAsync(RecognizeMode.Multiple);

				do {
					if (this.Worker.CancellationPending) {
						e.Cancel = true;
						break;
					}
					this.FuncChangeImgStatus(0);
					Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec)));
				} while (true);

				if (_ms_wloop != null) {
					_ms_wloop.StopRecording();
					_ms_wloop.DataAvailable -= _ms_wi_DataAvailable;
					//_ms_writer.Close();
					//_ms_writer = null;
					_ms_wloop_ss.Close();
					resampler.Reset();
				} else {
					_ms_wi.StopRecording();
					_ms_wi.DataAvailable -= _ms_wi_DataAvailable;
					_ms_ss.Close();
				}
				_ms_recogEngine.RecognizeAsyncCancel();
				_ms_recogEngine.SpeechHypothesized -= _ms_recogEngine_SpeechHypothesized;
				_ms_recogEngine.SpeechRecognized -= _ms_recogEngine_SpeechRecognized;
				_ms_recogEngine.UnloadAllGrammars();
				_ms_recogEngine.Dispose();
				if (_ms_wloop != null) {
					_ms_wloop.Dispose();
				} else {
					_ms_wi.Dispose();
				}
				_ms_recogEngine = null;

			} catch (NullReferenceException w_e) {
				FuncWriteLogFile(w_e.ToString());
				this.FuncChangeImgStatus(2);
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
				this.FuncChangeImgStatus(2);
			}
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
		// 匿名デリゲートだとエラーが出るので外だし
		void _ms_wi_DataAvailable(object sender, WaveInEventArgs e) {
			try {
				if (_ms_wloop != null) {
					byte[] output = Convert16(e.Buffer, e.BytesRecorded, _ms_wloop.WaveFormat);
					_ms_wloop_ss.WriteLoop(output, 0, output.Length);
					//_ms_writer.Write(output, 0, output.Length);
				} else {
					_ms_ss.Write(e.Buffer, 0, e.BytesRecorded);
				}
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
				this.FuncChangeImgStatus(2);
			}
		}
		// 推定時の処理
		void _ms_recogEngine_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e2) {
		}
		// 認識時の処理
		void _ms_recogEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e2) {
			if (0.3 < e2.Result.Confidence) {
				Dispatcher.BeginInvoke((Action)(() => {
					var msg = e2.Result.Text;
					if (msg.Contains("FFF")) return;
					if (_TranslateAPI == "GoogleTranslatorAPI") {
						if (_recog_lang_set == "ja-JP") {
							txtbRecogStatus.Text = msg + "[" + e2.Result.Confidence + "]";
							FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), msg, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
						} else {
							FuncTranslateTextToSpeech(msg);
						}
					} else {
						txtbRecogStatus.Text = msg + "[" + e2.Result.Confidence + "]";
						FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), msg, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
					}
				}));
			}
		}
		// GoogleCloudSpeechAPI
		void Worker_DoWork_GCS_Recog(object sender, DoWorkEventArgs e) {
			try {
				// キャンセルトークンの取得
				_tokenGCSAPIcancelTokenS = new CancellationTokenSource();
				CancellationToken cToken = _tokenGCSAPIcancelTokenS.Token;

				System.Timers.Timer timer = null;
				IAudioRecorder recorder = null;

				var credential = GoogleCredential.FromJson(_strGCSAPIJson);
				credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
				var channel = new Channel("speech.googleapis.com:443", credential.ToChannelCredentials());
				var context = new SpeechContext();
				var streamingConfig = new StreamingRecognitionConfig {
					Config = new RecognitionConfig {
						SampleRateHertz = 16000,
						//SampleRateHertz = 8000,
						Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
						LanguageCode = _recog_lang_set,
					},
					InterimResults = true, // 逐次報告、しないとしゃべり放題ならず
					SingleUtterance = false, // 50秒喋り放題
				};
				List<string> lstLastspeaktext = new List<string>();
				while (!this.Worker.CancellationPending && !cToken.IsCancellationRequested) {
					if (this.Worker.CancellationPending || cToken.IsCancellationRequested) {
						e.Cancel = true;
						break;
					}
					context.Phrases.Clear();
					context.Phrases.AddRange(_lstPhrases);
					streamingConfig.Config.SpeechContexts.Clear();
					streamingConfig.Config.SpeechContexts.Add(context);
					if (WaveIn.DeviceCount == MainWindow.InputDevice) {
						// ループバックの場合
						//streamingConfig.Config.LanguageCode = "en-US";
					}
					var client = new Speech.SpeechClient(channel);
					using (var call = client.StreamingRecognize()) {
						var responseReaderTask = Task.Run(async () => {
							string outtext = "";
							string lastknowntext = "";
							string lastspeaktext = "";
							bool bSpeaked = false;
							bool bReset = false;
							while (await call.ResponseStream.MoveNext()) {
								if (this.Worker.CancellationPending || cToken.IsCancellationRequested) {
									break;
								}
								var note = call.ResponseStream.Current;
								if (note.Results != null && note.Results.Count > 0 && note.Results[0].Alternatives.Count > 0) {
									outtext = note.Results[0].Alternatives[0].Transcript;
									Action<bool> act2 = delegate (bool bContainAlpha) {
										if (!bSpeaked) {
											var speechtxt = outtext;
											speechtxt = Regex.Replace(speechtxt, @"\s", "");
											if (0 < lastspeaktext.Length) {
												// 前回の発言内容とスペースを除去
												try {
													var iLeng = speechtxt.Length;
													foreach (var tmpStr in lstLastspeaktext) {
														speechtxt = speechtxt.Replace(tmpStr, "");
													}
													if (iLeng == speechtxt.Length) {
														if (1 <= lastspeaktext.Length) speechtxt = speechtxt.Substring(lastspeaktext.Length, speechtxt.Length - lastspeaktext.Length);
													}
												} catch (Exception w_e4) {
													if (1 <= lastspeaktext.Length) speechtxt = speechtxt.Replace(lastspeaktext, "");
												}
												speechtxt = Regex.Replace(speechtxt, @"\s", "");
											}
											// アルファベット混じりだと発声が遅れるので処置する
											if (bContainAlpha) {
												// 信頼度がある程度以上ならば区切りと判断
												float stable_stability = 0.7f;
												if (stable_stability <= note.Results[0].Stability) {
													var subtext = "";
													if (1 < note.Results.Count) {
														subtext = note.Results[1].Alternatives[0].Transcript;
														if (0 < subtext.Length) {
															subtext = Regex.Replace(subtext, @"\s", "");
															speechtxt += subtext;
														}
													}
													// 前回の発言内容とスペースを除去
													try {
														var iLeng = speechtxt.Length;
														foreach (var tmpStr in lstLastspeaktext) {
															speechtxt = speechtxt.Replace(tmpStr, "");
														}
														if (iLeng == speechtxt.Length) {
															if (1 <= lastspeaktext.Length) speechtxt = speechtxt.Substring(lastspeaktext.Length, speechtxt.Length - lastspeaktext.Length);
														}
													} catch (Exception w_e4) {
														if (1 <= lastspeaktext.Length) speechtxt = speechtxt.Replace(lastspeaktext, "");
													}
													speechtxt = Regex.Replace(speechtxt, @"\s", "");
													if (speechtxt.Length == 0) return;
													Dispatcher.BeginInvoke((Action)(() => {
														FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), speechtxt, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
													}));
													lastspeaktext = speechtxt;
													if (10 <= lstLastspeaktext.Count) {
														lstLastspeaktext.RemoveAt(0);
													}
													if (3 < lastspeaktext.Length) {
														lstLastspeaktext.Add(lastspeaktext);
													}
													bSpeaked = true;
													//txtbRecogStatus.Text = lastspeaktext;
													//bReset = true;
												}
											} else {
												if (Regex.IsMatch(speechtxt, @"[a-zA-Z]")) {
													return;
												}
												if (speechtxt.Length == 0) return;
												Dispatcher.BeginInvoke((Action)(() => {
													FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), speechtxt, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
												}));
												lastspeaktext = speechtxt;
												if (10 <= lstLastspeaktext.Count) {
													lstLastspeaktext.RemoveAt(0);
												}
												if (3 < lastspeaktext.Length) {
													lstLastspeaktext.Add(lastspeaktext);
												}
												bSpeaked = true;
												// 暫定処置 バッファが長くなりすぎたらリセットする
												//if (40 < lastspeaktext.Length) {
												//	if (WaveIn.DeviceCount == MainWindow.InputDevice) {
												//	} else {
												//		bReset = true;
												//	}
												//}
											}
										}
									};
									await Dispatcher.BeginInvoke((Action)(() => {
										if (_recog_lang_set == "ja-JP") {
											txtbRecogStatus.Text = outtext + "[" + note.Results[0].Stability + "]";
											if (outtext.EndsWith(@"リセット")) {
												// リセット用の音声コマンドが文末にあればリセット
												if (WaveIn.DeviceCount == MainWindow.InputDevice) {
												} else {
													bReset = true;
												}
											} else {
												if (lastspeaktext != outtext) {
													bSpeaked = false;
												}
												bool bContainAlpha = false;
												if (lastknowntext != outtext) {
													lastknowntext = outtext;
													// 英字混じりだとなかなか発言しない場合があるので途中で喋らせる
													if (Regex.IsMatch(outtext, @"[a-zA-Z]")) {
														bContainAlpha = true;
													}
													if (note.Results[0].IsFinal || Regex.IsMatch(outtext, @"[a-zA-Z]")) {
														act2(bContainAlpha);
													}
												} else {
													act2(bContainAlpha);
													//if (bContainAlpha) {
													//	bReset = true;
													//}
												}
											}
										}
										// 日本語以外の場合
										if (_recog_lang_set != "ja-JP") {
											if (lastspeaktext != outtext) {
												bSpeaked = false;
											}
											if (!bSpeaked) {
												var speechtxt = outtext;
												if (0.8 < note.Results[0].Stability) {
													if (0 < lastspeaktext.Length) {
													}
													var subtext = "";
													if (1 < note.Results.Count) {
														subtext = note.Results[1].Alternatives[0].Transcript;
														if (0 < subtext.Length) {
															speechtxt += subtext;
														}
													}
													// 前回の発言内容を除去
													try {
														if (10 < lstLastspeaktext.Count) lstLastspeaktext.Clear();
														foreach (var tmpStr in lstLastspeaktext) {
															speechtxt = speechtxt.Replace(tmpStr, "");
														}
													} catch (Exception w_e4) {
														FuncWriteLogFile(w_e4.ToString());
													}
													Dispatcher.BeginInvoke((Action)(() => {
														if (_TranslateAPI == "GoogleTranslatorAPI") {
															FuncTranslateTextToSpeech(speechtxt);
														} else {
															//this.FuncWriteTextLog(speechtxt + "[" + note.Results[0].Stability + "]");
															FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), speechtxt, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
														}
													}));
													lastspeaktext = outtext + subtext;
													lstLastspeaktext.Add(lastspeaktext);
													//txtbRecogStatus.Text = outtext + "[" + note.Results[0].Stability + "]";
													bSpeaked = true;
												}
											}
											// 暫定処置 バッファが長くなりすぎたらリセットする
											//if (50 < lastspeaktext.Length) {
											//	bReset = true;
											//}
										}
									}));
									if (bReset) {
										bReset = false;
										break;
									}
								}
							}
						}, cToken);
						var initialRequest = new StreamingRecognizeRequest();
						initialRequest.StreamingConfig = streamingConfig;
						call.RequestStream.WriteAsync(initialRequest).Wait();
						Dispatcher.BeginInvoke((Action)(() => {
							txtbRecogStatus.Text = "Waiting...";
							this.FuncChangeImgStatus(0);
						}));
						try {
							recorder = new RecordModel();
							recorder.RecordDataAvailabled += (sender2, e2) => {
								if (0 < e2.Length) {
									try {
										lock (recorder) {
											call.RequestStream.WriteAsync(new StreamingRecognizeRequest {
												AudioContent = RecognitionAudio.FromBytes(e2.Buffer, 0, e2.Length).Content,
											}).Wait();
										}
									} catch (InvalidOperationException w_e4) {
										//FuncWriteLogFile(w_e4.ToString());
										FuncWriteLogFile(w_e4.ToString());
									} catch (AggregateException w_e4) {
										// ループバックの場合
										if (WaveIn.DeviceCount == MainWindow.InputDevice) {
											//Console.WriteLine(e3.ToString());
											//throw e3;
										}
										//FuncWriteLogFile(w_e4.ToString());
										w_e4.Handle((w_e6) => {
											FuncWriteLogFile(w_e4.ToString());
											return false;
										});
									}
								}
								if (this.Worker.CancellationPending) {
									recorder.Stop();
									_tokenGCSAPIcancelTokenS.Cancel();
								}
							};
							recorder.Start();
							// Cloud Speech API1回60秒までなので、50秒まできたら打ち切る
							timer = new System.Timers.Timer(1000 * 58);
							timer.Start();
							timer.Elapsed += async (sender2, e2) => {
								try {
									recorder.Stop();
									await call.RequestStream.CompleteAsync();
								} catch (TaskCanceledException w_e4) {
									FuncWriteLogFile(w_e4.ToString());
								}
							};
							responseReaderTask.Wait();
							timer.Stop();
							timer.Dispose();
							if (this.Worker.CancellationPending || _tokenGCSAPIcancelTokenS.IsCancellationRequested) {
								try {
									recorder.Stop();
									_tokenGCSAPIcancelTokenS.Cancel();
									e.Cancel = true;
								} catch (Exception w_e4) {
									FuncWriteLogFile(w_e4.ToString());
								}
							}
						} catch (InvalidOperationException w_e4) {
							FuncWriteLogFile(w_e4.ToString());
							continue;
						} catch (TaskCanceledException w_e4) {
							FuncWriteLogFile(w_e4.ToString());
							recorder.Stop();
							continue;
						} catch (AggregateException w_e4) {
							FuncWriteLogFile(w_e4.ToString());
							continue;
						} catch (Exception w_e4) {
							FuncWriteLogFile(w_e4.ToString());
						} finally {
							//recorder.Dispose();
							Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec)));
						}
					}
					if (this.Worker.CancellationPending || _tokenGCSAPIcancelTokenS.IsCancellationRequested) {
						e.Cancel = true;
						break;
					}
					Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec)));
				}
			} catch (Exception w_e) {
				Dispatcher.BeginInvoke((Action)(() => {
					e.Cancel = true;
					FuncWriteLogFile(w_e.ToString());
				}));
				this.FuncChangeImgStatus(2);
				if (w_e.InnerException != null && w_e.InnerException.Message.Contains("StatusCode=Unauthenticated,")) {
					if (isNowTestingGCS) {
						Dispatcher.BeginInvoke((Action)(() => {
							MessageBox.Show("お試し期間は終了しました。\nGoogleCloudSpeechApiの認証用Jsonファイルをご用意ください。\n詳しくは\n「Google Speech APIを使えるようになるまで Qiita」\n辺りで検索してください。");
							txtbRecogStatus.Text = "お試し期間は終了しました。GoogleCloudSpeechApiの認証用Jsonファイルをご用意ください";
							lblGCSAPI.Text = "未認証(試用終)";
						}));
					} else {
						Dispatcher.BeginInvoke((Action)(() => {
							MessageBox.Show("GoogleCloudSpeechApiの認証に失敗しました。");
							txtbRecogStatus.Text = "GoogleCloudSpeechApiの認証に失敗しました。";
							lblGCSAPI.Text = "未認証";
						}));
					}
				}
			}
		}
		// Bing Speech API
		void Worker_DoWork_BingSpeech_Recog(object sender, DoWorkEventArgs e) {
			try {
				// キャンセルトークンの取得
				_tokenGCSAPIcancelTokenS = new CancellationTokenSource();
				CancellationToken cToken = _tokenGCSAPIcancelTokenS.Token;

				System.Timers.Timer timer;
				while (!this.Worker.CancellationPending && !cToken.IsCancellationRequested) {
					if (this.Worker.CancellationPending || cToken.IsCancellationRequested) {
						e.Cancel = true;
						_tokenGCSAPIcancelTokenS.Cancel();
						break;
					}
					_micClient = SpeechRecognitionServiceFactory.CreateDataClient(SpeechRecognitionMode.LongDictation, _recog_lang_set, _keyBingSAPI1);
					_micClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
					_micClient.OnResponseReceived += this.OnMicDictationResponseReceivedHandler;
					_micClient.OnConversationError += this.OnConversationErrorHandler;
					_micClient.SendAudioFormat(SpeechAudioFormat.create16BitPCMFormat(16000));

					var recorder = new RecordModel();
					recorder.RecordDataAvailabled += (sender2, e2) => {
						if (0 < e2.Length) {
							try {
								lock (recorder) {
									_micClient.SendAudio(e2.Buffer, e2.Length);
								}
							} catch (InvalidOperationException w_e4) {
								FuncWriteLogFile(w_e4.ToString());
							} catch (AggregateException w_e4) {
								FuncWriteLogFile(w_e4.ToString());
							}
						}
						if (this.Worker.CancellationPending) {
							recorder.Stop();
							_tokenGCSAPIcancelTokenS.Cancel();
						}
					};
					recorder.Start();
					Dispatcher.BeginInvoke((Action)(() => {
						txtbRecogStatus.Text = "Waiting...(単語辞書は使用できません)";
						this.FuncChangeImgStatus(0);
					}));

					// Bing Speech API1回15秒までなので、14秒まできたら打ち切る
					timer = new System.Timers.Timer(14500);
					timer.Start();
					timer.Elapsed += (sender2, e2) => {
						try {
							recorder.Stop();
						} catch (TaskCanceledException w_e4) {
							FuncWriteLogFile(w_e4.ToString());
						}
					};
					do {
						if (this.Worker.CancellationPending || cToken.IsCancellationRequested) {
							e.Cancel = true;
							break;
						}
						Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec/4)));
					} while (!recorder.isStoped);

					timer.Stop();
					timer.Dispose();

					Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec/4)));
				}
			} catch (NullReferenceException w_e) {
				FuncWriteLogFile(w_e.ToString());
				this.FuncChangeImgStatus(2);
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
				this.FuncChangeImgStatus(2);
			}
		}
		private void WriteResponseResult(SpeechResponseEventArgs e) {
			if (e.PhraseResponse.Results.Length == 0) {
				return;
			} else {
				var outtext = "";
				for (int i = 0; i < e.PhraseResponse.Results.Length; i++) {
					outtext = e.PhraseResponse.Results[i].DisplayText;
					Dispatcher.BeginInvoke((Action)(() => {
						txtbRecogStatus.Text = outtext;
						FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), outtext, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
					}));
				}
			}
		}
		private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e) {
			//if (e.PartialResult.Length == 0) {
			//	return;
			//} else {
			//	var outtext = "";
			//	for (int i = 0; i < e.PartialResult.Length; i++) {
			//		outtext = e.PartialResult;
			//		Dispatcher.BeginInvoke((Action)(() => {
			//			txtbRecogStatus.Text = outtext + "[推測]";
			//			FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), outtext, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
			//		}));
			//	}
			//}
		}
		private void OnMicDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e) {
			//if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
			//	e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout) {
			//	Dispatcher.Invoke(
			//		(Action)(() => {
			//			_micClient.EndAudio();
			//		})
			//	);
			//}
			this.WriteResponseResult(e);
		}
		private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e) {
			FuncWriteLogFile("["+e.SpeechErrorCode+"]"+e.SpeechErrorText);
			this.FuncChangeImgStatus(2);
		}

		
		private void FuncWriteTextLog(string msg) {
			try {
				if (_OutputText) {
					string appendText = "";
					if (_OutputText_opt1) {
						appendText = DateTime.Now.ToString("[yyyy/MM/dd HH:mm:ss] ");
					}
					appendText = appendText + msg + Environment.NewLine;
					File.AppendAllText(Environment.CurrentDirectory + "\\kikisen-vc-txt.log", appendText);
				}
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
			}
		}
		private async void FuncVoicePlay(int deviceNo, string msg, string actor, string volume = "100", string rate = "0", string emphasis = "", string prate = "",
								   string vt_pitch = "100", string vt_speed = "100", string vt_volume = "100", string vt_emotion = null) {
			try {
				await Task.Run(() => {
					if (_lstMsActors.Contains(actor)) {
						IWaveProvider provider = null;
						using (var stream = new MemoryStream()) {
							using (var synth = new System.Speech.Synthesis.SpeechSynthesizer()) {
								synth.SelectVoice(actor);
								var iVolume = int.Parse(volume);
								iVolume = (100 < iVolume) ? 100 : iVolume;
								iVolume = (iVolume < 50) ? 50 : iVolume;
								synth.Volume = iVolume;
								var iRate = int.Parse(rate);
								iRate = (10 < iRate) ? 10 : iRate;
								iRate = (iRate < -10) ? -10 : iRate;
								synth.Rate = iRate;
								synth.SetOutputToWaveStream(stream);
								synth.SetOutputToAudioStream(stream, _out_safi);
								System.Speech.Synthesis.PromptBuilder builder = new System.Speech.Synthesis.PromptBuilder();
								builder.Culture = CultureInfo.CreateSpecificCulture("ja-JP");
								builder.StartVoice(builder.Culture);
								builder.StartSentence();
								builder.StartStyle(new System.Speech.Synthesis.PromptStyle() { Emphasis = (System.Speech.Synthesis.PromptEmphasis)Enum.Parse(typeof(System.Speech.Synthesis.PromptEmphasis), emphasis, true), Rate = (System.Speech.Synthesis.PromptRate)Enum.Parse(typeof(System.Speech.Synthesis.PromptRate), prate, true) });
								builder.AppendText(msg);
								builder.EndStyle();
								builder.EndSentence();
								builder.EndVoice();
								synth.Speak(builder);
								stream.Seek(0, SeekOrigin.Begin);
								provider = new RawSourceWaveStream(stream, new WaveFormat(44100, 16, 1));
							}

							//outdevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); // 既定の出力
							//using (var wavPlayer = new WasapiOut(_outdevice, AudioClientShareMode.Shared, false, 300)) {
							//	wavPlayer.Init(provider);
							//	wavPlayer.Play();
							//}
							using (WaveOutEvent wavout = new WaveOutEvent()) {
								wavout.DeviceNumber = deviceNo;
								wavout.Init(provider);
								wavout.Play();
								while (wavout.PlaybackState == PlaybackState.Playing) {
									Thread.Sleep(200);
								}
							}
						}
					} else if (_lstVTActors.Contains(actor)) {
						HttpClient http = new HttpClient();
						http.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(_keyVTWAPI)));
						FormUrlEncodedContent encContent = null;
						string[] capableActor = new string[] { "haruka", "hikari", "takeru", "santa", "bear" };
						if (0 <= Array.IndexOf(capableActor, actor)) {
							var tmp_emotion = (vt_emotion == "なし") ? null : vt_emotion;
							if (tmp_emotion != null) {
								encContent = new FormUrlEncodedContent(
									new[]{
									new KeyValuePair<string , string>( "text" , msg),
									new KeyValuePair<string , string>( "speaker" , actor),
									new KeyValuePair<string , string>( "pitch" , vt_pitch),
									new KeyValuePair<string , string>( "speed" , vt_speed),
									new KeyValuePair<string , string>( "volume" , vt_volume),
									new KeyValuePair<string , string>( "emotion" , tmp_emotion),
									new KeyValuePair<string , string>( "emotion_level" , "4"),
									}
								);
							} else {
								encContent = new FormUrlEncodedContent(
									new[]{
									new KeyValuePair<string , string>( "text" , msg),
									new KeyValuePair<string , string>( "speaker" , actor),
									new KeyValuePair<string , string>( "pitch" , vt_pitch),
									new KeyValuePair<string , string>( "speed" , vt_speed),
									new KeyValuePair<string , string>( "volume" , vt_volume),
									}
								);
							}
						} else {
							encContent = new FormUrlEncodedContent(
								new[]{
								new KeyValuePair<string , string>( "text" , msg),
								new KeyValuePair<string , string>( "speaker" , actor),
								new KeyValuePair<string , string>( "pitch" , vt_pitch),
								new KeyValuePair<string , string>( "speed" , vt_speed),
								new KeyValuePair<string , string>( "volume" , vt_volume),
								}
							);
						}
						http.PostAsync("https://api.voicetext.jp/v1/tts", encContent).ContinueWith(async task => {
							var Stream = await task.Result.Content.ReadAsStreamAsync();
							WaveStream mainOutputStream = new WaveFileReader(Stream);
							WaveChannel32 volumeStream = new WaveChannel32(mainOutputStream);
							using (WaveOutEvent wavout = new WaveOutEvent()) {
								wavout.DeviceNumber = deviceNo;
								wavout.Init(volumeStream);
								wavout.Play();
								while (wavout.PlaybackState == PlaybackState.Playing) {
									Thread.Sleep(200);
								}
							}
							//outdevice = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); // 既定の出力
							//using (var wavPlayer = new WasapiOut(_outdevice, AudioClientShareMode.Shared, false, 300)) {
							//	wavPlayer.Init(volumeStream);
							//	wavPlayer.Play();
							//}
						});
					}
					this.FuncWriteTextLog(msg);
				});
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
			}
		}
		public async Task<bool> FuncTranslateTextToSpeech(string msg, string apikey = "") {
			try {
				if (_keyGTAPI == "" && apikey == "") {
					throw new Exception();
				}
				var service = new TranslateService(new BaseClientService.Initializer() {
					ApiKey = _keyGTAPI,
					ApplicationName = "kikisen-VC",
				});
				if (apikey != "") {
					// test connect
					service = new TranslateService(new BaseClientService.Initializer() {
						ApiKey = apikey,
						ApplicationName = "kikisen-VC",
					});
				}

				var srcText = msg;
				var tgtLang = "";
				var srcLang = "";
				if (_recog_lang_set == "ja-JP") {
					tgtLang = "en";
					srcLang = "ja";
				} else {
					tgtLang = "ja";
					srcLang = "en";
				}
				var translatedText = await TranslateTextAsync(service, srcText, srcLang, tgtLang).ConfigureAwait(false);
				Dispatcher.BeginInvoke((Action)(() => {
					txtbRecogStatus.Text = translatedText;
					FuncVoicePlay(cmbOutputDevice.Items.IndexOf(_OutputDevice), translatedText, _SpeechAPI, _say_msVolume, _say_msPitch, _say_msEmphasis, _say_msRate, _sayPitch, _saySpeed, _sayVolume, _sayEmotion);
				}));
				return true;
			} catch (Exception w_e) {
				FuncWriteLogFile(w_e.ToString());
			}
			return false;
		}

		private async Task<bool> FuncGoogleCloudSpeechAPIisEnable(string jsonpath) {
			try {
				if (!File.Exists(jsonpath)) return false;
				var credential = GoogleCredential.FromJson(File.ReadAllText(jsonpath));
				credential = credential.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
				var channel = new Channel("speech.googleapis.com:443", credential.ToChannelCredentials());
				var client = new Speech.SpeechClient(channel);
				var streamingConfig = new StreamingRecognitionConfig {
					Config = new RecognitionConfig { SampleRateHertz = 16000, Encoding = RecognitionConfig.Types.AudioEncoding.Linear16, LanguageCode = _recog_lang_set, }
				};
				using (var call = client.StreamingRecognize()) {
					var initialRequest = new StreamingRecognizeRequest { StreamingConfig = streamingConfig, };
					call.RequestStream.WriteAsync(initialRequest).Wait();
				}
				return true;
			} catch (Exception w_e) {
				if (isNowTestingGCS) {
					if (w_e.InnerException.Message.Contains("StatusCode=Unauthenticated,")) {
						txtbRecogStatus.Text = "お試し期間は終了しました。GoogleCloudSpeechApiの認証用Jsonファイルをご用意ください";
						lblGCSAPI.Text = "未認証";
					}
				}
				FuncWriteLogFile(w_e.ToString());
				return false;
			}
		}
		private async Task<bool> FuncBingSpeechAPIisEnable(string pw) {
			bool result = false;
			try {
				if (pw.Length == 0) return false;
				int counter = 0;
				result = true;
				var micClient = SpeechRecognitionServiceFactory.CreateDataClient(SpeechRecognitionMode.LongDictation, _recog_lang_set, pw);
				micClient.OnConversationError += (sender2, e2) => {
					if ("LoginFailed" == e2.SpeechErrorCode.ToString()) {
						result = false;
					}
					counter++;
				};
				var recorder = new RecordModel();
				recorder.RecordDataAvailabled += (sender2, e2) => {
					if (0 < e2.Length) {
						try {
							lock (recorder) {
								micClient.SendAudio(e2.Buffer, e2.Length);
							}
						} catch (Exception w_e4) {
						}
					}
				};
				recorder.Start();
				do {
					Thread.Sleep(Convert.ToInt32(Math.Round(_threadwaitsec)));
				} while (3 <= counter);
				recorder.Stop();
			} catch (Exception w_e) {
				result = false;
			}
			return result;
		}
		private async Task<bool> FuncVoiceTextWebAPIisEnable(string apikey) {
			if (apikey.Length <= 0) return false;
			HttpClient http = new HttpClient();
			http.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(apikey)));
			FormUrlEncodedContent encContent = new FormUrlEncodedContent(
				new[]{
						new KeyValuePair<string , string>( "text" , "connect test"),
						new KeyValuePair<string , string>( "speaker" , "show")
				}
			);
			var result = await http.PostAsync("https://api.voicetext.jp/v1/tts", encContent).ConfigureAwait(false);
			if (result.Content.Headers.ContentType.MediaType == "audio/wave") {
				return true;
			} else {
				return false;
			}
		}
		private async Task<string> TranslateTextAsync(TranslateService service, string text, string sourceLanguage, string targetLanguage) {
			var request = service.Translations.List(new[] { text }, targetLanguage);
			request.Source = sourceLanguage;
			request.Format = TranslationsResource.ListRequest.FormatEnum.Text;

			var response = await request.ExecuteAsync().ConfigureAwait(false);
			return response.Translations[0].TranslatedText;
		}
	}
}
