using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoGame.Models
{
    public class Card
    {
        public enum Size { Small, Medium, Large };

        public SolidColorBrush CardColor { get; private set; }
        public Size CardSize { get; private set; }
        public BitmapImage CardImage { get; private set; }

        public Card(Size cardSize, SolidColorBrush cardColor, BitmapImage cardImage)
        {
            this.CardColor = cardColor;
            this.CardSize = cardSize;
            this.CardImage = cardImage;
        }
    }

  


}

