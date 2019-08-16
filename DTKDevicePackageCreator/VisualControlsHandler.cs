using Ktos.DjToKey.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfApplication1
{
    public class VisualControlsHandler
    {
        private Random r;
        private bool captured = false;

        private List<ViewControl> _controls;
        public List<ViewControl> Controls
        {
            get
            {
                return _controls;
            }

            set
            {
                _controls = value;
                SynchronizeView();
            }
        }

        public void SynchronizeView()
        {
            if (ControlsCanvas != null)
            {
                ControlsCanvas.Children.Clear();
                AddControls(Controls);
            }
        }

        private UIElement source = null;

        public Canvas ControlsCanvas;
        public StackPanel DetailsPanel;

        private double x_shape, x_canvas, y_shape, y_canvas;

        public VisualControlsHandler()
        {
            r = new Random();
            Controls = new List<ViewControl>();
        }

        public void Clear()
        {
            Controls.Clear();
            ControlsCanvas.Children.Clear();
        }

        private string randomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void AddControls(IEnumerable<ViewControl> controls)
        {
            foreach (var c in controls)
            {
                addVisualControl(c);
            }
        }

        public void AddRandomControl()
        {
            var c = new ViewControl() { ControlId = r.Next().ToString(), ControlName = randomString(5), Type = Ktos.DjToKey.Plugins.Device.ControlType.Analog, Background = randomColor(), Height = 100, Width = 100, Left = 0, Top = 0 };
            Controls.Add(c);
            addVisualControl(c);
        }

        private void addVisualControl(ViewControl tag)
        {
            var b = new Border();

            b.Background = new SolidColorBrush(tag.Background);
            b.Width = tag.Width;
            b.Height = tag.Height;
            Canvas.SetLeft(b, tag.Left);
            Canvas.SetTop(b, tag.Top);
            b.Tag = tag;
            b.Child = new TextBlock() { Text = tag.ControlId + "\n" + tag.ControlName, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            b.Opacity = 0.7;

            b.MouseLeftButtonDown += B_MouseLeftButtonDown;
            b.MouseLeftButtonUp += B_MouseLeftButtonUp;
            b.MouseMove += B_MouseMove;
            b.MouseWheel += B_MouseWheel;
            b.MouseRightButtonDown += B_MouseRightButtonDown;

            ControlsCanvas.Children.Add(b);
        }

        private void B_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            source.Opacity = 0.7;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(ControlsCanvas).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(ControlsCanvas).Y;
        }

        private void B_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;

            var x = ((source as Border).Tag as ViewControl);
            x.Left = (int)Canvas.GetLeft(source);
            x.Top = (int)Canvas.GetTop(source);
            x.Width = (int)(source as Border).Width;
            x.Height = (int)(source as Border).Height;
        }

        private void B_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                double x = e.GetPosition(ControlsCanvas).X;
                double y = e.GetPosition(ControlsCanvas).Y;
                x_shape += x - x_canvas;
                Canvas.SetLeft(source, x_shape);
                x_canvas = x;
                y_shape += y - y_canvas;
                Canvas.SetTop(source, y_shape);
                y_canvas = y;
            }
        }

        private void B_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Opacity = 1;
            DetailsPanel.DataContext = ((sender as Border).Tag as Ktos.DjToKey.Plugins.Device.Control);
        }

        private void B_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            (sender as Decorator).Width -= e.Delta / 12;
            (sender as Decorator).Height -= e.Delta / 12;

            var x = ((source as Border).Tag as ViewControl);
            x.Width = (int)(source as Border).Width;
            x.Height = (int)(source as Border).Height;
        }

        public void RemoveControl(string controlId)
        {
            var c = FindControl(controlId);
            if (c != null)
            {
                ControlsCanvas.Children.Remove(FindVisualControl(c));
                Controls.Remove(c);
            }
        }

        private Border FindVisualControl(Ktos.DjToKey.Plugins.Device.Control c)
        {
            for (int i = 0; i < ControlsCanvas.Children.Count; i++)
            {
                var f = ControlsCanvas.Children[i] as Border;
                if (f.Tag == c)
                {
                    return f;
                }
            }

            return null;
        }

        private ViewControl FindControl(string controlId)
        {
            return Controls.Find(x => x.ControlId == controlId);
        }

        public void UpdateControl(string controlId)
        {
            var c = FindControl(controlId);
            var f = FindVisualControl(c);

            if (f != null) (f.Child as TextBlock).Text = c.ControlId + "\n" + c.ControlName;

        }

        public void UpdateControlBackground(string controlId)
        {
            var c = FindControl(controlId);
            var f = FindVisualControl(c);

            c.Background = randomColor();
            if (f != null) (f as Border).Background = new SolidColorBrush(c.Background);
        }

        public void HighlightControl(string controlId)
        {
            var c = FindControl(controlId);
            if (c == null)
                return;

            var f = FindVisualControl(c);
            if (f != null) f.Opacity = 1;
        }

        private Color randomColor()
        {
            Color[] clr = { Colors.Cornsilk, Colors.ForestGreen, Colors.MediumVioletRed, Colors.RoyalBlue, Colors.SlateGray, Colors.Tomato, Colors.RosyBrown, Colors.YellowGreen, Colors.PeachPuff, Colors.WhiteSmoke };

            return clr[r.Next(clr.Length)];
        }
    }
}
