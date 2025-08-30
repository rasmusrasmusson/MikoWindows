// MikoMe/Services/TtsService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace MikoMe.Services
{
    /// <summary>
    /// Very small Text-to-Speech helper.
    /// Usage: await TtsService.SpeakAsync("你好", "zh-CN");
    /// </summary>
    public static class TtsService
    {
        // Reuse a single player so repeated calls don’t overlap
        private static readonly MediaPlayer _player = new MediaPlayer();

        /// <summary>
        /// Speak <paramref name="text"/> in the specified BCP-47 <paramref name="locale"/>,
        /// e.g. "en-US", "zh-CN". If the exact voice is not found, a best-effort match is used.
        /// </summary>
        public static async Task SpeakAsync(string text, string locale = "en-US")
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            // Stop anything currently playing
            try { _player.Pause(); } catch { /* ignore */ }

            using var synth = new SpeechSynthesizer();

            // Try to pick a voice that matches the requested locale (exact, then language-only)
            try
            {
                var voice =
                    SpeechSynthesizer.AllVoices
                        .FirstOrDefault(v => v.Language.Equals(locale, StringComparison.OrdinalIgnoreCase))
                    ?? SpeechSynthesizer.AllVoices
                        .FirstOrDefault(v =>
                        {
                            var lang = locale.Split('-')[0]; // "en" from "en-US"
                            return v.Language.StartsWith(lang, StringComparison.OrdinalIgnoreCase);
                        });

                if (voice != null)
                    synth.Voice = voice;
            }
            catch
            {
                // If enumerating voices fails, just fall back to the default synthesizer voice.
            }

            // Synthesize and play
            using var stream = await synth.SynthesizeTextToStreamAsync(text);
            _player.Source = MediaSource.CreateFromStream(stream, stream.ContentType);
            _player.Play();
        }

        /// <summary>Immediately stop any ongoing speech.</summary>
        public static void Stop()
        {
            try { _player.Pause(); } catch { /* ignore */ }
            try { _player.Source = null; } catch { /* ignore */ }
        }
    }
}
