using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SECoursework.Presentation_Layer.Commands;
using SECoursework.Business.Models;
using System.Collections.ObjectModel;
using SECoursework.Presentation_Layer.Views;
using SECoursework.Data_Layer;
using SECoursework.Business;
using System.IO;

namespace SECoursework.Presentation_Layer.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public string lblHeader { get; set; }
        public string lblBody { get; set; }
        public string lblMessagesList { get; set; }
        public string lblSIR { get; set; }
        public string lblTrending { get; set; }
        public string lblURL { get; set; }
        public string lblMentions { get; set; }

        public string txtBody { get; set; }
        public string txtHeader { get; set; }
        public string txtMessagesList { get; set; }
        public ObservableCollection<string> SIRList { get; private set; }
        public ObservableCollection<string> TrendingList { get; private set; }
        public ObservableCollection<string> QuarantineList { get; private set; }
        public ObservableCollection<string> MentionsList { get; private set; }

        public ICommand ProcessButtonCommand { get; private set; }
        public ICommand UploadButtonCommand { get; private set; }

        public string btnProcessText { get; set; }
        public string btnUploadText { get; set; }

        public MainWindowViewModel()
        {
            lblHeader = "Message Header";
            lblBody = "Message Body";
            lblMessagesList = "List of Messages";
            lblSIR = "Serious Incident Reports";
            lblTrending = "Trending List";
            lblURL = "URL Quarantine List";
            lblMentions = "Mentions List";

            btnProcessText = "Process Message";
            btnUploadText = "Upload Messages from File";

            txtBody = string.Empty;
            txtHeader = string.Empty;
            txtMessagesList = string.Empty;

            QuarantineList = new ObservableCollection<string>();
            MentionsList = new ObservableCollection<string>();
            TrendingList = new ObservableCollection<string>();
            SIRList = new ObservableCollection<string>();

            ProcessButtonCommand = new RelayCommand(ProcessButtonClick);
            UploadButtonCommand = new RelayCommand(UploadButtonClick);
        }

        private void ProcessButtonClick()
        {
            txtHeader.Replace("\n", "").Replace("\r", "");
            if (string.IsNullOrWhiteSpace(txtHeader))
            {
                MessageBox.Show("Please enter a message header");
                return;
            }
            if (string.IsNullOrWhiteSpace(txtBody))
            {
                MessageBox.Show("Please enter a message body");
                return;
            }

            //validate message
            bool message_validated = InputValidator.ValidateMessage(txtHeader, txtBody);
            if (!message_validated)
            {
                return;
            }

            //process message
            Message message = MessageProcessor.ProcessMessage(txtHeader, txtBody);
            UpdateViewModel(message);
        }
        private void UploadButtonClick()
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension
            dlg.DefaultExt = ".txt";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and process messages
            if (result == true)
            {
                {
                    List<string> lines = File.ReadAllLines(dlg.FileName).ToList();

                    string header = "";
                    int validated = 0;
                    int failed = 0;
                    List<string> body = new();
                    int flag = 0;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i] == "***END***")
                        {
                            string joined = string.Join("\r\n", body);
                            bool inputValidated = InputValidator.ValidateMessage(header, joined);
                            if (inputValidated)
                            {
                                Message message = MessageProcessor.ProcessMessage(header, joined);
                                UpdateViewModel(message);
                                validated++;
                            }
                            else
                            {
                                failed++;
                            }
                            header = "";
                            body.Clear();
                            flag = 0;
                        }
                        if (flag == 2)
                        {
                            body.Add(lines[i]);
                        }
                        if (flag == 1)
                        {
                            header = lines[i];
                            flag = 2;
                        }
                        if (lines[i] == "***START***")
                        {
                            flag = 1;
                        }
                    }
                    MessageBox.Show("Mesages successfully processed: " + validated + ", failed messages: " + failed);
                }
            }
        }
        
        private void UpdateViewModel(Message message)
        {
            //update lists
            QuarantineList = new ObservableCollection<string>(MessageProcessor.GetQuarantineList());
            TrendingList = new ObservableCollection<string>(MessageProcessor.GetTrending());
            SIRList = new ObservableCollection<string>(MessageProcessor.GetSIRList());
            MentionsList = new ObservableCollection<string>(MessageProcessor.GetMentions());

            OnChanged(nameof(QuarantineList));
            OnChanged(nameof(TrendingList));
            OnChanged(nameof(SIRList));
            OnChanged(nameof(MentionsList));

            string processedMessage = "";
            foreach (var prop in message.GetType().GetProperties())
            {
                processedMessage += prop.Name + ": " + prop.GetValue(message) + "\n";
            }

            MessageBox.Show(processedMessage);
        }
    }
}
