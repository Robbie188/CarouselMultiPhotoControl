using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CarouselKBSample.FileSystem;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Newtonsoft.Json.Linq;
using System.IO;

namespace CarouselKBSample.Controls
{
    public partial class MultiPhotoControl : ControlBase, INotifyPropertyChanged
    {
        const string PLACE_HOLDER_FILENAME = "camera_blue.png";
        const int PHOTO_COMPRESSION_QUALITY = 75;
        private CarouselViewModel _carouselViewModel;
        private int _minPhotos;
        private int _maxPhotos;
        private bool _allowAnnotate;

        public bool PreviousEnabled
        {
            get
            {
                if (carousel == null)
                    return false;
                if (carousel.ItemsSource.Count > 1 && carousel.SelectedIndex > 1)
                    return true;
                else
                    return false;
            }
        }


        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                try
                {
                    var images = new ObservableCollection<CarouselModel>();
                    int count = 0;
                    if (!string.IsNullOrWhiteSpace(value) && value != "[]")
                    {
                        JArray allImages = JArray.Parse(value);
                        foreach (string fileName in allImages)
                        {
                            if (!string.IsNullOrWhiteSpace(fileName))
                            {
                                images.Add(new CarouselModel(App.CurrentApp.CurrentFormAttachmentDirectory + '/' + fileName));
                            }
                            count++;
                        }
                    }
                    else
                    {
                        images.Add(new CarouselModel(PLACE_HOLDER_FILENAME));
                    }
                    _carouselViewModel.ImageCollection = images;
                    if (carousel != null)
                    {
                        carousel.DataSource = _carouselViewModel.ImageCollection;
                    }
                    SetSummaryText();
                    ControlValue = value;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private int CurrentPhotoCount
        {
            get
            {
                int count = 0;
                if (!String.IsNullOrEmpty(Text))
                {
                    JArray items = JArray.Parse(Text);
                    count = items.Count;
                }
                return count;
            }
        }

        private void SetSummaryText()
        {

            string result = string.Format(
                "TAKEN: {0} ", CurrentPhotoCount);
            result += _minPhotos > 0 ? string.Format("MIN: {0} ", _minPhotos) : "";
            result += _maxPhotos > 0 ? string.Format("MAX: {0}", _maxPhotos) : "";
            SummaryLabel.Text = result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        private async Task DeletePhotoAsync(string filePath)
        {
            //remove from the JArray String
            JArray imageArray = JArray.Parse(Text);
            var fileName = Path.GetFileName(filePath);
            var toRemove = imageArray.Children<JToken>().Where(o => o.ToString() == fileName).FirstOrDefault();
            imageArray.Remove(toRemove);

            //delete file from filesystem
            bool result = await FileHelper.DeleteFile(filePath);
            if (!result)
            {
                System.Diagnostics.Debug.WriteLine("error deleting photo");
            }
            SetValue(TextProperty, imageArray.ToString());
        }



        public int MinPhotos { get => _minPhotos; set { _minPhotos = value; SetSummaryText(); } }
        public int MaxPhotos { get => _maxPhotos; set { _maxPhotos = value; SetSummaryText(); } }
        public bool AllowAnnotate { get => _allowAnnotate; set => _allowAnnotate = value; }

        /// <summary>
        /// 
        /// </summary>
        public static readonly BindableProperty TextProperty = BindableProperty.Create("Text", typeof(string), typeof(MultiPhotoControl), "", BindingMode.TwoWay, null, handleSourceChanged);

        public MultiPhotoControl()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += async (s, e) =>
            {
                //check if take new photo image or something else
                //_carouselViewModel.ImageCollection.Add(new CarouselModel("camera_green.png"));
                try
                {
                    if (((FileImageSource)((Image)s).Source).File == PLACE_HOLDER_FILENAME)
                    {
                        await TakeAPhoto();
                    }
                    else
                    {
                        string action;
                        if (AllowAnnotate)
                        {
                            action = await App.CurrentApp.MainPage.DisplayActionSheet("Photo Options",
                            "cancel",
                            "delete",
                            "annotate");
                        }
                        else
                        {
                            action = await App.CurrentApp.MainPage.DisplayActionSheet("Photo Options",
                            "cancel",
                            "delete");
                        }

                        if (action == "delete")
                        {
                            await DeletePhotoAsync(((FileImageSource)((Image)s).Source).File);
                        }
                        //if (action == TextResources.camera_option_annotate_photo)
                        //{
                        //    var pathToFileToEdit = ((FileImageSource)((Image)s).Source).File;
                        //    SfImageEditorPage editor = new SfImageEditorPage(pathToFileToEdit);
                        //    editor.Editor.ImageSaved += Editor_ImageSaved;
                        //    await Navigation.PushAsync(new NavigationPage(editor));
                        //}
                    }
                }
                catch (Exception ex)
                {

                }
            };

            var itemTemplate = new DataTemplate(() =>
            {
                var grid = new Grid();
                var nameLabel = new Image();
                nameLabel.GestureRecognizers.Add(tapGestureRecognizer);
                nameLabel.SetBinding(Image.SourceProperty, "Image");
                grid.Children.Add(nameLabel);
                return grid;
            });
            carousel.ItemTemplate = itemTemplate;
            _carouselViewModel = new CarouselViewModel();
            carousel.BindingContext = _carouselViewModel;
            carousel.DataSource = _carouselViewModel.ImageCollection;
        }

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bindable"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        private static void handleSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            MultiPhotoControl control = bindable as MultiPhotoControl;
            if (control != null)
            {
                if (newValue != null)
                {
                    control.Text = newValue.ToString();
                }
            }
        }

        private void PreviousClicked(object sender, EventArgs e)
        {
            if (carousel.SelectedIndex > 0)
                carousel.SelectedIndex--;
        }

        private async void TakeNewClicked(object sender, EventArgs e)
        {
            await TakeAPhoto();
        }

        private async Task TakeAPhoto()
        {

            if (CurrentPhotoCount >= _maxPhotos && _maxPhotos > 0)
                return;

            bool result = true;
            await CrossMedia.Current.Initialize();
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Photos);

            //deal with saving image to gallery permissions
            //if (Settings.SaveImagesToAlbum && storageStatus != PermissionStatus.Granted)
            //{
            //    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Photos });
            //    storageStatus = results[Permission.Photos];
            //    if (storageStatus != PermissionStatus.Granted)
            //    {
            //        Settings.SaveImagesToAlbum = false;
            //        await App.CurrentApp.DisplayAlertAsync(TextResources.dialog_permission_storage_issue_message, TextResources.dialog_permission_issue_title);
            //    }
            //}

            //deal with permissions to access the camera
            if (cameraStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                cameraStatus = results[Permission.Camera];
                //if there are still no permissions then give up
                if (cameraStatus != PermissionStatus.Granted)
                {
                    //await App.CurrentApp.DisplayAlertAsync(TextResources.dialog_permission_camera_error_message, TextResources.dialog_permission_issue_title);
                    result = false;
                    return;
                }
            }

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                result = false;
                return;
            }
            var filename = Guid.NewGuid() + ".png";

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "c365 OnSite",
                Name = filename,
                SaveToAlbum = false,
                CompressionQuality = PHOTO_COMPRESSION_QUALITY,
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Small
            });

            if (file == null)
            {
                result = false;
                return;
            }
            //TODO Delete previous image if it exists

            //move image to new location
            await FileHelper.MoveFileAsync(file.Path, App.CurrentApp.CurrentFormAttachmentDirectory + "/" + filename);

            JArray photosArray;
            if (string.IsNullOrWhiteSpace(Text))
            {
                photosArray = new JArray();
                photosArray.Add(filename);
            }
            else
            {
                photosArray = JArray.Parse(Text);
                photosArray.Add(filename);
            }
            SetValue(TextProperty, photosArray.ToString());
        }

        private void NextClicked(object sender, EventArgs e)
        {
            if (carousel.SelectedIndex < (_carouselViewModel.ImageCollection.Count - 1))
                carousel.SelectedIndex++;
        }

        private void ConfigureControlButtons()
        {

        }

        //public override void Validate()
        //{
        //    base.Validate();
        //    if (_minPhotos > 0 && CurrentPhotoCount < _minPhotos)
        //    {
        //        ValidationStatus = false;
        //    }
        //}
    }

    public class CarouselModel
    {
        public CarouselModel(string imageString)
        {
            Image = imageString;
        }
        private string _image;
        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }
    }

    public class CarouselViewModel : INotifyPropertyChanged
    {
        public CarouselViewModel()
        {
            ImageCollection.Add(new CarouselModel("camera_blue.png"));
        }

        private ObservableCollection<CarouselModel> imageCollection = new ObservableCollection<CarouselModel>();
        public ObservableCollection<CarouselModel> ImageCollection
        {
            get { return imageCollection; }
            set { imageCollection = value; OnPropertyChanged("ImageCollection"); }
        }

        #region INotifyPropertyChanged



        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
    
}