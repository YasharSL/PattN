using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp;
using SkiaSharp.HarfBuzz;

namespace v2rayN.Base;

internal class FlagTextBlock : TextBlock
{
    public static readonly DependencyProperty FlagTextProperty = DependencyProperty.Register(
        nameof(FlagText),
        typeof(string),
        typeof(FlagTextBlock),
        new PropertyMetadata(null, OnFlagTextChanged));

    public string? FlagText
    {
        get => (string?)GetValue(FlagTextProperty);
        set => SetValue(FlagTextProperty, value);
    }

    private static void OnFlagTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var block = (FlagTextBlock)d;
        block.Inlines.Clear();
        var text = e.NewValue as string;
        if (text.IsNullOrEmpty())
        {
            return;
        }

        foreach (var part in Split(text))
        {
            if (part.IsFlag && FlagEmojiImages.TryGet(part.Text, out var source))
            {
                var image = new Image
                {
                    Source = source,
                    Height = 16,
                    Stretch = Stretch.Uniform,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                block.Inlines.Add(new InlineUIContainer(image)
                {
                    BaselineAlignment = BaselineAlignment.Center
                });
                continue;
            }

            block.Inlines.Add(new Run(part.Text));
        }
    }

    private static IEnumerable<(string Text, bool IsFlag)> Split(string text)
    {
        var buffer = new System.Text.StringBuilder();
        var index = 0;
        while (index < text.Length)
        {
            if (TryReadFlag(text, index, out var flag))
            {
                if (buffer.Length > 0)
                {
                    yield return (buffer.ToString(), false);
                    buffer.Clear();
                }

                yield return (flag, true);
                index += flag.Length;
                continue;
            }

            buffer.Append(text[index]);
            index++;
        }

        if (buffer.Length > 0)
        {
            yield return (buffer.ToString(), false);
        }
    }

    private static bool TryReadFlag(string text, int index, out string flag)
    {
        flag = string.Empty;
        if (index + 3 >= text.Length || !char.IsHighSurrogate(text[index]) || !char.IsHighSurrogate(text[index + 2]))
        {
            return false;
        }

        var first = char.ConvertToUtf32(text, index);
        var second = char.ConvertToUtf32(text, index + 2);
        if (first is < 0x1F1E6 or > 0x1F1FF || second is < 0x1F1E6 or > 0x1F1FF)
        {
            return false;
        }

        flag = text.Substring(index, 4);
        return true;
    }
}

internal static class FlagEmojiImages
{
    private const int FontSize = 48;
    private static readonly Dictionary<string, ImageSource> Cache = new();
    private static readonly object Gate = new();
    private static SKTypeface? _typeface;
    private static MemoryStream? _fontBytes;

    public static bool TryGet(string flag, out ImageSource source)
    {
        lock (Gate)
        {
            if (Cache.TryGetValue(flag, out source!))
            {
                return true;
            }

            if (!TryRender(flag, out source!))
            {
                return false;
            }

            Cache[flag] = source;
            return true;
        }
    }

    private static bool TryRender(string flag, out ImageSource source)
    {
        source = null!;
        var typeface = Typeface();
        if (typeface == null)
        {
            return false;
        }

        using var font = new SKFont(typeface, FontSize);
        using var shaper = new SKShaper(typeface);
        using var paint = new SKPaint { IsAntialias = true };
        using var bitmap = new SKBitmap(FontSize + 16, FontSize + 8, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawShapedText(shaper, flag, 0, FontSize, SKTextAlign.Left, font, paint);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = new MemoryStream(data.ToArray());
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = stream;
        bitmapImage.EndInit();
        bitmapImage.Freeze();
        source = bitmapImage;
        return true;
    }

    private static SKTypeface? Typeface()
    {
        if (_typeface != null)
        {
            return _typeface;
        }

        var info = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/TwemojiMozilla.ttf"));
        if (info == null)
        {
            return null;
        }

        _fontBytes = new MemoryStream();
        info.Stream.CopyTo(_fontBytes);
        _fontBytes.Position = 0;
        _typeface = SKTypeface.FromStream(_fontBytes);
        return _typeface;
    }
}

internal class FlagTextColumn : MyDGTextColumn
{
    protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
    {
        var block = new FlagTextBlock
        {
            Margin = new Thickness(8, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        if (Binding != null)
        {
            block.SetBinding(FlagTextBlock.FlagTextProperty, Binding);
        }

        return block;
    }
}
