using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CarouselKBSample.Controls
{
    public class ControlBase : StackLayout
    {
        protected Label _label;
        private string _value;
        private bool _required;
        private bool _validationStatus;
        

        /// <summary>
        /// 
        /// </summary>
        public virtual string ControlValue
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }

      
        /// <summary>
        /// Validation fails if this is true but control has no value.
        /// </summary>
        public bool Required
        {
            get
            {
                return _required;
            }
            set
            {
                _required = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ValidationStatus
        {
            get
            {
                return _validationStatus;
            }
            set
            {
                _validationStatus = value;
                if (!value)
                {
                    _label.TextColor = Palette._022;
                }
                else
                {
                    _label.TextColor = Palette._024;
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public string Title
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ControlBase()
        {
            _validationStatus = true;
            _label = new Label()
            {
                TextColor = Palette._024,
                FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
            };
            Children.Add(_label);
            Padding = 5;
        }
    }
}
