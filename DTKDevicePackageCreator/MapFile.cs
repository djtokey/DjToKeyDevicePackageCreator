using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    class MapFile: INotifyPropertyChanged
    {
        private string map;
        public string Map
        {
            get { return map; }
            set
            {
                if (this.map != value)
                {
                    map = value;
                    OnPropertyChanged(nameof(Map));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

    }
}
