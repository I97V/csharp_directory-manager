using System;

namespace csharp_directory_manager
{
	public class Record
	{
		public DateTime CreationTime { get; set; }
		public DateTime LastAccessTime { get; set; }
		public DateTime LastWriteFile { get; set; }
		public string Extension { get; set; }
		public string FullName { get; set; }
		public bool IsReadOnly { get; set; }
		public long Length { get; set; }
		public string Name { get; set; }
	}
}