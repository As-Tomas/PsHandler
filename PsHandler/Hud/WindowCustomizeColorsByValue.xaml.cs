﻿// PsHandler - poker software helping tool.
// Copyright (C) 2014-2015  kampiuceris

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PsHandler.UI;

namespace PsHandler.Hud
{
    public class HudColorsByValueParams
    {
        public enum HudColorsByValueType { HudColorsByValueOpponents, HudColorsByValueHero }

        private readonly HudColorsByValueType _hudColorsByValueType;
        public List<ColorByValue> ColorsByValue = new List<ColorByValue>();

        public HudColorsByValueParams(HudColorsByValueType hudColorsByValueType)
        {
            _hudColorsByValueType = hudColorsByValueType;

            var input = new List<ColorByValue>();
            switch (_hudColorsByValueType)
            {
                case HudColorsByValueType.HudColorsByValueOpponents:
                    input = Config.HudBigBlindOpponentsColorsByValue;
                    break;
                case HudColorsByValueType.HudColorsByValueHero:
                    input = Config.HudBigBlindHeroColorsByValue;
                    break;
            }

            foreach (var colorByValue in input)
            {
                ColorsByValue.Add(colorByValue.Clone());
            }
        }

        public void Save()
        {
            var output = ColorsByValue.Select(colorByValue => colorByValue.Clone()).ToList();

            switch (_hudColorsByValueType)
            {
                case HudColorsByValueType.HudColorsByValueOpponents:
                    Config.HudBigBlindOpponentsColorsByValue = output;
                    break;
                case HudColorsByValueType.HudColorsByValueHero:
                    Config.HudBigBlindHeroColorsByValue = output;
                    break;
            }
        }
    }

    /// <summary>
    /// Interaction logic for WindowCustomizeColorsByValue.xaml
    /// </summary>
    public partial class WindowCustomizeColorsByValue : Window
    {
        public HudColorsByValueParams HudColorsByValueParams;

        public WindowCustomizeColorsByValue(Window owner, Color defaultColor, HudColorsByValueParams.HudColorsByValueType hudColorsByValueType)
        {
            InitializeComponent();
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            HudColorsByValueParams = new HudColorsByValueParams(hudColorsByValueType);

            Rectangle_Default.Fill = new SolidColorBrush(defaultColor);
            Label_Default.Content = defaultColor.ToString();
            List<string> colorNames = typeof(System.Drawing.Color).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(item => item.Name).ToList();
            foreach (string colorName in colorNames)
            {
                System.Drawing.Color colorDrawing = System.Drawing.Color.FromName(colorName);
                System.Windows.Media.Color colorMedia = Color.FromArgb(colorDrawing.A, colorDrawing.R, colorDrawing.G, colorDrawing.B);
                if (defaultColor.Equals(colorMedia))
                {
                    Label_Default.Content = colorName;
                    break;
                }
            }

            foreach (var colorByValue in HudColorsByValueParams.ColorsByValue)
            {
                StackPanel_ColorsByValue.Children.Add(new UCColorByValue(this, StackPanel_ColorsByValue, colorByValue));
            }
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            StackPanel_ColorsByValue.Children.Add(new UCColorByValue(this, StackPanel_ColorsByValue));
        }

        private bool CollectParams()
        {
            HudColorsByValueParams.ColorsByValue = new List<ColorByValue>();

            foreach (var item in StackPanel_ColorsByValue.Children.OfType<UCColorByValue>())
            {
                var colorByValue = item.GetColorByValue();
                if (colorByValue == null)
                {
                    WindowMessage.ShowDialog(string.Format("Invalid one or more 'Color By Value' input."), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                    return false;
                }
                HudColorsByValueParams.ColorsByValue.Add(colorByValue);
            }

            return true;
        }

        private void Button_SaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            if (CollectParams())
            {
                HudColorsByValueParams.Save();
                Close();
            }
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Apply_Click(object sender, RoutedEventArgs e)
        {
            if (CollectParams())
            {
                HudColorsByValueParams.Save();
            }
        }
    }
}

