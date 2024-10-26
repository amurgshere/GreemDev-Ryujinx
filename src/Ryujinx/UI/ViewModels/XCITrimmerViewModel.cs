using DynamicData.Binding;
using Ryujinx.UI.App.Common;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class XCITrimmerViewModel : BaseModel
    {
        private readonly ApplicationLibrary _applicationLibrary;
        private ObservableCollectionExtended<XCITrimmerApplicationData> _xciApplications;

        public ObservableCollectionExtended<XCITrimmerApplicationData> XCIApplications
        {
            get => _xciApplications;
            set
            {
                _xciApplications = value;
                OnPropertyChanged();
            }
        }        
        public XCITrimmerViewModel(ApplicationLibrary applicationLibrary)
        {
            _applicationLibrary = applicationLibrary;
            // LoadXCIApplications();
        }

    }
}