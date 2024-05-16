using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;

namespace popNet_test
{
    public partial class Form1 : Form
    {
        private const string ApplicationName = "Stock BOX";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // ListViewの設定
            listView1.View = View.Details;
            listView1.Columns.Add("Subject", 400);
        }

        private async void button1_Click(object sender, EventArgs e)
        {

            try
            {   // スコープ設定で指定したURLを追加
                string[] Scopes = { GmailService.Scope.GmailReadonly };
                UserCredential credential;

                // Token の取得
                using (var stream = new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read))
                {
                    string credPath = "token.json";
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath, true));
                }

                await FetchEmails(credential);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
        private async Task FetchEmails(UserCredential credential)
        {
            try
            {
                var service = new GmailService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // ユーザーのメールリストを取得
                var request = service.Users.Messages.List("me");
                request.LabelIds = "INBOX";
                request.MaxResults = 10;

                var response = await request.ExecuteAsync();
                listView1.Items.Clear();

                if (response.Messages != null && response.Messages.Count > 0)
                {
                    foreach (var msg in response.Messages)
                    {
                        var messageRequest = service.Users.Messages.Get("me", msg.Id);
                        var message = await messageRequest.ExecuteAsync();

                        string subject = message.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value;

                        // リストビューに表示
                        ListViewItem item = new ListViewItem(subject);
                        listView1.Items.Add(item);
                    }
                }
                else
                {
                    MessageBox.Show("No messages found.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Btn_Path_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // 選択したファイルパスをテキストボックスに表示
                    textBox1.Text = openFileDialog.FileName;
                }
            }
        }
    }
}
