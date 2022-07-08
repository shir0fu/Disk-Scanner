using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Threading;

namespace Task10;

public partial class MainWindow : Window
{
    BackgroundWorker worker = new BackgroundWorker();
    ObservableCollection<Node> _nodes;
    string _dirName;
    
    public MainWindow()
    {
        InitializeComponent();

        GetDrivesName();
        
    }

    void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        progressBar1.Value = e.ProgressPercentage;
    }

    public void GetDrivesName()
    {
        DriveInfo[] drive = DriveInfo.GetDrives();
        ObservableCollection<string> listDrives = new ObservableCollection<string>();

        foreach (DriveInfo driveInfo in drive)
        {
            listDrives.Add(driveInfo.Name);
        }

        this.Dispatcher.BeginInvoke(() => { comboBox1.ItemsSource = listDrives; });
    }


    public ObservableCollection<Node> SearchDir(string dirName)
    {
        ObservableCollection<Node> listNodes = new ObservableCollection<Node>();
        Node node;

        int count = 0;
        
        try
        {

            string[] dirs = Directory.GetDirectories(dirName);
            foreach (string dir in dirs)
            {
                worker.ReportProgress(count);
                count++;
                double catalogSize = 0;
                node = new Node();
                node.Name = dir;

                // write size of folder
                double size = SizeOfFolder(dir, ref catalogSize);
                if (size > 1024)
                {
                    node.Size = Convert.ToString(Math.Round((double)size / 1024, 2)) + "GB";
                }
                else
                {
                    node.Size = Convert.ToString(Math.Round((double)size, 2)) + "MB";
                }

                node.Nodes = SearchDir(dir);
                listNodes.Add(node);
            }
        }
        catch (UnauthorizedAccessException)
        {

        }

        return listNodes;
    }


    public double SizeOfFolder(string folder, ref double catalogSize)
    {
        try
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            DirectoryInfo[] diA = di.GetDirectories();
            FileInfo[] fi = di.GetFiles();

            foreach (FileInfo f in fi)
            {
                catalogSize = catalogSize + f.Length;
            }

            foreach (DirectoryInfo df in diA)
            {

                SizeOfFolder(df.FullName, ref catalogSize);
            }

            return Math.Round((double)(catalogSize / 1024 / 1024), 2);
        }

        catch { }

        return catalogSize;
    }


    private void ComboBox_Selected(object sender, RoutedEventArgs e)
    {
        ComboBox comboBox = (ComboBox)sender;
        string selectedItem = (string)comboBox.SelectedItem;
        MessageBox.Show("Selected Disk: " + selectedItem);

        _dirName = selectedItem;

    }


    private void Button_Click(object sender, RoutedEventArgs e)
    {
        worker.WorkerReportsProgress = true;
        worker.DoWork += (s, eArg) => _nodes = SearchDir(_dirName);
        worker.RunWorkerCompleted += (s, eArg) => { baseTreeView.ItemsSource = _nodes; progressBar1.Value = 100; };
        worker.ProgressChanged += WorkerProgressChanged;
        worker.RunWorkerAsync();
    }
}



public class Node
{
    public string Size { get; set; }

    public string Name { get; set; }

    public ObservableCollection<Node> Nodes { get; set; }

    public ObservableCollection<string> Drives { get; set; }

}
