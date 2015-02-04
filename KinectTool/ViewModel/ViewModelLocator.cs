/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:KinectTool"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;

namespace KinectTool.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
           ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

           SimpleIoc.Default.Register<MainViewModel>();
           SimpleIoc.Default.Register<KinectEmulateViewModel>();
           SimpleIoc.Default.Register<KinectRecordViewModel>();
           SimpleIoc.Default.Register<RecordingConvertorViewModel>();
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public KinectRecordViewModel Recorder
        {
            get
            {
                return ServiceLocator.Current.GetInstance<KinectRecordViewModel>();
            }
        }

        public KinectEmulateViewModel Emulator
        {
            get
            {
                return ServiceLocator.Current.GetInstance<KinectEmulateViewModel>();
            }
        }

        public RecordingConvertorViewModel RecordingConvertor
        {
            get
            {
                return ServiceLocator.Current.GetInstance<RecordingConvertorViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
            ServiceLocator.Current.GetInstance<MainViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<KinectEmulateViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<KinectRecordViewModel>().Cleanup();
            ServiceLocator.Current.GetInstance<RecordingConvertorViewModel>().Cleanup();
        }
    }
}