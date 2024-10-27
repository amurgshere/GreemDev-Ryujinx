using Avalonia.Collections;
using DynamicData;
using Gommon;
using Avalonia.Threading;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Common.Utilities;
using Ryujinx.UI.App.Common;
using Ryujinx.UI.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Ryujinx.Common.Utilities.XCIFileTrimmer;

namespace Ryujinx.Ava.UI.ViewModels
{
    internal static class AvaloniaListExtensions
    {
        /// <summary>
        /// Adds or Replaces an item in an AvaloniaList irrespective of whether the item already exists
        /// </summary>
        /// <typeparam name="T">The type of the element in the AvaoloniaList</typeparam>
        /// <param name="list">The list containing the item to replace</param>
        /// <param name="item">The item to replace</param>
        /// <returns>True if the item was found and replaced, false if it was addded</returns>
        /// <remarks>
        /// The indexes on the AvaloniaList will only replace if the item does not match, 
        /// this causes the items to not be replaced if the Equality is customised on the 
        /// items. This method will instead find, remove and add the item to ensure it is
        /// replaced correctly.
        /// </remarks>
        public static bool AddOrReplaceWith<T>(this AvaloniaList<T> list, T item)
        {
            var index = list.IndexOf(item);

            if (index != -1)
            {
                list.RemoveAt(index);
                list.Insert(index, item);
                return true;
            }
            else
            {
                list.Add(item);
                return false;
            }
        }

        public static void AddOrReplaceMatching<T>(this AvaloniaList<T> list, IList<T> sourceList, IList<T> matchingList)
        {
            foreach (var match in matchingList)
            {
                var index = sourceList.IndexOf(match);
                if (index != -1)
                {
                    list.AddOrReplaceWith(sourceList[index]);
                }
                else
                {
                    list.AddOrReplaceWith(match);
                }
            }
        }
    }

    public class XCITrimmerViewModel : BaseModel
    {
        private const string _FileExtXCI = "XCI";

        private readonly Ryujinx.Common.Logging.XCIFileTrimmerLog _logger;
        private readonly ApplicationLibrary _applicationLibrary;
        private Optional<XCITrimmerFileModel> _processingApplication = null;
        private AvaloniaList<XCITrimmerFileModel> _allXCIFiles = new();
        private AvaloniaList<XCITrimmerFileModel> _selectedXCIFiles = new();
        private AvaloniaList<XCITrimmerFileModel> _displayedXCIFiles = new();
        private MainWindowViewModel _mainWindowViewModel;
        private CancellationTokenSource _cancellationTokenSource;
        private string _search;

        public XCITrimmerViewModel(MainWindowViewModel mainWindowViewModel)
        {
            _logger = new XCIFileTrimmerWindowLog(this);
            _mainWindowViewModel = mainWindowViewModel;
            _applicationLibrary = _mainWindowViewModel.ApplicationLibrary;
            LoadXCIApplications();
        }

        private void LoadXCIApplications()
        {
            var apps = _applicationLibrary.Applications.Items
                .Where(app => app.FileExtension == _FileExtXCI);

            foreach (var xciApp in apps)
            {
                AddOrUpdateApplication(xciApp, true);
            }

            OnPropertyChanged(nameof(UpdateCount));
            SortAndFilter();
        }

        private bool AddOrUpdateApplication(XCITrimmerFileModel xci, bool suppressChanged = false, bool autoSelect = true)
        {
            var xciApp = _applicationLibrary.Applications.Items.First(app => app.FileExtension == _FileExtXCI && app.Path == xci.Path);
            return AddOrUpdateApplication(xciApp, false);
        }

        private bool AddOrUpdateApplication(ApplicationData xciApp, bool suppressChanged = false, bool autoSelect = true)
        {
            XCITrimmerFileModel xci = XCITrimmerFileModel.FromApplicationData(xciApp, _logger);
            bool replaced = _allXCIFiles.AddOrReplaceWith(xci);
            _displayedXCIFiles.AddOrReplaceWith(xci);

            if (autoSelect && xci.Trimmable)
            {
                _selectedXCIFiles.AddOrReplaceWith(xci);
            }

            if (!suppressChanged)
            {
                if (!replaced)
                {
                    OnPropertyChanged(nameof(UpdateCount));
                }

                SortAndFilter();
            }

            return replaced;
        }

        private void PerformOperation(bool trim)
        {
            if (Processing)
            {
                return;
            }

            Processing = true;
            var cancellationToken = _cancellationTokenSource.Token;

            Thread XCIFileTrimThread = new(() =>
            {
                var toProcess = Sort(SelectedXCIFiles).ToList();
                var viewsSaved = DisplayedXCIFiles.ToList();

                Dispatcher.UIThread.Post(() =>
                {
                    _selectedXCIFiles.Clear();
                    _displayedXCIFiles.Clear();
                    _displayedXCIFiles.AddRange(toProcess);
                });

                try
                {
                    foreach (var xciApp in toProcess)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var trimmer = new XCIFileTrimmer(xciApp.Path, _logger);

                        Dispatcher.UIThread.Post(() =>
                        {
                            ProcessingApplication = xciApp;
                        });

                        try
                        {
                            var outcome = OperationOutcome.Undetermined;
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            if (trim)
                            {
                                outcome = trimmer.Trim(cancellationToken);
                            }
                            else
                            {
                                outcome = trimmer.Untrim(cancellationToken);
                            }
                        }
                        finally
                        {
                            Dispatcher.UIThread.Post(() =>
                            {
                                if (ProcessingApplication.HasValue)
                                {
                                    AddOrUpdateApplication(ProcessingApplication.Value, true, false);
                                }
                                ProcessingApplication = null;
                            });
                        }
                    }
                }
                finally
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        _displayedXCIFiles.AddOrReplaceMatching(_allXCIFiles, viewsSaved);
                        _selectedXCIFiles.AddOrReplaceMatching(_allXCIFiles, toProcess);
                        Processing = false;
                        SortAndFilter();
                    });
                }
            })
            {
                Name = "GUI.XCIFilesTrimmerThread",
                IsBackground = true,
            };

            XCIFileTrimThread.Start();
        }

        private bool Filter<T>(T arg)
        {
            if (arg is XCITrimmerFileModel content)
            {
                return string.IsNullOrWhiteSpace(_search) || content.Name.ToLower().Contains(_search.ToLower()) || content.Path.ToLower().Contains(_search.ToLower());
            }

            return false;
        }

        private IOrderedEnumerable<XCITrimmerFileModel> Sort(AvaloniaList<XCITrimmerFileModel> list)
        {
            return list
                .OrderBy(it => it.Name)
                .ThenBy(it => it.Path);
        }

        public void TrimSelected()
        {
            PerformOperation(true);
        }

        public void UntrimSelected()
        {
            PerformOperation(false);
        }

        public void SetProgress(int current, int maximum)
        {
            if (_processingApplication != null)
            {
                int percentageProgress = 100 * current / maximum;
                if (!ProcessingApplication.HasValue || (ProcessingApplication.Value.PercentageProgress != percentageProgress))
                {
                    ProcessingApplication = ProcessingApplication.Value with { PercentageProgress = percentageProgress };
                }
            }
        }

        public void SelectAll()
        {
            SelectedXCIFiles.Clear();
            SelectedXCIFiles.AddRange(DisplayedXCIFiles);
        }

        public void SelectNone()
        {
            SelectedXCIFiles.Remove(DisplayedXCIFiles);
        }

        public void Select(XCITrimmerFileModel model)
        {
            SelectedXCIFiles.ReplaceOrAdd(model, model);
        }

        public void Deselect(XCITrimmerFileModel model)
        {
            SelectedXCIFiles.Remove(model);
        }

        public void SortAndFilter()
        {
            if (Processing)
            {
                return;
            }

            Sort(AllXCIFiles)
                .AsObservableChangeSet()
                .Filter(Filter)
                .Bind(out var view).AsObservableList();

            var items = SelectedXCIFiles.ToArray();

            _displayedXCIFiles.Clear();
            _displayedXCIFiles.AddRange(view);

            foreach (XCITrimmerFileModel item in items)
            {
                SelectedXCIFiles.ReplaceOrAdd(item, item);
            }

            OnPropertyChanged(nameof(DisplayedXCIFiles));
        }

        public Optional<XCITrimmerFileModel> ProcessingApplication
        {
            get => _processingApplication;
            set
            {
                if (!value.HasValue && _processingApplication.HasValue)
                {
                    value = _processingApplication.Value with { PercentageProgress = null };
                }

                if (value.HasValue)
                {
                    _displayedXCIFiles.AddOrReplaceWith(value.Value);
                }

                _processingApplication = value;
                OnPropertyChanged();
            }
        }

        public bool Processing
        {
            get => _cancellationTokenSource != null;
            private set
            {
                if (value && !Processing)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                else if (!value && Processing)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(Cancel));
            }
        }

        public bool Cancel
        {
            get => _cancellationTokenSource != null && _cancellationTokenSource.IsCancellationRequested;
            set
            {
                if (value)
                {
                    if (!Processing)
                    {
                        return;
                    }

                    _cancellationTokenSource.Cancel();
                }

                OnPropertyChanged();
            }
        }

        public AvaloniaList<XCITrimmerFileModel> AllXCIFiles
        {
            get => _allXCIFiles;
            set
            {
                _allXCIFiles = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UpdateCount));
                SortAndFilter();
            }
        }

        public string UpdateCount
        {
            get => string.Format(LocaleManager.Instance[LocaleKeys.XCITrimmerCount], AllXCIFiles.Count);
        }

        public AvaloniaList<XCITrimmerFileModel> DisplayedXCIFiles
        {
            get => _displayedXCIFiles;
            set
            {
                _displayedXCIFiles = value;
                OnPropertyChanged();
            }
        }

        public AvaloniaList<XCITrimmerFileModel> SelectedXCIFiles
        {
            get => _selectedXCIFiles;
            set
            {
                _selectedXCIFiles = value;
                OnPropertyChanged();
            }
        }

        public string Search
        {
            get => _search;
            set
            {
                _search = value;
                OnPropertyChanged();
                SortAndFilter();
            }
        }
    }
}