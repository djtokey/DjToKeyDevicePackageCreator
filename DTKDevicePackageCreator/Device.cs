using Ktos.DjToKey.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace WpfApplication1
{
    public class Device : INotifyPropertyChanged
    {

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (this._name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        
        public List<ViewControl> Controls { get; set; }


        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (this._image != value)
                {
                    _image = value;
                    OnPropertyChanged(nameof(Image));
                }
            }
        }


        public override string ToString()
        {
            return Name;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
