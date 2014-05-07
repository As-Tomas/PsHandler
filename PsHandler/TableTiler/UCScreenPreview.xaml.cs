﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace PsHandler.TableTiler
{
    /// <summary>
    /// Interaction logic for UCScreenPreview.xaml
    /// </summary>
    public partial class UCScreenPreview : UserControl
    {
        public UCScreenPreview()
        {
            InitializeComponent();
        }

        public void Update(System.Drawing.Rectangle[] config, bool numerate)
        {
            System.Windows.Forms.Screen[] screens = System.Windows.Forms.Screen.AllScreens;

            double
                leftScreen = double.MaxValue,
                topScreen = double.MaxValue,
                rightScreen = double.MinValue,
                bottomScreen = double.MinValue;

            foreach (System.Windows.Forms.Screen screen in screens)
            {
                if (leftScreen > screen.Bounds.Left) leftScreen = screen.Bounds.Left;
                if (topScreen > screen.Bounds.Top) topScreen = screen.Bounds.Top;
                if (rightScreen < screen.Bounds.Right) rightScreen = screen.Bounds.Right;
                if (bottomScreen < screen.Bounds.Bottom) bottomScreen = screen.Bounds.Bottom;
            }

            double
                widthCanvas = Canvas_Screens.ActualWidth,
                heightCanvas = Canvas_Screens.ActualHeight,
                widthScreen = Math.Abs((rightScreen - leftScreen)),
                heightScreen = Math.Abs((bottomScreen - topScreen));

            double ratio = widthCanvas / widthScreen; // x ratio
            if (heightScreen * ratio > heightCanvas) ratio = heightCanvas / heightScreen; // y ratio
            if (widthScreen * ratio > widthCanvas) throw new NotSupportedException();

            System.Drawing.Rectangle rScreen = new System.Drawing.Rectangle((int)leftScreen, (int)topScreen, (int)widthScreen, (int)heightScreen);
            double diffScreenX = -rScreen.Left;
            double diffScreenY = -rScreen.Top;

            // draw

            Canvas_Screens.Children.Clear();
            foreach (System.Windows.Forms.Screen screen in screens)
            {
                AddRectangle(rScreen, ratio, diffScreenX, diffScreenY, screen.Bounds, Brushes.DarkSlateBlue, 1, Brushes.LightSteelBlue);
            }
            if (config != null)
            {
                for (int i = 0; i < config.Length; i++)
                {
                    AddTable(rScreen, ratio, diffScreenX, diffScreenY, config[i], numerate ? (i + 1).ToString(CultureInfo.InvariantCulture) : "");
                }
            }

            // test all windows
            //IntPtr[] windowHWndAll = WinApi.GetWindowHWndAll(); for (int i = 0; i < windowHWndAll.Length; i++) if (!WinApi.GetWindowTitle(windowHWndAll[i]).Equals("Program Manager")) AddTable(rScreen, ratio, diffScreenX, diffScreenY, WinApi.GetWindowRectangle(windowHWndAll[i]), i.ToString());
        }

        private void AddTable(System.Drawing.Rectangle rScreen, double ratio, double diffScreenX, double diffScreenY, System.Drawing.Rectangle rObject, string text)
        {
            AddRectangle(rScreen, ratio, diffScreenX, diffScreenY, rObject, Brushes.DimGray, 1, new SolidColorBrush(Color.FromArgb(0x90, 0xFF, 0xFF, 0xFF)));
            AddLabel(rScreen, ratio, diffScreenX, diffScreenY, rObject, text, Brushes.DimGray, Brushes.Transparent);
        }

        private void AddRectangle(System.Drawing.Rectangle rScreen, double ratio, double diffScreenX, double diffScreenY,
            System.Drawing.Rectangle rObject, Brush stroke, double strokeThickness, Brush fill)
        {
            double x, y, width, height;
            width = rObject.Width * ratio;
            height = rObject.Height * ratio;
            x = (rObject.X + diffScreenX) * ratio;
            y = (rObject.Y + diffScreenY) * ratio;

            Rectangle r = new Rectangle();
            r.Width = width;
            r.Height = height;
            r.Stroke = stroke;
            r.StrokeThickness = strokeThickness;
            r.Fill = fill;

            Canvas_Screens.Children.Add(r);
            Canvas.SetLeft(r, x);
            Canvas.SetTop(r, y);
        }

        private void AddLabel(System.Drawing.Rectangle rScreen, double ratio, double diffScreenX, double diffScreenY,
            System.Drawing.Rectangle rObject, string text, Brush foreground, Brush background)
        {
            double x, y, width, height;
            width = rObject.Width * ratio;
            height = rObject.Height * ratio;
            x = (rObject.X + diffScreenX) * ratio;
            y = (rObject.Y + diffScreenY) * ratio;

            Label l = new Label();
            l.Width = width;
            l.Height = height;
            l.Foreground = foreground;
            l.Background = background;
            l.Padding = new Thickness(0);
            l.Margin = new Thickness(0);
            l.FontSize = height < width ? height * 0.5 : width * 0.5;
            l.Content = text;
            l.HorizontalContentAlignment = HorizontalAlignment.Center;
            l.VerticalContentAlignment = VerticalAlignment.Center;

            Canvas_Screens.Children.Add(l);
            Canvas.SetLeft(l, x);
            Canvas.SetTop(l, y);
        }
    }
}