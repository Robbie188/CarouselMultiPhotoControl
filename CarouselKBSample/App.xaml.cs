using CarouselKBSample.FileSystem;
using Xamarin.Forms;

namespace CarouselKBSample
{
    public partial class App : Application
	{
        private static App _app;
        public static App CurrentApp
        {
            get
            {
                return _app;
            }
        }
        public string CurrentFormAttachmentDirectory { get; set; }
        public App()
		{
			InitializeComponent();
			MainPage = new CarouselKBSamplePage();
            _app = this;
        }

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
