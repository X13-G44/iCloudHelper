/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				06.10.2025
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
///
/// Code based on:
/// https://stackoverflow.com/questions/30030876/execute-command-after-animation
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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;



namespace AutoUnzip.help
{
    /// <summary>
    /// 
    /// This helper object is attached to storyboard instance and allows to execute a command when the storyboard "completed" event is triggered.
    /// It is for MVVM Pattern; and used in XAML code.
    /// 
    /// https://stackoverflow.com/questions/30030876/execute-command-after-animation
    /// 
    /// Use it like this:
    ///
    ///     <Storyboard TargetProperty = "Opacity" local:StoryboardHelper.CompletedCommand="{Binding Path=StoryCompletedCommand}">
    ///         <DoubleAnimation From = "0" To="1" Duration="0:0:5"/>
    ///     </Storyboard>
    /// 
    /// </summary>
    public static class StoryboardHelper
    {
        public static void SetCompletedCommand (DependencyObject o, ICommand value)
        {
            o.SetValue (CompletedCommandProperty, value);
        }

        public static ICommand GetCompletedCommand (DependencyObject o)
        {
            return (ICommand) o.GetValue (CompletedCommandProperty);
        }

        public static readonly DependencyProperty CompletedCommandProperty = 
            DependencyProperty.RegisterAttached ("CompletedCommand", typeof (ICommand), typeof (StoryboardHelper), new PropertyMetadata (null, OnCompletedCommandChanged));



        public static void SetCompletedCommandParameter (DependencyObject o, object value)
        {
            o.SetValue (CompletedCommandParameterProperty, value);
        }

        public static object GetCompletedCommandParameter (DependencyObject o)
        {
            return o.GetValue (CompletedCommandParameterProperty);
        }

        public static readonly DependencyProperty CompletedCommandParameterProperty = 
            DependencyProperty.RegisterAttached ("CompletedCommandParameter", typeof (object), typeof (StoryboardHelper), new PropertyMetadata (null));



        private static void OnCompletedCommandChanged (object sender, DependencyPropertyChangedEventArgs e)
        {
            var sb = sender as Storyboard;


            if (sb != null)
            {
                sb.Completed += (a, b) =>
                {
                    var command = GetCompletedCommand (sb);


                    if (command != null)
                    {
                        if (command.CanExecute (GetCompletedCommandParameter (sb)))
                        {
                            command.Execute (GetCompletedCommandParameter (sb));
                        }
                    }
                };
            }
        }
    }
}
