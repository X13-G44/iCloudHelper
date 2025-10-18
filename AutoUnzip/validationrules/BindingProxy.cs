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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace AutoUnzip.ValidationRules
{
    /// <summary>
    /// See this listening. We need a Binding Proxy for our Data Context (-> Bindings with dependency properties on our wrapper instances)
    /// Based on https://social.technet.microsoft.com/wiki/contents/articles/31422.wpf-passing-a-data-bound-value-to-a-validation-rule.aspx
    /// </summary>
    public class BindingProxy : System.Windows.Freezable
    {
        protected override Freezable CreateInstanceCore ()
        {
            return new BindingProxy ();
        }

        public object Data
        {
            get { return (object) GetValue (DataProperty); }
            set { SetValue (DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register ("Data", typeof (object), typeof (BindingProxy), new PropertyMetadata (null));
    }
}
