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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PsHandler.UI;
using PsHandler.UI.ToolTips;

namespace PsHandler.TableTiler
{
    /// <summary>
    /// Interaction logic for WindowTableTileEdit.xaml
    /// </summary>
    public partial class WindowTableTileEdit : Window, IFilter
    {
        public TableTile TableTile;
        public bool Saved;
        private WindowWindowsInfo _windowWindowsInfo;
        private WindowGenerateLayout _windowGenerateLayout;

        public WindowTableTileEdit(Window owner, TableTile tableTile = null)
        {
            InitializeComponent();
            Owner = owner;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            TableTile = tableTile ?? new TableTile();

            // Hook

            Grid_Main.MouseLeftButtonDown += (sender, args) => Grid_Main.Focus();

            Closing += (sender, args) =>
            {
                if (_windowWindowsInfo != null)
                {
                    _windowWindowsInfo.Close();
                }
                if (_windowGenerateLayout != null)
                {
                    _windowGenerateLayout.Close();
                }
            };

            TextBox_RegexWindowTitle.TextChanged += (sender, args) => UpdateIFilter();
            TextBox_RegexWindowClass.TextChanged += (sender, args) => UpdateIFilter();

            TextBox_XYWidthHeight.TextChanged += (sender, args) => UCScreenPreview_Main_Update();
            CheckBox_SortTournamentsByStartingTime.Checked += (sender, args) => UCScreenPreview_Main_Update();
            CheckBox_SortTournamentsByStartingTime.Unchecked += (sender, args) => UCScreenPreview_Main_Update();
            CheckBox_EnableAutoTile.Checked += (sender, args) =>
            {
                RadioButton_ToTheTop.IsEnabled = true;
                RadioButton_ToTheClosest.IsEnabled = true;
                Label_DisabledToTheTop.Visibility = Visibility.Collapsed;
                Label_DisabledToTheClosest.Visibility = Visibility.Collapsed;
            };
            CheckBox_EnableAutoTile.Unchecked += (sender, args) =>
            {
                RadioButton_ToTheTop.IsEnabled = false;
                RadioButton_ToTheClosest.IsEnabled = false;
                Label_DisabledToTheTop.Visibility = Visibility.Visible;
                Label_DisabledToTheClosest.Visibility = Visibility.Visible;
            };
            SizeChanged += (sender, args) => UCScreenPreview_Main_Update();
            Loaded += (sender, args) =>
            {
                string text = TextBox_XYWidthHeight.Text; TextBox_XYWidthHeight.Text = text + "1"; TextBox_XYWidthHeight.Text = text;
                text = TextBox_RegexWindowTitle.Text; TextBox_RegexWindowTitle.Text = text + "1"; TextBox_RegexWindowTitle.Text = text;
                text = TextBox_RegexWindowClass.Text; TextBox_RegexWindowClass.Text = text + "1"; TextBox_RegexWindowClass.Text = text;
            };

            // Seed

            TextBox_Name.Text = TableTile.Name;
            TextBoxHotkey_Hotkey.KeyCombination = TableTile.KeyCombination;
            CheckBox_SortTournamentsByStartingTime.IsChecked = TableTile.SortByStartingHand;
            TextBox_RegexWindowTitle.Text = TableTile.RegexWindowTitle == null ? "" : TableTile.RegexWindowTitle.ToString();
            TextBox_RegexWindowClass.Text = TableTile.RegexWindowClass == null ? "" : TableTile.RegexWindowClass.ToString();
            StringBuilder sb = new StringBuilder(); foreach (var xywh in TableTile.XYWHs) sb.Append(string.Format("{0} {1} {2} {3}{4}", xywh.X, xywh.Y, xywh.Width, xywh.Height, Environment.NewLine)); TextBox_XYWidthHeight.Text = sb.ToString();
            CheckBox_SortTournamentsByStartingTime.IsChecked = TableTile.SortByStartingHand;
            CheckBox_BringToFront.IsChecked = TableTile.BringToFront;
            CheckBox_EnableAutoTile.IsChecked = TableTile.AutoTile;
            switch (TableTile.AutoTileMethod)
            {
                case AutoTileMethod.ToTheTopSlot:
                    RadioButton_ToTheTop.IsChecked = true;
                    break;
                case AutoTileMethod.ToTheClosestSlot:
                    RadioButton_ToTheClosest.IsChecked = true;
                    break;
            }
            TextBox_TableCountEqualOrGreaterThan.Text = string.Format("{0}", TableTile.TableCountEqualOrGreaterThan);
            TextBox_TableCountEqualOrLessThan.Text = string.Format("{0}", TableTile.TableCountEqualOrLessThan);

            // ToolTips

            GroupBox_XYWidthHeight.ToolTip = "Create slots (one per line): X Y Width Height";
            ToolTipService.SetShowDuration(GroupBox_XYWidthHeight, 60000);

            CheckBox_SortTournamentsByStartingTime.ToolTip = "Older tournaments will target upper slots (cash tables won't be affected).";
            ToolTipService.SetShowDuration(CheckBox_SortTournamentsByStartingTime, 60000);

            Label_RegexWindowTitle.ToolTip = "Regular Expression (Regex) for window's title. You can learn and test regex at: http://rubular.com/ or any other similar site.";
            ToolTipService.SetShowDuration(Label_RegexWindowTitle, 60000);

            Label_RegexWindowClass.ToolTip = "Regular Expression (Regex) for window's class name. You can learn and test regex at: http://rubular.com/ or any other similar site.";
            ToolTipService.SetShowDuration(Label_RegexWindowClass, 60000);

            RadioButton_ToTheTop.ToolTip = new UCToolTipTableTilerAutoTileTop();
            ToolTipService.SetShowDuration(RadioButton_ToTheTop, 60000);

            RadioButton_ToTheClosest.ToolTip = new UCToolTipTableTilerAutoTileClosest();
            ToolTipService.SetShowDuration(RadioButton_ToTheClosest, 60000);

            Label_DisabledToTheTop.ToolTip = new UCToolTipTableTilerAutoTileTop();
            ToolTipService.SetShowDuration(Label_DisabledToTheTop, 60000);

            Label_DisabledToTheClosest.ToolTip = new UCToolTipTableTilerAutoTileClosest();
            ToolTipService.SetShowDuration(Label_DisabledToTheClosest, 60000);

            UCScreenPreview_Main_Update();
        }

        private void UCScreenPreview_Main_Update()
        {
            var config = GetXYWHs();
            TextBox_XYWidthHeight.Background = config == null ? new SolidColorBrush(Colors.MistyRose) : new SolidColorBrush(Colors.Honeydew);
            try
            {
                UCScreenPreview_Main.Update(config, CheckBox_SortTournamentsByStartingTime.IsChecked == true);
            }
            catch
            {
            }
        }

        private System.Drawing.Rectangle[] GetXYWHs()
        {
            string text = TextBox_XYWidthHeight.Text;
            string[] lines = text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            List<System.Drawing.Rectangle> config = new List<System.Drawing.Rectangle>();
            foreach (string line in lines)
            {
                string[] words = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length != 4) return null;
                int[] values = new int[4]; ;
                for (int i = 0; i < 4; i++) if (!int.TryParse(words[i], out values[i])) return null;
                if (values[2] <= 0 || values[3] <= 0) return null;
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(values[0], values[1], values[2], values[3]);
                config.Add(rect);
            }
            return config.ToArray();
        }

        private void Button_WindowsInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_windowWindowsInfo != null) _windowWindowsInfo.Close();
            _windowWindowsInfo = new WindowWindowsInfo(this);
            _windowWindowsInfo.Show();
        }

        private void Button_GenerateLayout_Click(object sender, RoutedEventArgs e)
        {
            if (_windowGenerateLayout != null) _windowGenerateLayout.Close();
            _windowGenerateLayout = new WindowGenerateLayout();
            _windowGenerateLayout.Show();
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            if (TextBox_Name.Text.Length == 0)
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", Label_Name.Content), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            if (App.TableTileManager.GetTableTilesCopy().Any(o => o.Name.ToLowerInvariant().Equals(TextBox_Name.Text.ToLowerInvariant())))
            {
                WindowMessage.ShowDialog("Cannot save. Duplicate names.", "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            System.Drawing.Rectangle[] xywhs = GetXYWHs();
            if (xywhs == null)
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", "X Y Width Height"), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            try
            {
                new Regex(TextBox_RegexWindowTitle.Text);
            }
            catch
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", Label_RegexWindowTitle.Content), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            try
            {
                new Regex(TextBox_RegexWindowClass.Text);
            }
            catch
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", Label_RegexWindowClass.Content), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            try
            {
                if (TextBox_TableCountEqualOrGreaterThan.Text.Length > 0)
                {
                    TableTile.TableCountEqualOrGreaterThan = int.Parse(TextBox_TableCountEqualOrGreaterThan.Text);
                }
                else
                {
                    TableTile.TableCountEqualOrGreaterThan = 1;
                }
            }
            catch
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", TextBox_TableCountEqualOrGreaterThan.Text), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }
            try
            {
                if (TextBox_TableCountEqualOrLessThan.Text.Length > 0)
                {
                    TableTile.TableCountEqualOrLessThan = int.Parse(TextBox_TableCountEqualOrLessThan.Text);
                }
                else
                {
                    TableTile.TableCountEqualOrLessThan = 100;
                }
            }
            catch
            {
                WindowMessage.ShowDialog(string.Format("Invalid '{0}' input.", TextBox_TableCountEqualOrLessThan.Text), "Error saving", WindowMessageButtons.OK, WindowMessageImage.Error, this);
                return;
            }

            TableTile.Name = TextBox_Name.Text;
            TableTile.KeyCombination = TextBoxHotkey_Hotkey.KeyCombination;
            TableTile.SortByStartingHand = CheckBox_SortTournamentsByStartingTime.IsChecked == true;
            TableTile.BringToFront = CheckBox_BringToFront.IsChecked == true;
            TableTile.RegexWindowTitle = new Regex(TextBox_RegexWindowTitle.Text);
            TableTile.RegexWindowClass = new Regex(TextBox_RegexWindowClass.Text);
            TableTile.XYWHs = xywhs;
            TableTile.AutoTile = CheckBox_EnableAutoTile.IsChecked == true;
            TableTile.AutoTileMethod = RadioButton_ToTheTop.IsChecked == true ? AutoTileMethod.ToTheTopSlot : RadioButton_ToTheClosest.IsChecked == true ? AutoTileMethod.ToTheClosestSlot : /*unknown default = to the top slot*/ AutoTileMethod.ToTheTopSlot;

            Saved = true;
            Close();
        }

        private void Button_Close_Click(object sender, RoutedEventArgs e)
        {
            Saved = false;
            Close();
        }

        // IFilter

        private readonly object _iFilterLock = new object();
        public Regex _regexWindowTitle = new Regex("");
        public Regex _regexWindowClass = new Regex("");
        public Regex RegexWindowTitle
        {
            get
            {
                lock (_iFilterLock)
                {
                    return _regexWindowTitle;
                }
            }
        }
        public Regex RegexWindowClass
        {
            get
            {
                lock (_iFilterLock)
                {
                    return _regexWindowClass;
                }
            }
        }

        private void UpdateIFilter()
        {
            lock (_iFilterLock)
            {
                try
                {
                    _regexWindowTitle = new Regex(TextBox_RegexWindowTitle.Text);
                    TextBox_RegexWindowTitle.Background = Brushes.Honeydew;
                }
                catch
                {
                    _regexWindowTitle = new Regex("");
                    TextBox_RegexWindowTitle.Background = Brushes.MistyRose;
                }
                try
                {
                    _regexWindowClass = new Regex(TextBox_RegexWindowClass.Text);
                    TextBox_RegexWindowClass.Background = Brushes.Honeydew;
                }
                catch (Exception)
                {
                    _regexWindowClass = new Regex("");
                    TextBox_RegexWindowClass.Background = Brushes.MistyRose;
                }
            }
        }
    }
}
