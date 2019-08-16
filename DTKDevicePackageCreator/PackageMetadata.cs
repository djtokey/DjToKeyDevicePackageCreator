using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class PackageMetadata : INotifyPropertyChanged
    {

        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                if (this.title != value)
                {
                    title = value;
                    OnPropertyChanged();
                }
            }
        }


        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (this.description != value)
                {
                    description = value;
                    OnPropertyChanged();
                }
            }
        }


        private string version;
        public string Version
        {
            get { return version; }
            set
            {
                if (this.version != value)
                {
                    version = value;
                    OnPropertyChanged();
                }
            }
        }


        private string keywords;
        public string Keywords
        {
            get { return keywords; }
            set
            {
                if (this.keywords != value)
                {
                    keywords = value;
                    OnPropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
