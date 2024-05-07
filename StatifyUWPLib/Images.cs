using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using System.Text.Json.Nodes;
using System.Net;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage;
using System.Runtime.InteropServices.WindowsRuntime;

using static StatifyUWPLib.SettingsProvider;

namespace StatifyUWPLib
{
    public class Images
    {
        public static async Task<ImageSource> GetProfilePic()
        {
            if (File.Exists(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\pfp.png"))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\pfp.png");
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    return bitmapImage;
                }
            }
            else
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, "https://api.spotify.com/v1/me");
                request.Headers.Add("Authorization", $"Bearer {AccessToken}");
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string resp = await response.Content.ReadAsStringAsync();
                JsonNode node = JsonNode.Parse(resp);
                Uri imgUri = new Uri(node["images"][0]["url"].ToString());
                (new WebClient()).DownloadFile(imgUri, Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\pfp.png");
                StorageFile file = await StorageFile.GetFileFromPathAsync(Windows.Storage.ApplicationData.Current.LocalFolder.Path + "\\pfp.png");
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    return bitmapImage;
                }
            }
        }

public static async Task<BitmapImage> GetImageByURL(string imageUrl, int maxWidth = 100, int maxHeight = 100)
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(new Uri(imageUrl));

                using (InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream())
                {
                    // Set up scaling & capture bitmap
                    await randomAccessStream.WriteAsync(imageBytes.AsBuffer());
                    randomAccessStream.Seek(0);

                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(randomAccessStream);

                    double originalWidth = bitmapImage.PixelWidth;
                    double originalHeight = bitmapImage.PixelHeight;

                    double scaleX = maxWidth / originalWidth;
                    double scaleY = maxHeight / originalHeight;
                    double scale = Math.Min(scaleX, scaleY);
                    int newWidth = (int)(originalWidth * scale);
                    int newHeight = (int)(originalHeight * scale);

                    // Obtain pixel data and convert it to buffer
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                    BitmapTransform transform = new BitmapTransform { ScaledWidth = (uint)newWidth, ScaledHeight = (uint)newHeight };
                    PixelDataProvider pixelData = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
                    WriteableBitmap resizedBitmap = new WriteableBitmap(newWidth, newHeight);
                    
                    // Set bitmap data
                    using (Stream stream = resizedBitmap.PixelBuffer.AsStream())
                    {
                        IBuffer imageBuffer = pixelData.DetachPixelData().AsBuffer();
                        await stream.WriteAsync(imageBuffer.ToArray(), 0, (int)imageBuffer.Capacity);
                    }

                    // Reencode into BitmapImage
                    using (InMemoryRandomAccessStream resizedStream = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, resizedStream);
                        Stream pixelStream = resizedBitmap.PixelBuffer.AsStream();
                        byte[] pixels = new byte[pixelStream.Length];
                        await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)newWidth, (uint)newHeight, decoder.DpiX, decoder.DpiY, pixels);
                        await encoder.FlushAsync();

                        resizedStream.Seek(0);

                        BitmapImage resizedImage = new BitmapImage();
                        await resizedImage.SetSourceAsync(resizedStream);

                        return resizedImage;
                    }
                }
            }
        }
    }
}
