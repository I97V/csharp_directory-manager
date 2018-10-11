using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;

namespace csharp_directory_manager
{
	public partial class MainWindow : Window
	{
		List<Record> old_records = new List<Record>();
		List<Record> new_records = new List<Record>();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Browse_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.ShowDialog();

			tb_path.Text = fbd.SelectedPath;
		}

		private void Check_Click(object sender, RoutedEventArgs e)
		{
			string path_toJson = "foldersdata/" + tb_name.Text + ".json";

			dg_deleted.Items.Clear();
			dg_changed.Items.Clear();
			dg_accessed.Items.Clear();
			dg_new.Items.Clear();
			old_records.Clear();
			new_records.Clear();

			//	Check file-data
			if(!File.Exists(path_toJson))
			{
				MessageBoxResult result = System.Windows.MessageBox.Show(
					"Old records not found\nCreate new file?", 
					"", 
					MessageBoxButton.YesNo,
					MessageBoxImage.Asterisk, 
					MessageBoxResult.No
				);

				if(result == MessageBoxResult.Yes)
				{
					FillNewRecords(tb_path.Text);

					CreateFile(path_toJson, JsonConvert.SerializeObject(new_records));

					System.Windows.MessageBox.Show("File created!", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
				}

				return;
			}

			//	Fill old_records
			using (StreamReader sr = new StreamReader(path_toJson))
				old_records.AddRange(JsonConvert.DeserializeObject<List<Record>>(sr.ReadToEnd()));

			//	Rewrite file-data
			FillNewRecords(tb_path.Text);
			CreateFile(path_toJson, JsonConvert.SerializeObject(new_records));

			//	Check
			CheckOnDeleted();
			CheckOthers();
		}

		private void Open_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(tb_open_path.Text);

		}

		/*	Other functions	*/

		private void FillNewRecords(string _path)
		{
			if (!Directory.Exists(_path))
			{
				System.Windows.MessageBox.Show("Directory not found", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			try
			{
				DirectoryInfo root_dir = new DirectoryInfo(_path);

				if (root_dir.GetDirectories().Length == -1)
					return;

				foreach (FileInfo file in root_dir.GetFiles())
				{
					try
					{
						Record r = new Record
						{
							CreationTime = file.CreationTime,
							LastAccessTime = file.LastAccessTime,
							LastWriteFile = file.LastWriteTime,
							Extension = file.Extension,
							FullName = file.FullName,
							IsReadOnly = file.IsReadOnly,
							Length = file.Length,
							Name = file.Name
						};

						new_records.Add(r);
					}
					catch(Exception ex)
					{
						System.Windows.MessageBox.Show(ex.ToString(), "Error to file!", MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}

				foreach (var folder in root_dir.GetDirectories())
				{
					FillNewRecords(folder.FullName);
				}
			}
			catch(Exception ex)
			{
				System.Windows.MessageBox.Show(ex.ToString(), "Error to path!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			
		}

		private void CreateFile(string _path, string _text)
		{
			using (StreamWriter sw = new StreamWriter(_path))
			{
				sw.Write(_text);
			}
		}

		/*	Check functions	*/

		private void CheckOnDeleted()
		{
			for (int i = 0; i < old_records.Count; i++)
				if (!File.Exists(old_records[i].FullName))
					dg_deleted.Items.Add(old_records[i]);

		}

		private void CheckOthers()
		{
			bool isHas;

			for (int i = 0; i < new_records.Count; i++)
			{
				isHas = false;

				for (int j = 0; j < old_records.Count; j++)
				{
					// Check on New
					if(new_records[i].Name == old_records[j].Name)
					{
						isHas = true;
						// Check on Write
						if (!DateTime.Equals(new_records[i].LastWriteFile, old_records[j].LastWriteFile))
							dg_changed.Items.Add(new_records[i]);

						//	Check on Access
						if (!DateTime.Equals(new_records[i].LastAccessTime, old_records[j].LastAccessTime))
							dg_accessed.Items.Add(new_records[i]);

						break;
					}
				}

				if(!isHas)
					dg_new.Items.Add(new_records[i]);
			}
		}

		/*	Selection	*/

		private void Deleted_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Record r = dg_deleted.SelectedItem as Record;
			tb_open_path.Text = r.FullName;
		}

		private void New_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Record r = dg_new.SelectedItem as Record;
			tb_open_path.Text = r.FullName;
		}

		private void Changed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Record r = dg_changed.SelectedItem as Record;
			tb_open_path.Text = r.FullName;
		}

		private void Accessed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			Record r = dg_accessed.SelectedItem as Record;
			tb_open_path.Text = r.FullName;
		}
	}
}
