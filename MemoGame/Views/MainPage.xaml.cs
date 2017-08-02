using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MemoGame.ViewModels;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using System.Collections.ObjectModel;
using MemoGame.Models;
using Windows.Storage;
using Newtonsoft.Json;
using System.Text;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MemoGame
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            CardCollection = new CardViewModel();
            Loaded += MainPage_Loaded;

            CardCollection.timer.Interval = TimeSpan.FromMilliseconds(10);
            CardCollection.timer.Tick += Timer_Tick;

            
            
        }

        public Rectangle frontrectangle = new Rectangle();
        public Rectangle backrectangle = new Rectangle();

        public DateTime StartTime = DateTime.MinValue;
        private TimeSpan _currentElapsedTime = TimeSpan.Zero;
        private TimeSpan _totalElapsedTime = TimeSpan.Zero;
        
        
        public string TimerString;
        

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CardCollection.FillUpGridViews();

            var folder = ApplicationData.Current.RoamingFolder;

            //var path = Windows.Storage.ApplicationData.Current.RoamingFolder.Path;

            if (await folder.TryGetItemAsync("collection.json") != null)
            {
                var file = await folder.GetFileAsync("collection.json");
                using (var stream = await file.OpenStreamForReadAsync())
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string json = await reader.ReadToEndAsync();
                    var collection = JsonConvert.DeserializeObject<ObservableCollection<Highscore>>(json);
                    CardCollection.Highscores = collection;

                }
            }
          

        }

        public CardViewModel CardCollection { get; set; }

        public void Timer_Tick(object sender, object e)
        {
            var timeSinceStartTime = DateTime.Now - StartTime ;
            timeSinceStartTime = new TimeSpan(
                                              timeSinceStartTime.Hours,
                                              timeSinceStartTime.Minutes,
                                              timeSinceStartTime.Seconds
                                              
                                              );

            // The current elapsed time is the time since the start button was
            // clicked, plus the total time elapsed since the last reset
            _currentElapsedTime = timeSinceStartTime + _totalElapsedTime;

            // These are just two Label controls which display the current 
            // elapsed time and total elapsed time

            TimerTextBlock.Text = _currentElapsedTime.ToString();
            CardCollection.TimeFinished = TimerTextBlock.Text;
        }


        public void Start_Game(object sender, RoutedEventArgs e)
        {
            var CardGrid = this.FindName("CardContainer") as GridView;
          
            if (!CardCollection.TimerRunning)
            {
                if (CardCollection.CardMatch != 0)
                {
                    CardCollection.CardMatch = 0;
                    CardGrid.IsEnabled = true;
                    _currentElapsedTime = TimeSpan.Zero;
                    CardCollection.LargeCards.Clear();
                    CardCollection.LargeCards = new ObservableCollection<Card>();
                    CardCollection.FillUpGridViews();
                }
                
                // Set the start time to Now
               
                StartTime = DateTime.Now;

                CardCollection.HighscoreEnabled = false;
               
                CardGrid.SelectionMode = ListViewSelectionMode.Single;



                CardCollection.PlayIcon = new SymbolIcon(Symbol.Pause);

                // Store the total elapsed time so far
                _totalElapsedTime = _currentElapsedTime;

                

                CardCollection.timer.Start();
                CardCollection.TimerRunning = true;
            }

            else // If the timer is already running
            {
                
                CardCollection.timer.Stop();
                CardCollection.TimerRunning = false;
                CardCollection.PlayIcon = new SymbolIcon(Symbol.Play);
                CardGrid.SelectionMode = ListViewSelectionMode.None;
            }
        }

        public void GoToHighscores()
        {
            Pivot.SelectedItem = HighscoresTable;
        }

        public void SelectionStatus(object sender, RoutedEventArgs e)
        {
            var pivot = (Pivot)sender;
            if (pivot.SelectedItem.Equals(HighscoresTable))
            {
                Play.IsEnabled = false;
            }

            if (pivot.SelectedItem.Equals(Game))
            {
                Play.IsEnabled = true;
            }

        }



    }
}
