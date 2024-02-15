using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NovaLauncher.Views.Controls
{
    /// <summary>
    /// Interaction logic for NewsOption.xaml
    /// </summary>
    public partial class NewsOption : UserControl
    {
        public NewsOption(string Title, string Discription, string imageUrl)
        {
            InitializeComponent();
            Titletxt.Text = Title;
            Biotxt.Text = Discription;
            try
            {
                BitmapImage bitmap = new BitmapImage(new Uri(imageUrl));
                NewsImage.ImageSource = bitmap;
            }
            catch { NewsImage.Opacity = 0; }
        }
    }
}
