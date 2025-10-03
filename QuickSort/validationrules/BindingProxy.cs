using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace QuickSort.validationrules
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
