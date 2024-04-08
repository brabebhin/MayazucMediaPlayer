using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using ZXing;
// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class QRCodeControl : BaseUserControl
    {
        public string QRMessage
        {
            get;
            private set;
        }

        public double QRCodeWidth
        {
            get => (double)GetValue(QRCodeWidthProperty);
            set => SetValue(QRCodeWidthProperty, value);
        }

        public double QRCodeHeight
        {
            get => (double)GetValue(QRCodeHeightProperty);
            set => SetValue(QRCodeHeightProperty, value);
        }

        public static DependencyProperty QRCodeWidthProperty = DependencyProperty.Register(nameof(QRCodeWidth), typeof(double), typeof(QRCodeControl), new PropertyMetadata(200));
        public static DependencyProperty QRCodeHeightProperty = DependencyProperty.Register(nameof(QRCodeHeight), typeof(double), typeof(QRCodeControl), new PropertyMetadata(200));

        public QRCodeControl()
        {
            InitializeComponent();
        }

        public void LoadQRMessage(string QRMessage)
        {
            if (string.IsNullOrWhiteSpace(QRMessage))
            {
                QRMessage = "Not connected to any network";
            }
            this.QRMessage = QRMessage;

            var write = new BarcodeWriter<BitmapImage>();
            write.Format = BarcodeFormat.QR_CODE;
            var wb = write.Write(QRMessage);
            QRimage.Source = wb;
        }
    }
}
