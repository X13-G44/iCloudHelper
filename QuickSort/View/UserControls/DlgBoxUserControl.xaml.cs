/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				09.01.2026
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2025 Christian Harscher (alias X13-G44)
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the Apache License as
/// published by the Free Software Foundation, either version 2 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// Apache License for more details.
///
/// You should have received a copy of the Apache License
/// along with this program. If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
///      
/// ////////////////////////////////////////////////////////////////////////



using QuickSort.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;



namespace QuickSort.View.UserControls
{
    public partial class DlgBoxUserControl : UserControl
    {
        public DlgBoxUserControl ()
        {
            InitializeComponent ();

            PART_DLGBOXUSERCONTROL.Visibility = Visibility.Collapsed;
        }



        public DlgBoxViewModel Configuration
        {
            get { return (DlgBoxViewModel) GetValue (ConfigurationProperty); }
            set { SetValue (ConfigurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Configuration. This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.Register ("Configuration", typeof (DlgBoxViewModel), typeof (DlgBoxUserControl), new PropertyMetadata (null, OnConfigurationPropertyChanged));



        private static void OnConfigurationPropertyChanged (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DlgBoxUserControl;
            var vm = e.NewValue as DlgBoxViewModel;
            var vmOld = e.OldValue as DlgBoxViewModel;



            if (vmOld != null)
            {
                // Try to clean / delete / remove the (validation rule) binding between our PART_TEXTBOX instance
                // the old DlgBoxViewModel-Configuration instance.
                // Note: DlgBoxViewModel.TextBox.Text property is bind to PART_TEXTBOX.Text!

                // Code below not need, since we remove the binding completely.
                //
                //var bindingExpression = control.PART_TEXTBOX.GetBindingExpression (TextBox.TextProperty);
                //if (bindingExpression != null)
                //{
                //    var binding = bindingExpression.ParentBinding;
                //    if (binding != null)
                //    {
                //        // Remove the validation rules
                //        binding.ValidationRules.Clear ();
                //    }
                //
                //    // Set Binding (again), to the updated validation rules becomes active.
                //    control.PART_TEXTBOX.SetBinding (TextBox.TextProperty, binding);
                //}

                // Remove binding
                BindingOperations.ClearBinding (control.PART_TEXTBOX, TextBox.TextProperty);
            }

            if (vm == null)
            {
                control.PART_DLGBOXUSERCONTROL.Visibility = Visibility.Hidden;

                return;
            }

            control.PART_DLGBOXUSERCONTROL.Visibility = Visibility.Visible;

            control.PART_SYMBOL.Text = GetDlgSymbol (vm.Type);
            control.PART_TITLE.Text = vm.Title;
            control.PART_MESSAGE.Text = vm.Message;
            control.PART_MESSAGE.MinWidth = vm.CenterButton != null ? 500 : 300;
            control.PART_MESSAGE.MaxWidth = vm.CenterButton != null ? 500 : 500;

            control.PART_TEXTBOX.Visibility = Visibility.Hidden;
            if (vm.TextBox != null)
            {
                control.PART_TEXTBOX.Visibility = Visibility.Visible;
                control.PART_TEXTBOX.Text = vm.TextBox.Text;

                // Make a new binding between DlgBoxViewModel.TextBox.Text property to bind to PART_TEXTBOX.Text and
                // also set the validation rules - this is necessary.
                var binding = new Binding ();
                binding.Source = vm.TextBox;
                binding.Path = new PropertyPath ("Text");
                binding.Mode = BindingMode.TwoWay;
                binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

                if (vm.TextBox.ValidationRules.Count > 0)
                {
                    // Add validation rules
                    foreach (var rule in vm.TextBox.ValidationRules)
                    {
                        binding.ValidationRules.Add (rule);
                    }
                }

                // Set binding to activate them and enable the rules.
                BindingOperations.SetBinding (control.PART_TEXTBOX, TextBox.TextProperty, binding);
            }

            control.PART_BTN_LEFT.Visibility = Visibility.Hidden;
            if (vm.LeftButton != null)
            {
                control.PART_BTN_LEFT.Visibility = Visibility.Visible;
                control.PART_BTN_LEFT.Content = vm.LeftButton.Text;
                control.PART_BTN_LEFT.Tag = GetButtonSymbol (vm.LeftButton.Symbol);

                control.PART_BTN_LEFT.Command = new RelayCommand (
                    (param) =>
                    {
                        vm.LeftButton.Action.Invoke (vm);
                        control.PART_DLGBOXUSERCONTROL.Visibility = Visibility.Hidden;
                    }
                );
            }

            control.PART_BTN_CENTER.Visibility = Visibility.Hidden;
            if (vm.CenterButton != null)
            {
                control.PART_BTN_CENTER.Visibility = Visibility.Visible;
                control.PART_BTN_CENTER.Content = vm.CenterButton.Text;
                control.PART_BTN_CENTER.Tag = GetButtonSymbol (vm.CenterButton.Symbol);

                control.PART_BTN_CENTER.Command = new RelayCommand (
                    (param) =>
                    {
                        vm.CenterButton.Action.Invoke (vm);
                        control.PART_DLGBOXUSERCONTROL.Visibility = Visibility.Hidden;
                    }
                );
            }

            control.PART_BTN_RIGHT.Visibility = Visibility.Hidden;
            if (vm.RightButton != null && vm.RightButton.Action != null)
            {
                control.PART_BTN_RIGHT.Visibility = Visibility.Visible;
                control.PART_BTN_RIGHT.Content = vm.RightButton.Text;
                control.PART_BTN_RIGHT.Tag = GetButtonSymbol (vm.RightButton.Symbol);

                control.PART_BTN_RIGHT.Command = new RelayCommand (
                    (param) =>
                    {
                        vm.RightButton.Action.Invoke (vm);
                        control.PART_DLGBOXUSERCONTROL.Visibility = Visibility.Hidden;
                    }
                );
            }
        }



        private static String GetDlgSymbol (DlgBoxType symbol)
        {
            switch (symbol)
            {
                case DlgBoxType.Question:
                    {
                        return "❔";
                    }

                case DlgBoxType.Warning:
                    {
                        return "⚠";
                    }

                case DlgBoxType.Error:
                    {
                        return "❌";
                        //return "❗";
                    }

                default:
                    {
                        return "";
                    }
            }
        }



        private static String GetButtonSymbol (DlgBoxButtonSymbol symbol)
        {
            switch (symbol)
            {
                case DlgBoxButtonSymbol.Empty:
                    {
                        return "";
                    }

                case DlgBoxButtonSymbol.Check:
                    {
                        return "\uE10B";    // "&#xE10B;"
                    }

                case DlgBoxButtonSymbol.Cross:
                    {
                        return "\uE10A";    // "&#xE10A;"
                    }

                case DlgBoxButtonSymbol.Move:
                    {
                        return "\uE1AE";    // "&#xE1AE;"
                    }

                case DlgBoxButtonSymbol.OpenFolder:
                    {
                        return "\uE19C";    // "&#xE19C;"
                    }

                default:
                    {
                        return "";
                    }
            }
        }
    }



    public enum DlgBoxType
    {
        Question,
        Error,
        Warning,
    }



    public enum DlgBoxButtonSymbol
    {
        Empty,
        Check,
        Cross,
        Move,
        OpenFolder,
    }
}
