using MikoMe.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Ocr;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;
using Windows.Storage.Streams;

namespace MikoMe.Views
{
    public sealed partial class TranslatePage : Page
    {
        private bool _toChinese = false; // false = Chinese->English, true = English->Chinese

        public TranslatePage()
        {
            InitializeComponent();
            UpdateLayoutForMode();
            InputText.TextChanged += InputText_TextChanged; // real-time translation
        }

        private void UpdateLayoutForMode()
        {
            InputHeader.Text = _toChinese ? "English" : "Chinese";
            OutputHeader.Text = _toChinese ? "Chinese" : "English";

            InputText.Text = string.Empty;
            OutputEnglish.Text = string.Empty;
            OutputHanzi.Text = string.Empty;
            OutputPinyin.Text = string.Empty;
            StatusText.Text = string.Empty;

            OutputEnglish.Visibility = _toChinese ? Visibility.Collapsed : Visibility.Visible;
            ZhTargetPanel.Visibility = _toChinese ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            _toChinese = !_toChinese;
            UpdateLayoutForMode();
        }

        // ---- Paste ----
        private async void PasteButton_Click(object sender, RoutedEventArgs e) => await PasteIntoAsync(InputText);

        private async void PasteKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            await PasteIntoAsync(InputText);
        }

        private static OcrEngine CreateBestOcrEngine()
        {
            var zh = new Language("zh-Hans");
            return OcrEngine.TryCreateFromLanguage(zh) ??
                   OcrEngine.TryCreateFromUserProfileLanguages();
        }

        private async Task PasteIntoAsync(TextBox target)
        {
            try
            {
                var data = Clipboard.GetContent();
                if (data == null) { StatusText.Text = "Clipboard is empty."; return; }

                if (data.Contains(StandardDataFormats.Text))
                {
                    target.Text = (await data.GetTextAsync())?.Trim();
                    StatusText.Text = "Pasted text from clipboard.";
                    return;
                }

                if (data.Contains(StandardDataFormats.Bitmap))
                {
                    var bmp = await data.GetBitmapAsync();
                    if (bmp != null)
                    {
                        using IRandomAccessStream stream = await bmp.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        var sb = await decoder.GetSoftwareBitmapAsync();

                        var engine = CreateBestOcrEngine();
                        if (engine == null)
                        {
                            StatusText.Text = "OCR engine not available.";
                            return;
                        }

                        var result = await engine.RecognizeAsync(sb);
                        target.Text = result?.Text?.Trim();
                        StatusText.Text = string.IsNullOrWhiteSpace(target.Text)
                            ? "OCR ran, but no text found."
                            : "Extracted text from image.";
                        return;
                    }
                }

                StatusText.Text = "Clipboard has no text or image.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Paste/OCR failed: {ex.Message}";
            }
        }

        // ---- Real-time translation ----
        private async void InputText_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = (InputText.Text ?? "").Trim();
            if (string.IsNullOrEmpty(input))
            {
                OutputEnglish.Text = "";
                OutputHanzi.Text = "";
                OutputPinyin.Text = "";
                return;
            }

            try
            {
                if (_toChinese)
                {
                    var hanzi = await TranslatorService.TranslateToChineseAsync(input);
                    OutputHanzi.Text = hanzi;
                    OutputPinyin.Text = await TranslatorService.TransliterateToPinyinAsync(hanzi);
                }
                else
                {
                    OutputEnglish.Text = await TranslatorService.TranslateToEnglishAsync(input);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Translate failed: {ex.Message}";
            }
        }

        // ---- Copy buttons ----
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var textToCopy = _toChinese ? OutputHanzi.Text : OutputEnglish.Text;
            if (!string.IsNullOrWhiteSpace(textToCopy))
            {
                var package = new DataPackage();
                package.SetText(textToCopy);
                Clipboard.SetContent(package);
                StatusText.Text = "Copied.";
            }
            else StatusText.Text = "Nothing to copy.";
        }

        private void CopyBox2Button_Click(object sender, RoutedEventArgs e)
        {
            var textToCopy = _toChinese ? OutputPinyin.Text : string.Empty;
            if (!string.IsNullOrWhiteSpace(textToCopy))
            {
                var package = new DataPackage();
                package.SetText(textToCopy);
                Clipboard.SetContent(package);
                StatusText.Text = "Copied.";
            }
            else StatusText.Text = "Nothing to copy.";
        }

        // ---- Speaker ----
        private async void SpeakerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string text = _toChinese ? OutputHanzi.Text : OutputEnglish.Text;
                if (string.IsNullOrWhiteSpace(text))
                {
                    StatusText.Text = "Nothing to speak.";
                    return;
                }

                var synth = new SpeechSynthesizer();
                synth.Voice = _toChinese
                    ? SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Language.StartsWith("zh"))
                    : SpeechSynthesizer.AllVoices.FirstOrDefault(v => v.Language.StartsWith("en"));

                var stream = await synth.SynthesizeTextToStreamAsync(text);
                var player = new MediaPlayer();
                player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
                player.Play();
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Speech failed: {ex.Message}";
            }
        }

        private void SpeakerKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            SpeakerButton_Click(this, new RoutedEventArgs());
        }
    }
}
