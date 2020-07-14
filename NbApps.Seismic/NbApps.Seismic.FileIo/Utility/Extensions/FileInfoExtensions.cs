using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Extensions
{
    public static class FileInfoExtensions
    {
        /// <summary> Fast file move with big buffers
        /// </summary>
        /// <param name="source">Source file path</param> 
        /// <param name="destination">Destination file path</param> 
        public static FileInfo FastCopy(this FileInfo sourceFileInfo, string destination)
        {
            var source = sourceFileInfo.FullName;
            int array_length = (int)Math.Pow(2, 19);
            byte[] dataArray = new byte[array_length];
            using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.None, array_length))
            {
                using (BinaryReader bwread = new BinaryReader(fsread))
                {
                    using (FileStream fswrite = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, array_length))
                    {
                        using (BinaryWriter bwwrite = new BinaryWriter(fswrite))
                        {
                            for (; ; )
                            {
                                int read = bwread.Read(dataArray, 0, array_length);
                                if (0 == read)
                                    break;
                                bwwrite.Write(dataArray, 0, read);
                            }
                        }
                    }
                }
            }
            return new FileInfo(destination);
        }

        /// <summary> Fast file move with big buffers
        /// </summary>
        /// <param name="source">Source file path</param> 
        /// <param name="destination">Destination file path</param> 
        public static FileInfo FastCopy(this FileInfo sourceFileInfo, FileInfo destination)
        {
            var source = sourceFileInfo.FullName;
            int array_length = (int)Math.Pow(2, 19);
            byte[] dataArray = new byte[array_length];
            using (FileStream fsread = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, array_length))
            {
                using (BinaryReader bwread = new BinaryReader(fsread))
                {
                    using (FileStream fswrite = new FileStream(destination.FullName, FileMode.Create, FileAccess.Write, FileShare.None, array_length))
                    {
                        using (BinaryWriter bwwrite = new BinaryWriter(fswrite))
                        {
                            for (; ; )
                            {
                                int read = bwread.Read(dataArray, 0, array_length);
                                if (0 == read)
                                    break;
                                bwwrite.Write(dataArray, 0, read);
                            }
                        }
                    }
                }
            }
            destination.Refresh();
            return destination;
        }
    }
}
