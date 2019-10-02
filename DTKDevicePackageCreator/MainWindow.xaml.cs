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
        private MapWindow map;
        private MapFile mapFile;
        private VisualControlsHandler vch;

        private ObservableCollection<Device> devices;

        public MainWindow()
        {
            InitializeComponent();
            devices = new ObservableCollection<Device>();

            lbDevices.ItemsSource = devices;

            metadata = new MetadataWindow();
            midi = new MidiWindow();


            map = new MapWindow();
            mapFile = new MapFile();
            map.DataContext = mapFile;

            vch = new VisualControlsHandler();
            vch.ControlsCanvas = LayoutRoot;
            vch.DetailsPanel = controlData;

            midi.ControlsHandler = vch;

            controlType.ItemsSource = Enum.GetValues(typeof(Ktos.DjToKey.Plugins.Device.ControlType));
            
        }

        private void addControl(object sender, RoutedEventArgs e)
        {
            vch.AddRandomControl();
        }        

        private void addDevice(object sender, RoutedEventArgs e)
        {
            var i = new InputDialog();
            i.Title = "New device name";            
            i.ShowDialog();            

            if (i.DialogResult == true)
            {
                devices.Add(new Device() { Name = i.Value, Controls = new List<ViewControl>() });
            }
        }

        private void changeImage(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = "png";
            ofd.Filter = "PNG Image|*.png";
            if (ofd.ShowDialog() ?? false)
            {
                (lbDevices.SelectedItem as Device).Image = new BitmapImage(new Uri("file://" + ofd.FileName));
            }
        }

        private void Devices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbDevices.SelectedValue == null)
            {
                lbDevices.SelectedIndex = 0;
                return;
            }

            DataContext = (lbDevices.SelectedItem as Device);
            vch.Controls = (lbDevices.SelectedItem as Device).Controls;
            midi.Device = (lbDevices.SelectedItem as Device);
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
            devices.Clear();            

            using (var pack = Package.Open(fileName, FileMode.Open))
            {
                List<string> devicesInPackage = new List<string>();

                foreach (var p in pack.GetParts())
                {
                    string x = p.Uri.ToString().TrimStart('/');

                    if (x == "map.json")
                        continue;

                    x = x.Remove(x.IndexOf('/'));

                    if (x != "_rels" && x != "package")
                        devicesInPackage.Add(x);
                }

                foreach (var d in devicesInPackage.Distinct())
                {
                    Device x = new Device();
                    x.Name = d;

                    Uri u = new Uri(string.Format("/{0}/definition.json", x.Name), UriKind.Relative);

                    if (pack.PartExists(u))
                    {
                        var f = pack.GetPart(u).GetStream();
                        using (StreamReader reader = new StreamReader(f, Encoding.UTF8))
                        {
                            string json = reader.ReadToEnd();
                            x.Controls = JsonConvert.DeserializeObject<List<ViewControl>>(json);
                        }
                    }

                    u = new Uri(string.Format("/{0}/image.png", x.Name), UriKind.Relative);

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

                    devices.Add(x);
                }

                if (pack.PartExists(new Uri("/map.json", UriKind.Relative)))
                {
                    using (TextReader tr = new StreamReader(pack.GetPart(new Uri("/map.json", UriKind.Relative)).GetStream()))
                    {
                        mapFile.Map = tr.ReadToEnd();
                    }
                }

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

        private void removeDevice(object sender, RoutedEventArgs e)
        {
            devices.Remove(lbDevices.SelectedItem as Device);            
        }

        private void showMetadata(object sender, RoutedEventArgs e)
        {
            metadata.Show();
        }

        private void showMidi(object sender, RoutedEventArgs e)
        {
            midi.Show();
        }

        private void showMap(object sender, RoutedEventArgs e)
        {
            map.Show();
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
                foreach (var d in devices)
                {
                    im = new Uri(string.Format("/{0}/image.png", d.Name), UriKind.Relative);
                    df = new Uri(string.Format("/{0}/definition.json", d.Name), UriKind.Relative);

                    if (d.Image != null)
                    {
                        var p1 = pack.CreatePart(im, "image/png");

                        PngBitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(d.Image));
                        encoder.Save(p1.GetStream());
                    }

                    var p2 = pack.CreatePart(df, "application/json");
                    using (TextWriter sw = new StreamWriter(p2.GetStream()))
                    {
                        sw.Write(JsonConvert.SerializeObject(d.Controls));
                    }
                }

                if (!string.IsNullOrEmpty(mapFile.Map))
                {
                    Uri mp = new Uri("/map.json", UriKind.Relative);
                    if (pack.PartExists(mp))
                        pack.DeletePart(mp);

                    var p3 = pack.CreatePart(new Uri("/map.json", UriKind.Relative), "application/json");
                    using (TextWriter sw = new StreamWriter(p3.GetStream()))
                    {
                        sw.Write(mapFile.Map);
                    }
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