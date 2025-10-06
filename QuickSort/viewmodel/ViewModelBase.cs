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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;



namespace QuickSort.viewmodel
{
    /// <summary>
    /// Base class for all ViewModel classes in the application. Provides support for 
    /// property changes notification as well as for data validation error notifications.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        #region INotifyPropertyChanged stuff


        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;



        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>
        protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = "")
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
            }
        }


        #endregion


        #region INotifyDataErrorInfo stuff


        /// <summary>
        /// Dictionary of property names and their (possible multiple) error messages.
        /// </summary>
        private readonly Dictionary<string, List<string>> _ErrorsByPropertyName = new Dictionary<string, List<string>> ();



        /// <summary>
        /// Gets a value indicating whether there are any validation errors.
        /// </summary>
        public bool HasErrors => _ErrorsByPropertyName.Any ();



        /// <summary>
        /// Raised when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;



        /// <summary>
        /// Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public IEnumerable GetErrors (string propertyName)
        {
            return _ErrorsByPropertyName.ContainsKey (propertyName) ? _ErrorsByPropertyName[propertyName] : null;
        }



        /// <summary>
        /// Remove all error messages for the given property.
        /// 
        /// This is an alternative to using ValidationRules Objects in the view.
        /// It allows to generate more complex rules in the viewmodel.
        /// </summary>
        /// <param name="propertyName"></param>
        public void ClearErrors (string propertyName)
        {
            _ErrorsByPropertyName.Remove (propertyName);

            OnErrorChanged (propertyName);
        }



        /// <summary>
        /// Add an error message to the list of errors for the given property.
        /// 
        /// This is an alternative to using ValidationRules Objects in the view.
        /// It allows to generate more complex rules in the viewmodel.
        /// </summary>
        /// <param name="errorMessage">Error message to add to a property</param>
        /// <param name="propertyName">Property name to add the message for it</param>
        public void AddError (string errorMessage, string propertyName)
        {
            if (!_ErrorsByPropertyName.ContainsKey (propertyName))
            {
                _ErrorsByPropertyName.Add (propertyName, new List<string> ());
            }

            _ErrorsByPropertyName[propertyName].Add (errorMessage);

            OnErrorChanged (propertyName);
        }



        /// <summary>
        /// Fires the ErrorsChanged event for the given property.
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnErrorChanged (string propertyName)
        {
            ErrorsChanged?.Invoke (this, new DataErrorsChangedEventArgs (propertyName));
        }


        #endregion
    }
}
