using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MemoGame.Models
{
    [DataContract]
    public class Highscore : INotifyPropertyChanged
    {
        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string FinishTime { get; private set; }

        [DataMember]
        public DateTime Date { get; private set; }

        [DataMember]
        private bool newBestScore;

        public bool NewBestScore
        {
            get { return newBestScore; }
            set
            {
                newBestScore = value;
                RaisePropertyChanged("NewBestScore");
            }
        }

        public Highscore(string name, string finishtime, DateTime date, bool newbestscore)
        {
            Name = name;
            FinishTime = finishtime;
            Date = date;
            NewBestScore = newbestscore;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string Selected)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Selected));
        }
    }
}
