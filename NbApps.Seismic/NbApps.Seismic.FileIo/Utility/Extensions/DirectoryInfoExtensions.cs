using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Utility.Extensions
{
    public static class DirectoryInfoExtensions
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteFile(string lpFileName);

        public static void DeleteContaining(this DirectoryInfo directoryInfo, SearchOption searchOption)
        {
            var files = Directory.EnumerateFiles(directoryInfo.FullName, "*", searchOption);
            var directories = Directory.EnumerateDirectories(directoryInfo.FullName, "*", searchOption);

            Parallel.ForEach(files, file =>
            {
                DeleteFile(file);
            });

            foreach (string directory in directories)
            {
                Directory.Delete(directory, true);
            }
        }

        public static void DeleteRecursive(this DirectoryInfo directoryInfo)
        {
            directoryInfo.DeleteContaining(SearchOption.AllDirectories);
            directoryInfo.Delete(true);
        }
    }
}
