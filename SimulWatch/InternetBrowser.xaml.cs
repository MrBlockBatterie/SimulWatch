using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using HtmlAgilityPack;
using Microsoft.Toolkit.Win32.UI.Controls.Interop.WinRT;
using Microsoft.Web.WebView2.Core;

namespace SimulWatch
{
    public partial class InternetBrowser : Window
    {
        public InternetBrowser()
        {
            InitializeComponent();
            
        }


        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var doc = new HtmlDocument();

            //Debug.WriteLine(Browser.Browser.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" }).Result);
            var html = await Browser.InvokeScriptAsync("eval", new string[] {"document.documentElement.outerHTML;"});
            doc.LoadHtml(html);

            var link = doc.DocumentNode.Descendants("iframe").First().GetAttributeValue("src", "no link");
            Debug.WriteLine(link);
            
            Browser.Navigate(link);


            
            
            Browser.NavigationCompleted += BrowserOnNavigationCompleted;
            

            
        }

        private async void BrowserOnNavigationCompleted(object sender, WebViewControlNavigationCompletedEventArgs e)
        {
            string html = await Browser.InvokeScriptAsync("eval", new string[] {"document.documentElement.outerHTML;"});
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            var mp4 = doc.DocumentNode.Descendants("source").First().GetAttributeValue("src", "no link");
            Debug.WriteLine(mp4);
            MainWindow mainWindow = (MainWindow) App.Current.MainWindow;
            mainWindow.StreamURL.Text = mp4;
                
            Browser.NavigationCompleted -= BrowserOnNavigationCompleted;
            Browser.GoBack();
            
        }
    }
}