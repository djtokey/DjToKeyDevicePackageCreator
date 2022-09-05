using Ktos.DjToKey.Models;
using Midi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DjToKey.DevicePackageCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {                
        private MetadataWindow metadata;
        private MidiWindow midi;        
        private VisualControlsHandler vch;

        private Device device;

        public MainWindow()
        {
            InitializeComponent();
            device = new Device() { Name = "device", Controls = new List<ViewControl>() };

            metadata = new MetadataWindow();
            midi = new MidiWindow();

            

            vch = new VisualControlsHandler();
            vch.ControlsCanvas = LayoutRoot;
            vch.DetailsPanel = controlData;

            midi.ControlsHandler = vch;

            DataContext = device;
            vch.Controls = device.Controls;
            midi.Device = device;

            controlType.ItemsSource = Enum.GetValues(typeof(Ktos.DjToKey.Plugins.Device.ControlType));
            
        }

        private void addControl(object sender, RoutedEventArgs e)
        {
            vch.AddRandomControl();
        }        

        private void changeImage(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = "png";
            ofd.Filter = "PNG Image|*.png";
            if (ofd.ShowDialog() ?? false)
            {
                device.Image = new BitmapImage(new Uri("file://" + ofd.FileName));
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = "dtkpkg";
            ofd.Filter = "DjToKey Device Packages|*.dtkpkg";
            if (ofd.ShowDialog() ?? false)
            {
                loadDevicesFromPackage(ofd.FileName);
                tbPackageName.Text = ofd.SafeFileName;
            }
        }

        private void loadDevicesFromPackage(string fileName)
        {
            var x = new Device() { Name = "device", Controls = new List<ViewControl>() };

            using (var pack = Package.Open(fileName, FileMode.Open))
            {
                Uri u = new Uri("/device/definition.json", UriKind.Relative);

                if (pack.PartExists(u))
                {
                    var f = pack.GetPart(u).GetStream();
                    using (StreamReader reader = new StreamReader(f, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        x.Controls = JsonConvert.DeserializeObject<List<ViewControl>>(json);
                    }
                }

                u = new Uri("/device/image.png", UriKind.Relative);

                if (pack.PartExists(u))
                {
                    BitmapImage bitmap;

                    using (var stream = pack.GetPart(u).GetStream())
                    {
                        bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }

                    x.Image = bitmap;
                }

                device = x;
                DataContext = device;
                vch.Controls = device.Controls;
                midi.Device = device;

                var packMeta = new PackageMetadata()
                {
                    Keywords = pack.PackageProperties.Keywords ?? string.Empty,
                    Title = pack.PackageProperties.Title ?? string.Empty,
                    Description = pack.PackageProperties.Description ?? string.Empty,
                    Version = pack.PackageProperties.Version ?? string.Empty
                };
                metadata.DataContext = packMeta;
            }
        }        

        private void showMetadata(object sender, RoutedEventArgs e)
        {
            metadata.Show();
        }

        private void showMidi(object sender, RoutedEventArgs e)
        {
            midi.Show();
        }        

        private void Save(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.DefaultExt = "dtkpkg";
            sfd.Filter = "DjToKey Device Packages|*.dtkpkg";
            

            if (sfd.ShowDialog() ?? false)
            {
                saveDeviceToPackage(sfd.FileName);
            }
        }

        private void saveDeviceToPackage(string fileName)
        {
            Uri im;
            Uri df;

            using (var pack = Package.Open(fileName, FileMode.Create))
            {
                im = new Uri("/device/image.png", UriKind.Relative);
                df = new Uri("/device/definition.json", UriKind.Relative);

                if (device.Image != null)
                {
                    var p1 = pack.CreatePart(im, "image/png");

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(device.Image));
                    encoder.Save(p1.GetStream());
                }

                var p2 = pack.CreatePart(df, "application/json");
                using (TextWriter sw = new StreamWriter(p2.GetStream()))
                {
                    sw.Write(JsonConvert.SerializeObject(device.Controls));
                }                               

                PackageMetadata packMeta = metadata.DataContext as PackageMetadata;
                if (packMeta == null) packMeta = new PackageMetadata();
                metadata.DataContext = packMeta;

                pack.PackageProperties.Keywords = packMeta.Keywords;
                pack.PackageProperties.Title = packMeta.Title;
                pack.PackageProperties.Description = packMeta.Description;
                pack.PackageProperties.Version = packMeta.Version;
                pack.PackageProperties.Category = "device";
            }
        }

        private void btnSaveControl(object sender, RoutedEventArgs e)
        {
            vch.UpdateControl((sender as Button).Tag.ToString());
        }

        private void btnRemoveControl(object sender, RoutedEventArgs e)
        {
            vch.RemoveControl((sender as Button).Tag.ToString());
        }

        private void btnBackgroundControl(object sender, RoutedEventArgs e)
        {
            vch.UpdateControlBackground((sender as Button).Tag.ToString());
        }

        private void MWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}