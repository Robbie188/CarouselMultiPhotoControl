using System;
using System.Collections.Generic;
using System.Linq;
using Syncfusion.SfCarousel.XForms.iOS;
using Foundation;
using UIKit;

namespace CarouselKBSample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			new SfCarouselRenderer();
			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}
