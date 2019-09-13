using System.Net;
using System.IO;
using System.Windows.Forms;

string result = null;
string url = "http://www.stackoverflow.com";
WebResponse response = null;
StreamReader reader = null;

try
{
    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
    request.Method = "GET";
    response = request.GetResponse();
    reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
    result = reader.ReadToEnd();
}
catch (Exception ex)
{
    // handle error
    MessageBox.Show(ex.Message);
}
finally
{
    if (reader != null)
        reader.Close();
    if (response != null)
        response.Close();
}