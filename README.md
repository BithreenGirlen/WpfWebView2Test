# WpfWebView2Test

Test WPF/WebView2 programme to exeute Javascript defined on HTML page.

## Background

CryptJS is widely used library, but has a [bug for AES](https://github.com/brix/crypto-js/issues/293), namely accepts a key of which length is not 16/24/32 bytes, and goes beyond the "standard".
So there would be situations where using any other libraries compliant with the AES specifications could not decrypt, and could not be helped but to use CryptoJS.

This is a sample solution using WebView2.

- HTML
  - AES decryption 
- C#
  - File I/O

## HTML side
``` html
<!DOCTYPE html>
<html lang="ja">
<head>
    <script type="text/javascript" src="js/rollups/aes.js"></script>
</head>
<body>
    <script>
        function decrypt(t) {
            var o = CryptoJS.AES.decrypt(t.replace(/\n/g, ''), "hexatonics");
            return CryptoJS.enc.Utf8.stringify(o);
        }
    </script>

</body>
</html>
```

## C# side

``` c#
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
```
