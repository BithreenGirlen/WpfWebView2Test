using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace WpfAppTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitialiseAsync();
        }

        async void InitialiseAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.WebMessageReceived += WebView_WebMessageReceived;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            webView.CoreWebView2.Navigate(Environment.CurrentDirectory);
        }

        private void WebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            UpdateAddressBarText(webView.Source.ToString());
        }

        /// <summary>
        /// Update text in DockPanel "addressBar".
        /// </summary>
        private void UpdateAddressBarText(string addressBarText)
        {
            addressBar.Text = addressBarText.Trim();
        }

        private void WebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {

        }
        /// <summary>
        /// Dock panel Button "Go".
        /// </summary>
        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            if (webView != null && webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(addressBar.Text);
            }
        }

        /// <summary>
        /// Return html file name selected in file dialog.
        /// </summary>
        private string SelectHtmlFile()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "HTML files| *.htm; *.html"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileName;
            }

            return string.Empty;
        }

        /// <summary>
        /// On "File" menu item "Open".
        /// </summary>
        private void MenuItem_Click_FileOpen(object sender, RoutedEventArgs e)
        {
            string file = SelectHtmlFile();
            if(!string.IsNullOrEmpty(file))
            {
                webView.CoreWebView2.Navigate(file);
            }
        }

        /// <summary>
        /// Return file names selected in file dialogue.
        /// </summary>
        private string[]? SelectFilesToProcess()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Text files| *.txt; *.bin; *.dat"
            };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                return openFileDialog.FileNames;
            }

            return null;
        }

        /// <summary>
        /// On "File" menu item "Select".
        /// </summary>
        private async void MenuItem_Click_FileSelect(object sender, RoutedEventArgs e)
        {
            string[]? files = SelectFilesToProcess();
            if (files != null && files.Length > 0)
            {
                string workFolder = Environment.CurrentDirectory + "/Decrypted";
                System.IO.Directory.CreateDirectory(workFolder);

                foreach (var file in files) 
                {
                    string fileText = System.IO.File.ReadAllText(file);
                    string decrypted = await ExecuteDecryptFunctionOnWebPage(fileText);
                    string filePath = workFolder + "/" + System.IO.Path.GetFileName(file);
                    System.IO.File.WriteAllText(filePath, decrypted);
                }
            }
        }

        private async Task<string> ExecuteDecryptFunctionOnWebPage(string text)
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
            };
            string msg = JsonSerializer.Serialize(text, jsonSerializerOptions);

            string result = await webView.ExecuteScriptAsync($"decrypt({msg})");

            string decrypted = JsonSerializer.Deserialize<string>(result);
            return decrypted;
        }
    }
}
