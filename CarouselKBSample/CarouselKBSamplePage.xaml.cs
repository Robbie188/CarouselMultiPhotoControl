using System.Collections.Generic;
using System.Collections.ObjectModel;
using Syncfusion.SfCarousel.XForms;
using Xamarin.Forms;
using CarouselKBSample.FileSystem;

namespace CarouselKBSample
{
    public partial class CarouselKBSamplePage : ContentPage
    {
       
        public CarouselKBSamplePage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            if(string.IsNullOrWhiteSpace(App.CurrentApp.CurrentFormAttachmentDirectory))
                App.CurrentApp.CurrentFormAttachmentDirectory = await FileHelper.CreateFormDirectory("test", 1);
        }
        
    }
}
