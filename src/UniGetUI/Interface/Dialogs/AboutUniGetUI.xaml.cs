using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using UniGetUI.Core.Tools;
using UniGetUI.Interface.Pages.AboutPages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace UniGetUI.Interface
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class AboutUniGetUI : Page
    {

        public event EventHandler? Close;
        int previousSelectedIndex = 0;
        public AboutUniGetUI()
        {
            InitializeComponent();
            SelectorBarItemPage1.Text = CoreTools.Translate("About");
            SelectorBarItemPage2.Text = CoreTools.Translate("Third-party licenses");
            SelectorBarItemPage3.Text = CoreTools.Translate("Contributors");
            SelectorBarItemPage4.Text = CoreTools.Translate("Translators");
            SelectorBarItemPage5.Text = CoreTools.Translate("Support me");
        }

        private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            System.Type pageType;

            switch (currentSelectedIndex)
            {
                case 0:
                    pageType = typeof(Pages.AboutPages.AboutUniGetUI);
                    break;
                case 1:
                    pageType = typeof(ThirdPartyLicenses);
                    break;
                case 2:
                    pageType = typeof(Contributors);
                    break;
                case 3:
                    pageType = typeof(Translators);
                    break;
                default:
                    pageType = typeof(SupportMe);
                    break;
            }

            SlideNavigationTransitionEffect slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

            ContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo { Effect = slideNavigationTransitionEffect });

            previousSelectedIndex = currentSelectedIndex;

        }

        private void CloseButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Close?.Invoke(this, new EventArgs());
        }
    }
}
