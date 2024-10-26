using Avalonia.Collections;
using DynamicData;
using DynamicData.Binding;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Common.Utilities;
using Ryujinx.UI.App.Common;
using Ryujinx.UI.Common.Models;
using System.IO;
using System.Linq;

namespace Ryujinx.Ava.UI.ViewModels
{
    public class XCITrimmerViewModel : BaseModel
    {
        private readonly Ryujinx.Common.Logging.XCIFileTrimmerLog _logger;
        private readonly ApplicationLibrary _applicationLibrary;
        private AvaloniaList<XCITrimmerApplicationModel> _xciTrimmerApplications = new();
        private AvaloniaList<XCITrimmerApplicationModel> _selectedXciTrimmerApplications = new();
        private AvaloniaList<XCITrimmerApplicationModel> _views = new();
        private string _search;

        public XCITrimmerViewModel(ApplicationLibrary applicationLibrary, Ryujinx.Common.Logging.XCIFileTrimmerLog logger)
        {
            _logger = logger;
            _applicationLibrary = applicationLibrary;
            LoadXCIApplications();
        }

        private void LoadXCIApplications()
        {
            var apps = _applicationLibrary.Applications.Items
                .Where(app => app.FileExtension == "XCI");

            foreach (var xciApp in apps.Select(xci => XCITrimmerApplicationModel.FromApplicationData(xci, _logger)))
            {
                XCITrimmerApplications.Add(xciApp);
                if (XCIFileTrimmer.CanTrim(xciApp.Path, _logger))
                {
                    SelectedXCITrimmerApplications.Add(xciApp);
                }

                OnPropertyChanged(nameof(UpdateCount));
            }

            Sort();
        }

        public AvaloniaList<XCITrimmerApplicationModel> XCITrimmerApplications
        {
            get => _xciTrimmerApplications;
            set
            {
                _xciTrimmerApplications = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateCount));
                Sort();
            }
        }
        public string UpdateCount
        {
            get => string.Format(LocaleManager.Instance[LocaleKeys.XCITrimmerCount], XCITrimmerApplications.Count);
        }
        public AvaloniaList<XCITrimmerApplicationModel> Views
        {
            get => _views;
            set
            {
                _views = value;
                OnPropertyChanged();
            }
        }

        public AvaloniaList<XCITrimmerApplicationModel> SelectedXCITrimmerApplications
        {
            get => _selectedXciTrimmerApplications;
            set
            {
                _selectedXciTrimmerApplications = value;
                OnPropertyChanged();
            }
        }

        private bool Filter<T>(T arg)
        {
            if (arg is XCITrimmerApplicationModel content)
            {
                return string.IsNullOrWhiteSpace(_search) || content.Name.ToLower().Contains(_search.ToLower()) || content.Path.ToLower().Contains(_search.ToLower());
            }

            return false;
        }

        public void SelectAll()
        {
            SelectedXCITrimmerApplications.Clear();
            SelectedXCITrimmerApplications.AddRange(XCITrimmerApplications);
        }

        public void SelectNone()
        {
            SelectedXCITrimmerApplications.Clear();
        }

        public void Select(XCITrimmerApplicationModel model)
        {
            SelectedXCITrimmerApplications.ReplaceOrAdd(model, model);
        }

        public void Deselect(XCITrimmerApplicationModel model)
        {
            SelectedXCITrimmerApplications.Remove(model);
        }

        public void Sort()
        {
            XCITrimmerApplications
                .OrderBy(it => it.Name)
                .ThenBy(it => it.Path)
                .AsObservableChangeSet()
                .Filter(Filter)
                .Bind(out var view).AsObservableList();

            var items = SelectedXCITrimmerApplications.ToArray();

            _views.Clear();
            _views.AddRange(view);

            foreach (XCITrimmerApplicationModel item in items)
            {
                SelectedXCITrimmerApplications.ReplaceOrAdd(item, item);
            }

            OnPropertyChanged(nameof(Views));
        }

        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
                Sort();
            }
        }
    }
}