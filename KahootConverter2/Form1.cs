using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using Microsoft.Win32;
using mshtml;

namespace KahootConverter2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        String data, title, kData;
        String[] placeText = new String[10];
        Button[] button = new Button[50];
        String[] qData;
        String[] aData;
        TextBox[] textboxA = new TextBox[50];
        int count = 0;
        int cur;

        public void placeHolder(TextBox textbox, String text, Button button2)
        {
            placeText[count] = text;
            textbox.Text = text;
            if (button2 != null)
            {
                button[count] = button2;
            }
            textboxA[count] = textbox;
            textbox.ForeColor = SystemColors.WindowFrame;
            textboxA[count].KeyDown += new KeyEventHandler(Enter2);
            textboxA[count].Enter += new System.EventHandler(onEnter);
            textboxA[count].Leave += new System.EventHandler(onLeave);
            count++;
        }

        public void Enter2(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && button[cur] != null)
            {
                button[cur].PerformClick();
            }
        }

        public void onEnter(object sender, EventArgs e)
        {
            TextBox textbox = sender as TextBox;
            for (int i = 0; i < placeText.Length; i++)
            {
                if (textbox.Text == placeText[i])
                {
                    textbox.ForeColor = SystemColors.WindowText;
                    textbox.Text = "";
                    cur = i;
                }
            }
        }

        public void onLeave(object sender, EventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox.Text == "")
            {
                textbox.ForeColor = SystemColors.WindowFrame;
                textbox.Text = placeText[cur];
            }
        }

        void getData(string ID)
        {
            string url = "https://quizlet.com/" + ID + "/test?mult_choice=on&prompt-with=1&limit=" + nmQuestions.Text;
            try
            {
                using (WebClient client = new WebClient())
                {
                    data = client.DownloadString(url);
                    kData = client.DownloadString("https://create.kahoot.it/#login");
                }
            } catch
            {
                MessageBox.Show("Invalid ID", "Error");
            }

            qData = data.Split(new string[] { "<span class='TermText qDef lang-en'>"}, StringSplitOptions.None);
            aData = data.Split(new string[] { "<span class='TermText qWord lang-en'>" }, StringSplitOptions.None);
            title = data.Split(new string[] { "<title>Test: " }, StringSplitOptions.None)[1].Split(new string[] { " | Quizlet</title>" }, StringSplitOptions.None)[0];
            this.Text = "Kahoot Converter - " + title + " - " + nmQuestions.Text + " Questions";
        }

        private static void WebBrowserVersionEmulation()
        {
            const string BROWSER_EMULATION_KEY =
            @"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION";
            //
            // app.exe and app.vshost.exe
            String appname = Process.GetCurrentProcess().ProcessName + ".exe";
            //
            // Webpages are displayed in IE9 Standards mode, regardless of the !DOCTYPE directive.
            const int browserEmulationMode = 11001;

            RegistryKey browserEmulationKey =
            Registry.CurrentUser.OpenSubKey(BROWSER_EMULATION_KEY, RegistryKeyPermissionCheck.ReadWriteSubTree) ??
            Registry.CurrentUser.CreateSubKey(BROWSER_EMULATION_KEY);

            if (browserEmulationKey != null)
            {
                browserEmulationKey.SetValue(appname, browserEmulationMode, RegistryValueKind.DWord);
                browserEmulationKey.Close();
            }
        }

        // Form1 Load.
        private void Form1_Load(object sender, EventArgs e)
        {
            placeHolder(txtSite, "Quizlet ID", btnScrape);
            placeHolder(txtUser, "Kahoot User", null);
            placeHolder(txtPass, "Kahoot Pass", btnLogin);
            WebBrowserVersionEmulation();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if ((txtUser.Text != "" || txtUser.Text != "Kahoot User") && (txtPass.Text != "" || txtPass.Text != "Kahoot Pass"))
            {
                webBrowser1.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webBrowser1_DocumentCompleted);
                webBrowser1.DocumentText = kData;
                webBrowser1.Navigate("https://create.kahoot.it/#login");
            } else
            {
                MessageBox.Show("Please ensure both username and password are filled out.", "Error");
            }
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            HtmlElement head = webBrowser1.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = webBrowser1.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            element.text = "function logIn() { var inpObj = document.getElementsByTagName('input'); inpObj[0].value = \"" + txtUser.Text + "\"; inpObj[1].value = \"" + txtPass.Text + "\";var e = jQuery.Event(\"keypress\");e.which = 13; e.keyCode = 13; var btns = document.getElementsByClassName(\"btn register\"); $(btns[1]).trigger(e);}";
            head.AppendChild(scriptEl);
            webBrowser1.Document.InvokeScript("logIn");
        }

        private void btnScrape_Click(object sender, EventArgs e)
        {
            getData(txtSite.Text);
        }
    }
}
