using System.Drawing.Imaging;

namespace ResizeImageApplicationDesktopUIVersion
{

    public partial class StartUpForm : Form
    {
        public StartUpForm()
        {
            InitializeComponent();

            ResizeImageManager resizeImageManager = new ResizeImageManager("test.jpg");
            byte[] originalImageData = resizeImageManager.ReadImageBytes();

            bool isImageRGBA = ResizeImageManager.IsImageRGBA(resizeImageManager.FullLocalImageFilePath);
            // Assuming original dimensions are accessible through ResizeImageManager properties
            int newWidth = resizeImageManager.Width / 3;
            int newHeight = resizeImageManager.Height / 3;

            byte[] downscaledImageData = ResizeImageManager.DownscaleImageUsingAveraging(originalImageData, resizeImageManager.Width, resizeImageManager.Height, newWidth, newHeight, isImageRGBA);

            // To display the downscaled image, you might need to convert the byte array back to an Image object.
            // This step will depend on how you implemented DisplayImageFromBytes. If DisplayImageFromBytes
            // expects raw image bytes (e.g., a BMP byte array), you may need to prepare the data accordingly.
            resizeImageManager.DisplayImageFromBytes(downscaledImageData, newWidth, newHeight, pictureBox1);

        }

        public class ResizeImageManager
        {
            private string fullLocalImageFilePath;

            private int height;

            private int width;

            public ResizeImageManager(string fullLocalImageFilePath)
            {
                this.fullLocalImageFilePath = fullLocalImageFilePath;
            }

            public Color[] GetImageColors()
            {
                byte[] bytes = ReadImageBytes();

                PixelFormat pixelFormat = GetImagePixelFormatValue(fullLocalImageFilePath);

                Color[] colors = pixelFormat == PixelFormat.Format32bppArgb || pixelFormat == PixelFormat.Format64bppArgb ? ConvertBytesToColorsRGBA(bytes/*, *//*pixelFormat == *//*pixelFormat == PixelFormat.Format32bppArgb || pixelFormat == PixelFormat.Format64bppArgb ? 4 : 3*/) : ConvertBytesToColorsRGB(bytes);

                return colors;
            }

            public byte[] ReadImageBytes()
            {
                // Ensure the file exists to avoid FileNotFoundException
                if (!File.Exists(fullLocalImageFilePath))
                {
                    throw new FileNotFoundException("File Not Found.", fullLocalImageFilePath);
                }

                // Read and return the file's bytes
                byte[] bytes = File.ReadAllBytes(fullLocalImageFilePath);

                Image image = Image.FromFile(fullLocalImageFilePath);

                int width = image.Width;

                int height = image.Height;

                this.height = height;

                this.width = width;

                return bytes;
            }

            public Color[] ConvertBytesToColorsRGBA(byte[] imageData)
            {
                int count = 3;

                if (imageData == null || imageData.Length % count != 0)
                {
                    throw new ArgumentException("Image data is null or not a valid RGB OR RGBA format.");
                }

                Color[] colors = new Color[imageData.Length / count];

                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * count;
                    byte blue = imageData[offset];
                    byte green = imageData[offset + 1];
                    byte red = imageData[offset + 2];

                    byte alpha = imageData[offset + 3];

                    colors[i] = Color.FromArgb(alpha, red, green, blue);

                }

                return colors;
            }

            public Color[] ConvertBytesToColorsRGB(byte[] imageData)
            {
                int count = 4;

                if (imageData == null || imageData.Length % /*4*/count != 0)
                {
                    throw new ArgumentException("Image data is null or not a valid RGB OR RGBA format.");
                }

                Color[] colors = new Color[imageData.Length / /*4*/count];

                for (int i = 0; i < colors.Length; i++)
                {
                    int offset = i * /*4*/count;
                    byte blue = imageData[offset];
                    byte green = imageData[offset + 1];
                    byte red = imageData[offset + 2];

                    byte alpha = 255;

                    colors[i] = Color.FromArgb(alpha, red, green, blue);

                }

                return colors;
            }

            public void DisplayImageFromBytes(byte[] imageData, int width, int height, PictureBox pictureBox)
            {
                using (var ms = new MemoryStream(imageData))
                {
                    Image image = Image.FromStream(ms);
                    pictureBox.Height = height;
                    pictureBox.Width = width;
                    pictureBox.Image = image;
                }
            }

            public static byte[] DownscaleImageUsingAveraging(byte[] originalPixels, int originalWidth, int originalHeight, int newWidth, int newHeight, bool isRGBA)
            {
                int bytesPerPixel = isRGBA ? 4 : 3;
                byte[] newPixels = new byte[newWidth * newHeight * bytesPerPixel];

                float xRatio = originalWidth / (float)newWidth;
                float yRatio = originalHeight / (float)newHeight;

                for (int newY = 0; newY < newHeight; newY++)
                {
                    for (int newX = 0; newX < newWidth; newX++)
                    {
                        long sumRed = 0, sumGreen = 0, sumBlue = 0, sumAlpha = 0;
                        int count = 0;

                        int startY = (int)(newY * yRatio);
                        int endY = (int)Math.Min(originalHeight, (newY + 1) * yRatio);
                        for (int y = startY; y < endY; y++)
                        {
                            int startX = (int)(newX * xRatio);
                            int endX = (int)Math.Min(originalWidth, (newX + 1) * xRatio);
                            for (int x = startX; x < endX; x++)
                            {
                                int originalIndex = (y * originalWidth + x) * bytesPerPixel;
                                sumRed += originalPixels[originalIndex];
                                sumGreen += originalPixels[originalIndex + 1];
                                sumBlue += originalPixels[originalIndex + 2];
                                if (isRGBA)
                                {
                                    sumAlpha += originalPixels[originalIndex + 3];
                                }
                                count++;
                            }
                        }

                        byte avgRed = (byte)(sumRed / count);
                        byte avgGreen = (byte)(sumGreen / count);
                        byte avgBlue = (byte)(sumBlue / count);
                        byte avgAlpha = isRGBA ? (byte)(sumAlpha / count) : (byte)255;

                        int newIndex = (newY * newWidth + newX) * bytesPerPixel;
                        newPixels[newIndex] = avgRed;
                        newPixels[newIndex + 1] = avgGreen;
                        newPixels[newIndex + 2] = avgBlue;
                        if (isRGBA)
                        {
                            newPixels[newIndex + 3] = avgAlpha;
                        }
                    }
                }

                return newPixels;
            }

            public static PixelFormat GetImagePixelFormatValue(string imagePath)
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    // Check if the PixelFormat indicates an alpha channel
                    return image.PixelFormat;
                }
            }

            public static bool IsImageRGBA(string imagePath)
            {
                using (Image image = Image.FromFile(imagePath))
                {
                    // Check if the PixelFormat indicates an alpha channel
                    return image.PixelFormat == PixelFormat.Format32bppArgb || image.PixelFormat == PixelFormat.Format64bppArgb;
                }
            }

            public string FullLocalImageFilePath { get => fullLocalImageFilePath; set => fullLocalImageFilePath = value; }

            public int Height { get => height; set => height = value; }

            public int Width { get => width; set => width = value; }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}