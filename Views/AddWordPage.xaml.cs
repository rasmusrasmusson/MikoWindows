using MikoMe.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace MikoMe.Views
{
    public sealed partial class AddWordPage : Page
    {
        public AddWordPage()
        {
            InitializeComponent();
            HanziTextBox.TextChanged += HanziTextBox_TextChanged; // 👈 listen for text changes
        }

        // ---- paste (button + Ctrl+V)
        private async void PasteButton_Click(object sender, RoutedEventArgs e) => await PasteFromClipboardIntoAsync(HanziTextBox);

        private async void PasteKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            await PasteFromClipboardIntoAsync(HanziTextBox);
        }

        private static OcrEngine CreateBestOcrEngine()
        {
            var zh = new Language("zh-Hans");
            return OcrEngine.TryCreateFromLanguage(zh) ??
                   OcrEngine.TryCreateFromUserProfileLanguages();
        }

        private async Task PasteFromClipboardIntoAsync(TextBox target)
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
                        using var stream = await bmp.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        var sb = await decoder.GetSoftwareBitmapAsync();

                        var engine = CreateBestOcrEngine();
                        if (engine == null)
                        {
                            StatusText.Text = "OCR engine not available. Install Chinese (Simplified) OCR in Windows language features.";
                            return;
                        }

                        var result = await engine.RecognizeAsync(sb);
                        target.Text = result?.Text?.Trim();
                        StatusText.Text = string.IsNullOrWhiteSpace(target.Text)
                            ? "OCR ran, but no readable text was found."
                            : "Extracted text from image (OCR).";
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

        // ---- Auto-translate when Hanzi changes ----
        private async void HanziTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hanzi = (HanziTextBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(hanzi))
            {
                PinyinTextBox.Text = "";
                EnglishTextBox.Text = "";
                return;
            }

            try
            {
                // Translate to English
                EnglishTextBox.Text = await TranslatorService.TranslateToEnglishAsync(hanzi);

                // Transliterate to Pinyin
                PinyinTextBox.Text = await TranslatorService.TransliterateToPinyinAsync(hanzi);

                StatusText.Text = "Translation updated.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Translation failed: {ex.Message}";
            }
        }

        // ---- Save (button + Ctrl+Enter)
        private void SaveKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            Save_Click(this, new RoutedEventArgs());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // TODO: put your DB insert here
            StatusText.Text = "Saved.";
        }
    }
}
