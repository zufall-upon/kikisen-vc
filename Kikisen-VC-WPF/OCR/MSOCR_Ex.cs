using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;

namespace Kikisen_VC_WPF.OCR
{
	public class MSOCR_Ex
	{

		/// <summary>
		/// Microsoft OCR の呼び出し
		/// </summary>
		/// <param name="bitmap"></param>
		/// <returns></returns>
		public async Task<OcrResult> detect(SoftwareBitmap bitmap) {
			var ocrEngine = OcrEngine.TryCreateFromUserProfileLanguages();
			//Language ocrLanguage = new Language("ja");
			//Language ocrLanguage = new Language("en");
			//OcrEngine ocrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage);
			var ocrResult = await ocrEngine.RecognizeAsync(bitmap);
			return ocrResult;
		}

		/// <summary>
		/// ファイルパスを指定して SoftwareBitmap を取得
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public async Task<SoftwareBitmap> LoadImage(string path) {
			var fs = System.IO.File.OpenRead(path);
			var buf = new byte[fs.Length];
			fs.Read(buf, 0, (int)fs.Length);
			var mem = new MemoryStream(buf);
			mem.Position = 0;

			var stream = await ConvertToRandomAccessStream(mem);
			var bitmap = await LoadImage(stream);
			return bitmap;
		}
		public async Task<SoftwareBitmap> LoadImage2(MemoryStream mem) {
			var stream = await ConvertToRandomAccessStream(mem);
			var bitmap = await LoadImage(stream);
			return bitmap;
		}
		/// <summary>
		/// IRandomAccessStream から SoftwareBitmap を取得
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		private async Task<SoftwareBitmap> LoadImage(IRandomAccessStream stream) {
			var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);
			var bitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
			return bitmap;
		}
		/// <summary>
		/// MemoryStream から IRandomAccessStream へ変換
		/// </summary>
		/// <param name="memoryStream"></param>
		/// <returns></returns>
		public async Task<IRandomAccessStream> ConvertToRandomAccessStream(MemoryStream memoryStream) {
			var randomAccessStream = new InMemoryRandomAccessStream();

			var outputStream = randomAccessStream.GetOutputStreamAt(0);
			var dw = new DataWriter(outputStream);
			var task = new Task(() => dw.WriteBytes(memoryStream.ToArray()));
			task.Start();
			await task;
			await dw.StoreAsync();
			await outputStream.FlushAsync();
			return randomAccessStream;
		}
	}
}
