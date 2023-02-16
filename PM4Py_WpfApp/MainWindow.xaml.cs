using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PM4Py_WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string pythonExecutable; // Zmienna na plik wykonywalny python.exe
        public string xesFilename;      // Zmienna na plik XES
        public string imagesDir;        // Zmienna na folder na obrazy
        public string imageName;        // Zmienna na nazwę obrazu (zwracana na wyjściu funkcji z pliku PythonProcessMiningAlgorithms.py)
        public string pythonFilesDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName + @"\PythonFile\";  // Zmienna na folder z plikiem PythonProcessMiningAlgorithms.py
        
        // Główne okno aplikacji
        public MainWindow()
        {
            InitializeComponent();
        }

        // Zwalnianie pamięci
        public void ReleaseMemory()
        {
            petriNetImage.Source = null;
            UpdateLayout();
            GC.Collect();
        }

        // Usuwa obrazy z folderu imagesDir
        public void DeleteFiles(string imagesDir)
        {
            DirectoryInfo di = new DirectoryInfo(imagesDir);
            foreach (FileInfo file in di.GetFiles("tmp??????????.png"))
            {
                file.Delete();
            }
        }

        // Wstawia obraz imageName do BitmapImage
        private void InsertImage(string imagesDir, string imageName)
        {
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(imagesDir + imageName);
            bitmap.EndInit();
            petriNetImage.Source = bitmap;
        }

        // Pozwala wybrać plik wykonywalny Python (python.exe)
        private void ChoosePythonExecutable_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".exe";
            openFileDialog.Filter = "EXE files (*.exe)|*.exe";

            if (openFileDialog.ShowDialog() == true)
            {
                pythonExecutable = openFileDialog.FileName;
            }

            if (pythonExecutable != null && xesFilename != null && imagesDir != null)
            {
                alphaMinerButton.IsEnabled = true;
                heuristicsMinerPetriNetButton.IsEnabled = true;
                heuristicsNetButton.IsEnabled = true;
                inductiveMinerPetriNetButton.IsEnabled = true;
                processTreeButton.IsEnabled = true;
            }
        }

        // Pozwala wybrać plik XES
        private void ChooseXesFileButton_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".xes";
            openFileDialog.Filter = "XES files (*.xes)|*.xes";

            if (openFileDialog.ShowDialog() == true)
            {
                xesFilename = openFileDialog.FileName;
            }

            if (pythonExecutable != null && xesFilename != null && imagesDir != null)
            {
                alphaMinerButton.IsEnabled = true;
                heuristicsMinerPetriNetButton.IsEnabled = true;
                heuristicsNetButton.IsEnabled = true;
                inductiveMinerPetriNetButton.IsEnabled = true;
                processTreeButton.IsEnabled = true;
            }
        }

        // Pozwala wybrać folder, w jakim mają być zapisywane obrazy
        private void ChooseImagesDir_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                imagesDir = dialog.SelectedPath + @"\\";
            }

            if (pythonExecutable != null && xesFilename != null && imagesDir != null)
            {
                alphaMinerButton.IsEnabled = true;
                heuristicsMinerPetriNetButton.IsEnabled = true;
                heuristicsNetButton.IsEnabled = true;
                inductiveMinerPetriNetButton.IsEnabled = true;
                processTreeButton.IsEnabled = true;
            }
        }

        // Zdalnie wywołuje funkcję alpha_miner z pliku PythonProcessMiningAlgorithms.py, która odkrywa sieć Petriego przy użyciu algorytmu Alpha Miner lub Alpha+ Miner
        private void AlphaMiner_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            if (cleaningDirectory.IsChecked == true)
            {
                DeleteFiles(imagesDir);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExecutable;
            if (alpha_classic.IsSelected)
            {
                start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "alpha_miner", xesFilename, imagesDir, "classic");
            }
            else if (alpha_plus.IsSelected)
            {
                if (removeUnconnected.IsChecked == true)
                {
                    start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "alpha_miner", xesFilename, imagesDir, "plus", "remove_unconnected");
                }
                else
                {
                    start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "alpha_miner", xesFilename, imagesDir, "plus", "leave_unconnected");
                }
            }
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    imageName = reader.ReadToEnd();
                }
            }

            InsertImage(imagesDir, imageName);
        }

        // Zdalnie wywołuje funkcję heuristics_miner_petri_net z pliku PythonProcessMiningAlgorithms.py, która odkrywa sieć Petriego przy użyciu algorytmu Heuristics Miner
        private void HeuristicsMinerPetriNet_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            if (cleaningDirectory.IsChecked == true)
            {
                DeleteFiles(imagesDir);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExecutable;
            start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5} {6}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "heuristics_miner_petri_net", xesFilename, imagesDir,
                heuristics_miner_dependency_threshold.Value.ToString().Replace(",", "."), heuristics_miner_and_threshold.Value.ToString().Replace(",", "."), heuristics_miner_loop_two_threshold.Value.ToString().Replace(",", "."));
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    imageName = reader.ReadToEnd();
                }
            }

            InsertImage(imagesDir, imageName);
        }

        // Zdalnie wywołuje funkcję heuristics_net z pliku PythonProcessMiningAlgorithms.py, która odkrywa sieć heurystyczną przy użyciu algorytmu Heuristics Miner
        private void HeuristicsNet_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            if (cleaningDirectory.IsChecked == true)
            {
                DeleteFiles(imagesDir);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExecutable;
            start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5} {6}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "heuristics_net", xesFilename, imagesDir,
                heuristics_miner_dependency_threshold.Value.ToString().Replace(",", "."), heuristics_miner_and_threshold.Value.ToString().Replace(",", "."), heuristics_miner_loop_two_threshold.Value.ToString().Replace(",", "."));
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    imageName = reader.ReadToEnd();
                }
            }

            InsertImage(imagesDir, imageName);
        }

        // Zdalnie wywołuje funkcję inductive_miner_petri_net z pliku PythonProcessMiningAlgorithms.py, która odkrywa sieć Petriego przy użyciu algorytmu Inductive Miner
        private void InductiveMinerPetriNet_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            if (cleaningDirectory.IsChecked == true)
            {
                DeleteFiles(imagesDir);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExecutable;
            if (inductive_classic.IsSelected)
            {
                start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "inductive_miner_petri_net", xesFilename, imagesDir,
                    "IM", inductive_miner_noise_threshold.Value.ToString().Replace(",", "."));
            }
            else if (inductive_infrequent.IsSelected)
            {
                start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "inductive_miner_petri_net", xesFilename, imagesDir,
                    "IMf", inductive_miner_noise_threshold.Value.ToString().Replace(",", "."));
            }
            else if (inductive_directly_follows.IsSelected)
            {
                start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4} {5}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "inductive_miner_petri_net", xesFilename, imagesDir,
                    "IMd", inductive_miner_noise_threshold.Value.ToString().Replace(",", "."));
            }
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    imageName = reader.ReadToEnd();
                }
            }

            InsertImage(imagesDir, imageName);
        }

        // Zdalnie wywołuje funkcję process_tree z pliku PythonProcessMiningAlgorithms.py, która odkrywa drzewo procesu przy użyciu algorytmu Inductive Miner
        private void ProcessTree_Click(object sender, RoutedEventArgs e)
        {
            ReleaseMemory();

            if (cleaningDirectory.IsChecked == true)
            {
                DeleteFiles(imagesDir);
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = pythonExecutable;
            start.Arguments = string.Format("\"{0}\" {1} \"{2}\" \"{3}\" {4}", pythonFilesDir + "PythonProcessMiningAlgorithms.py", "process_tree", xesFilename, imagesDir, inductive_miner_noise_threshold.Value.ToString().Replace(",", "."));
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    imageName = reader.ReadToEnd();
                }
            }

            InsertImage(imagesDir, imageName);
        }
    }
}
