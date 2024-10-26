using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.UI.ViewModels;
using Ryujinx.UI.App.Common;
using System.Threading.Tasks;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class XCITrimmerWindow : UserControl
    {
        public XCITrimmerViewModel ViewModel;

        public XCITrimmerWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        public XCITrimmerWindow(ApplicationLibrary applicationLibrary)
        {
            DataContext = ViewModel = new XCITrimmerViewModel(applicationLibrary);

            InitializeComponent();
        }

        public static async Task Show(ApplicationLibrary applicationLibrary)
        {
            ContentDialog contentDialog = new()
            {
                PrimaryButtonText = "",
                SecondaryButtonText = "",
                CloseButtonText = "",
                Content = new XCITrimmerWindow(applicationLibrary),
                Title = string.Format(LocaleManager.Instance[LocaleKeys.XCITrimmerWindowTitle])
            };

            Style bottomBorder = new(x => x.OfType<Grid>().Name("DialogSpace").Child().OfType<Border>());
            bottomBorder.Setters.Add(new Setter(IsVisibleProperty, false));

            contentDialog.Styles.Add(bottomBorder);

            await contentDialog.ShowAsync();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            ((ContentDialog)Parent).Hide();
        }

/*
        private void RemoveDLC(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is DownloadableContentModel model)
                {
                    ViewModel.Remove(model);
                }
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var content in e.AddedItems)
            {
                if (content is XCITrimmerApplicationData applicationData)
                {
                    //ViewModel.Select(applicationData);
                }
            }

            foreach (var content in e.RemovedItems)
            {
                if (content is XCITrimmerApplicationData applicationData)
                {
                    //ViewModel.Deselect(applicationData);
                }
            }
        }
*/
    }
}
