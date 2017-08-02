using MemoGame.Models;
using MemoGame;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.Foundation.Collections;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace MemoGame.ViewModels
{
    public class CardViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Card> SmallCards = new ObservableCollection<Card>();
        public ObservableCollection<Card> MediumCards = new ObservableCollection<Card>();
        //public ObservableCollection<Card> LargeCards = new ObservableCollection<Card>();
      

        public int SelectedPivotItemIndex;
        private List<Card> SelectedCards = new List<Card>();
        Random Rnd = new Random();
        private int CardNumbers = 36;
        public int CardMatch;
        public DispatcherTimer timer = new DispatcherTimer();
        public bool TimerRunning = false;
        public string TimeFinished;
        //public SymbolIcon PlayIcon = new SymbolIcon(Symbol.Pause);
        

        private bool highscoreEnabled = true;

        public bool HighscoreEnabled
        {
            get { return highscoreEnabled; }
            set
            {
                highscoreEnabled = value;
                RaisePropertyChanged("HighscoreEnabled");
            }
        }

        private ObservableCollection<Card> largeCards = new ObservableCollection<Card>();

        public ObservableCollection<Card> LargeCards
        {
            get { return largeCards; }

            set
            {
                largeCards = value;
                RaisePropertyChanged("LargeCards");
            }
        }


        private ObservableCollection<Highscore> highscores = new ObservableCollection<Highscore>();

        public ObservableCollection<Highscore> Highscores
        {
            get { return highscores; }
            set
            {
                highscores = value;
                RaisePropertyChanged("Highscores");
            }
        }

        private SymbolIcon playIcon = new SymbolIcon(Symbol.Play);

        public SymbolIcon PlayIcon
        {
            get { return playIcon; }

            set
            {
                playIcon = value;
                RaisePropertyChanged("PlayIcon");
            }
        }



        public async void CardClicked(object sender, RoutedEventArgs e)
        {

            var CardGrid = sender as GridView;
           
            if (CardGrid.SelectedItem != null)
            {
                if (SelectedCards.Count < 2)
                {
                    var Card = CardGrid.SelectedItem as Card;
                    if (Card != null)
                    {
                        SelectedCards.Add(Card);
                        Rectangle CardFront;
                        Rectangle CardBack;
                        GetCardFrontAndBack(Card, CardGrid, out CardFront, out CardBack);
                        ShowCard(CardFront, CardBack);
                    }

                }

                if (SelectedCards.Count == 2)
                {
                    CompareCards(SelectedCards, CardGrid);
                    CardGrid.SelectedItem = null;
                    SelectedCards = new List<Card>();
                }

                if (CardMatch == CardNumbers / 2)
                {
                    
                    timer.Stop();
                    TimerRunning = false;
                    HighscoreEnabled = true;
                   
                    PlayIcon = new SymbolIcon(Symbol.Play);

                    string name = await InputTextDialogAsync("Please enter your name");

                    var Highscore = new Highscore(name, TimeFinished, DateTime.Now, IsNewHighscore(TimeFinished));

                    if (Highscore.NewBestScore)
                    {
                        foreach (Highscore h in Highscores)
                        {
                            h.NewBestScore = false;
                        }
                        Highscores.Insert(0, Highscore);
                    }

                    else
                    {
                        AddNewScore(Highscore);
                    }

                    var folder = ApplicationData.Current.RoamingFolder;

                    if (await folder.TryGetItemAsync("collection.json") == null)
                    {
                        var file = await folder.CreateFileAsync("collection.json", CreationCollisionOption.ReplaceExisting);
                        using (var stream = await file.OpenStreamForWriteAsync())
                        using (var writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            string json = JsonConvert.SerializeObject(Highscores);
                            await writer.WriteAsync(json);
                        }
                    }

                    else
                    {
                        var file = await folder.GetFileAsync("collection.json");
                        using (var stream = await file.OpenStreamForWriteAsync())
                        using (var writer = new StreamWriter(stream, Encoding.UTF8))
                        {
                            string json = JsonConvert.SerializeObject(Highscores);
                            await writer.WriteAsync(json);
                        }
                    }

                    foreach (Card c in LargeCards)
                    {
                        Rectangle CardFront;
                        Rectangle CardBack;
                        var cardlistviewitem = CardGrid.ContainerFromItem(c) as GridViewItem;
                        cardlistviewitem.IsEnabled = true;
                        GetCardFrontAndBack(c, CardGrid, out CardFront, out CardBack);
                        var CardFlipCloseAnimation = FlipCloseAnimation(CardFront, CardBack);
                        CardFlipCloseAnimation.Begin();

                    }

                    CardGrid.SelectionMode = ListViewSelectionMode.None;

                }
            }
          
         
          
        }

        private void AddNewScore(Highscore highscore)
        {
            var thishighscoretime = TimeSpan.Parse(highscore.FinishTime);

            foreach (Highscore h in Highscores)
            {
                var highscoretimes = TimeSpan.Parse(h.FinishTime);

                if (highscoretimes > thishighscoretime)
                {
                    Highscores.Move(Highscores.IndexOf(h), Highscores.IndexOf(h) + 1);
                    Highscores.Insert(Highscores.IndexOf(h) - 1, highscore);
                }

                
            }

            Highscores.Add(highscore);

        }

        protected void RaisePropertyChanged(string Selected)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Selected));
        }

        private bool IsNewHighscore(string TimeFinished)
        {
            bool isnewhighscore = true;
            var thishighscoretime = TimeSpan.Parse(TimeFinished);

            foreach (Highscore hs in Highscores)
            {
                var highscoretimes = TimeSpan.Parse(hs.FinishTime);
                
                if (highscoretimes > thishighscoretime)
                {
                    isnewhighscore = true;
                }

                else
                {
                    isnewhighscore = false;
                }
            }

            return isnewhighscore;

        }


        private async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return "";
        }



        public void GetCardFrontAndBack(Card Card, GridView CardGrid, out Rectangle CardFront, out Rectangle CardBack)
        {
            var container = CardGrid.ContainerFromItem(Card) as GridViewItem;
            var itempresenter = VisualTreeHelper.GetChild(container, 0);
            var grid = VisualTreeHelper.GetChild(itempresenter, 0);
            var rectanglefront = VisualTreeHelper.GetChild(grid, 1) as Rectangle;
            var rectangleback = VisualTreeHelper.GetChild(grid, 0) as Rectangle;

            CardFront = rectanglefront;
            CardBack = rectangleback;
        }

        public void ShowCard(Rectangle CardFront, Rectangle CardBack)
        {
            var flipOpenAnimation = FlipOpenAnimation(CardFront, CardBack);
            flipOpenAnimation.Begin();
            CardBack.Visibility = Visibility.Visible;
        }

        public void HideCards(List<Card> Cards, GridView CardGrid)
        {
            var FirstCard = Cards[0];
            var SecondCard = Cards[1];

            Rectangle FirstCardFront;
            Rectangle FirstCardBack;
            GetCardFrontAndBack(FirstCard, CardGrid, out FirstCardFront, out FirstCardBack);

            Rectangle SecondCardFront;
            Rectangle SecondCardBack;
            GetCardFrontAndBack(SecondCard, CardGrid, out SecondCardFront, out SecondCardBack);

            var firstCardFlipCloseAnimation = FlipCloseAnimation(FirstCardFront, FirstCardBack);
            firstCardFlipCloseAnimation.Begin();
           

            var secondCardFlipCloseAnimation = FlipCloseAnimation(SecondCardFront, SecondCardBack);
            secondCardFlipCloseAnimation.Begin();
            

        }

        private Storyboard FlipOpenAnimation(Rectangle CardFront, Rectangle CardBack)
        {
            Storyboard FlipOpen = new Storyboard();

            DoubleAnimationUsingKeyFrames animation1 = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame keyframe1 = new EasingDoubleKeyFrame();
            keyframe1.Value = 0;
            keyframe1.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0));
            EasingDoubleKeyFrame keyframe2 = new EasingDoubleKeyFrame();
            keyframe2.Value = 90;
            keyframe2.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));

            DoubleAnimationUsingKeyFrames animation2 = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame keyframe3 = new EasingDoubleKeyFrame();
            keyframe3.Value = -90;
            keyframe3.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            EasingDoubleKeyFrame keyframe4 = new EasingDoubleKeyFrame();
            keyframe4.Value = 0;
            keyframe4.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4));

            animation1.KeyFrames.Add(keyframe1);
            animation1.KeyFrames.Add(keyframe2);

            animation2.KeyFrames.Add(keyframe3);
            animation2.KeyFrames.Add(keyframe4);

            Storyboard.SetTarget(animation1, CardFront);
            Storyboard.SetTargetProperty(animation1, "(UIElement.Projection).(PlaneProjection.Rotation" + "Y" + ")");
            Storyboard.SetTarget(animation2, CardBack);
            Storyboard.SetTargetProperty(animation2, "(UIElement.Projection).(PlaneProjection.Rotation" + "Y" + ")");

            FlipOpen.Children.Add(animation1);
            FlipOpen.Children.Add(animation2);

            return FlipOpen;
        }

        private Storyboard FlipCloseAnimation(Rectangle CardFront, Rectangle CardBack)
        {
            Storyboard FlipClose = new Storyboard();

            DoubleAnimationUsingKeyFrames animation3 = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame keyframe5 = new EasingDoubleKeyFrame();
            keyframe5.Value = 90;
            keyframe5.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            EasingDoubleKeyFrame keyframe6 = new EasingDoubleKeyFrame();
            keyframe6.Value = 0;
            keyframe6.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4));

            DoubleAnimationUsingKeyFrames animation4 = new DoubleAnimationUsingKeyFrames();
            EasingDoubleKeyFrame keyframe7 = new EasingDoubleKeyFrame();
            keyframe7.Value = -90;
            keyframe7.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2));
            EasingDoubleKeyFrame keyframe8 = new EasingDoubleKeyFrame();
            keyframe8.Value = -90;
            keyframe8.KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.4));

            animation3.KeyFrames.Add(keyframe5);
            animation3.KeyFrames.Add(keyframe6);

            animation4.KeyFrames.Add(keyframe7);
            animation4.KeyFrames.Add(keyframe8);

            Storyboard.SetTarget(animation3, CardFront);
            Storyboard.SetTargetProperty(animation3, "(UIElement.Projection).(PlaneProjection.Rotation" + "Y" + ")");
            Storyboard.SetTarget(animation4, CardBack);
            Storyboard.SetTargetProperty(animation4, "(UIElement.Projection).(PlaneProjection.Rotation" + "Y" + ")");

            FlipClose.Children.Add(animation3);
            FlipClose.Children.Add(animation4);


            return FlipClose;
        }


        public async void CompareCards (List<Card> Cards, GridView CardGrid)
        {
            
            if (Cards.Count == 2)
            {
                var FirstCard = Cards[0];
                var SecondCard = Cards[1];
                var FirstCardImage = FirstCard.CardImage.ImageSource;
                var SecondCardImage = SecondCard.CardImage.ImageSource;
                var firstcardlistviewitem = CardGrid.ContainerFromItem(FirstCard) as GridViewItem;
                var secondcardlistviewitem = CardGrid.ContainerFromItem(SecondCard) as GridViewItem;

                var firstimage = FirstCardImage as BitmapImage;
                var firstimagesource = firstimage.UriSource;

                var secondimage = SecondCardImage as BitmapImage;
                var secondimagesource = secondimage.UriSource;

                if (firstimagesource == secondimagesource)
                {
                    firstcardlistviewitem.IsEnabled = false;
                    secondcardlistviewitem.IsEnabled = false;
                    CardMatch++;
                   // SelectedCards = new List<Card>();
                }

                else
                {
                    await Task.Delay(400);
                    HideCards(Cards, CardGrid);
                    //SelectedCards = new List<Card>(); Ezt mindenképp analizáld ki !!!!

                }
            }
            

        }




        //public void PivotItem_Changed(object sender, RoutedEventArgs e)
        //{
        //    var pivot = (Pivot)sender;

        //    if (pivot.SelectedIndex == 0 && !smallGridViewFilledUp)
        //    {
        //        var cardsize = new Card.Size(150, 150);
        //        FillUpGridView(LargeCards, cardsize, 205);
        //        smallGridViewFilledUp = true;
        //    }
        //    if (pivot.SelectedIndex == 1 && !mediumGridViewFilledUp)
        //    {
        //        var cardsize = new Card.Size(100, 100);
        //        FillUpGridView(MediumCards, cardsize, 105);
        //        mediumGridViewFilledUp = true;
        //    }
        //    if (pivot.SelectedIndex == 2 && !largeGridViewFilledUp)
        //    {
        //        var cardsize = new Card.Size(60, 60);
        //        FillUpGridView(SmallCards, cardsize, 75);
        //        largeGridViewFilledUp = true;
        //    }

        //}

        public async Task<List<string>> GetImagePaths()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFolder MeMe = await assets.GetFolderAsync("MeMe Edition");
            var files = await MeMe.GetFilesAsync();
            var filepaths = new List<string>();

            foreach (StorageFile file in files)
            {
                filepaths.Add(file.Path);
            }

            return filepaths;
        }

        public string GetRandomImagePath(List<string> ImagePaths, List<string> results)
        {
            
            
            
            
            if (ImagePaths.Count < 1)
              return "There are no more new words!! :(";

            var index = Rnd.Next(0, ImagePaths.Count);
            var result = ImagePaths[index];

            if (results.Contains(result))
            {
                ImagePaths.RemoveAt(index);
                results.Remove(result);
            }

            else
            {
                results.Add(result);
            }

            return result;
           
        }

        public async void FillUpGridViews()
        {
            List<string> imagepaths =  await GetImagePaths();
            List<string> results = new List<string>();


            for (int cardnumbers = 0; cardnumbers < CardNumbers ; cardnumbers++)
            {
                var randomImagePath = GetRandomImagePath(imagepaths, results);
                var img = new ImageBrush();
                img.ImageSource = new BitmapImage(new Uri(randomImagePath, UriKind.Absolute));

                var newBigCard = new Card(new Card.Size(150, 150), img);
                LargeCards.Add(newBigCard);
            }

          


            //for (int cardnumbers = 0; cardnumbers < 95; cardnumbers++)
            //{
            //    var newMediumCard = new Card(new Card.Size(100, 100), img);
            //    MediumCards.Add(newMediumCard);
            //}

            //for (int cardnumbers = 0; cardnumbers < 305; cardnumbers++)
            //{
            //    var newSmallCard = new Card(new Card.Size(60, 60), img);
            //    SmallCards.Add(newSmallCard);
            //}
        }

    }
}
