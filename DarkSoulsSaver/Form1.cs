namespace DarkSoulsSaver
{
    public partial class Form1 : Form
    {

        private string saveLocation;
        private string backupLocation;
        private string currentGame;
        private string[] backupDirectories;
        private const string BACKUP_INFO_FILENAME = "backupInfo.csv";
        private const int LISTBOX_COLUMN_PADDING = 30;

        public Form1()
        {

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            currentGame = "DarkSoulsIII";
            saveLocation = Path.Combine(appData, currentGame);
            string dssBase = Path.Combine(myDocs, "DarkSoulsSaver");
            backupLocation = Path.Combine(dssBase, currentGame);

            Directory.CreateDirectory(backupLocation);
            InitializeComponent();

        }

        private void AddListBoxRow(string date, string time, string desc)
        {
            listBox1.Items.Add
                (
                date.PadRight(LISTBOX_COLUMN_PADDING - date.Length, ' ')
                + time.PadRight(LISTBOX_COLUMN_PADDING - time.Length, ' ')
                + desc.Replace("*comma*", ",")
                );
        }

        private void AddToListBox(string dir)
        {
            string dataFile = Path.Combine(dir, BACKUP_INFO_FILENAME);
            string text = File.ReadAllText(dataFile);
            string outputString = "";
            string[] fields = text.Split(",");
            AddListBoxRow(fields[0], fields[1], fields[2]);
        }

        private void RefreshControls()
        {
            listBox1.Items.Clear();
            AddListBoxRow("Date", "Time", "Description");
            backupDirectories = Directory.GetDirectories(backupLocation);
            foreach (string dir in backupDirectories)
            {
                AddToListBox(dir);
            }
            btnLoad.Enabled = false;
            btnDelete.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            RefreshControls();
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        private void WriteBackupInfo(string directory, string description)
        {
            string backupInfoFile = Path.Combine(directory, BACKUP_INFO_FILENAME);
            File.WriteAllText(backupInfoFile, String.Format("{0},{1},{2}", DateTime.Now.ToString("dd/MM/yyyy"), DateTime.Now.ToString("HH:mm"), description));
        }

        private void CreateBackup(string description)
        {
            description = description.Replace(",", "*comma*");
            string id = Guid.NewGuid().ToString();
            Directory.CreateDirectory(Path.Combine(backupLocation, id));
            string toDirectory = Path.Combine(backupLocation, id);
            CopyDirectory(saveLocation, toDirectory, true);
            WriteBackupInfo(toDirectory, description);
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            CreateBackup(txtDescription.Text);
            txtDescription.Text = "";
            RefreshControls();

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            Directory.Delete(GetPathToSelectedBackup(), true);
            RefreshControls();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool enabled = listBox1.SelectedIndex > 0;
            btnDelete.Enabled = enabled;
            btnLoad.Enabled = enabled;

        }

        private void AskIfBackupFirst()
        {
            DialogResult confirmResult = MessageBox.Show("Would you like to backup your saves first?",
                         "Backup?",
                         MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                CreateBackup("Pre-Load Backup");
            }

        }

        private bool ConfirmLoad()
        {
            DialogResult confirmResult = MessageBox.Show("This action will overwite your implemented saves with those from the backup!", "Confirm Overwrite!!", MessageBoxButtons.OKCancel);

            return confirmResult == DialogResult.OK;

        }

        private string GetPathToSelectedBackup()
        {
            return backupDirectories[listBox1.SelectedIndex - 1];
        }


        private void btnLoad_Click(object sender, EventArgs e)
        {
            AskIfBackupFirst();
            if (ConfirmLoad())
            {
                Directory.Delete(saveLocation, true);
                CopyDirectory(GetPathToSelectedBackup(), saveLocation, true);
            }
            RefreshControls();
        }
    }
}