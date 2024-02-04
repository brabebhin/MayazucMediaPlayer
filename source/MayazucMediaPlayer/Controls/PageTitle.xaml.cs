using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MayazucMediaPlayer.Controls
{
    public sealed partial class PageTitle : UserControl
    {
        public PageTitle()
        {
            InitializeComponent();
        }

        public IconElement Icon
        {
            get => (IconElement)IconPresenter.Content;
            set => IconPresenter.Content = value;
        }

        public string Title
        {
            get => TitleBox.Text;
            set => TitleBox.Text = value;
        }

        public static DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(PageTitle), new PropertyMetadata(string.Empty, TitlePropertyChanged));

        private static void TitlePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (string)e.NewValue;
            var obj = d as PageTitle;
            obj.Title = newValue;
        }

        public static DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(IconElement), typeof(PageTitle), new PropertyMetadata(null, IconPropertyChanged));

        private static void IconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = (IconElement)e.NewValue;
            var obj = d as PageTitle;
            obj.Icon = newValue;
        }
    }
}
