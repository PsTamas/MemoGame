using MemoGame.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MemoGame.ViewModels
{
    public class CardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Card> Cards = new ObservableCollection<Card>();
        
        public void AddCard()
        {
            var newcard = new Card(Card.Size.Small, new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)), new BitmapImage(new Uri("ms-appx:///Assets/avatar_ac3009f7fffd_128.png")));
            Cards.Add(newcard);
        }
    }
}
