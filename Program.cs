using System;
using System.IO;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Renaming_Pictures
{
    class Program
    {

        private const string CAM_MAKE = "NIKON D5100";
        private const string BASE_DIR = @"Z:\Pictures";


        private static DateTime getDateTaken(string path) // Returns a DateTime Object of the date which the photo is taken
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var metadata = ImageMetadataReader.ReadMetadata(fileStream);
            var subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().ToList();
            var LastsubIfd = subIfdDirectory[subIfdDirectory.Count - 1];
            var dateTime = LastsubIfd?.GetDateTime(36867);
            fileStream.Close();
            return (DateTime)dateTime;
        }

        private static bool checkIfDSLR(string path) // Checks if the image was take by a DSLR - specifically Nikon D5100
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            var metadata = ImageMetadataReader.ReadMetadata(fileStream);
            fileStream.Close();
            var ifd0Directory = metadata.OfType<ExifIfd0Directory>().FirstOrDefault();
            string make;
            try {
                make = ifd0Directory?.GetString(ExifDirectoryBase.TagModel);
            } catch (MetadataException e) {
                make = "";
                Console.WriteLine(e);
            }
            if (make == CAM_MAKE) {
                return true;
            } else {
                return false;
            }
        }

        private static Dictionary<string, List<int>> countDateTimes(List<string[]> files)
        {
            var fileDates = new Dictionary<string, List<int>>();
            for (int i = 0; i < files.Count; i++) {
                string dateTime = files[i][1];
                if (fileDates.ContainsKey(dateTime)) {
                    fileDates[dateTime].Add(i);
                } else {
                    fileDates[dateTime] = new List<int>() { i };
                }
            }
            return fileDates;
        }

        public static void Main(string[] args)
        {
            string path = BASE_DIR;

            DirectoryInfo dir = new DirectoryInfo(path);

            List<FileInfo> files = dir.GetFiles("*", SearchOption.AllDirectories)
                .Where(file => file.Name.ToLower().EndsWith("jpg") || file.Name.ToLower().EndsWith("nef") || file.Name.ToLower().EndsWith("jpeg")).ToList();

            List<string[]> filesToRename = new List<string[]>();

            string regExpression = @"[0-9]{8}-[0-9]{6}#[0-9]{3}";

            foreach (FileInfo file in files) {
                if (checkIfDSLR(file.FullName) && !(Regex.IsMatch(file.Name, regExpression))) { 
                    filesToRename.Add(new string[2]{ file.FullName, getDateTaken(file.FullName).ToString()});
                } 
            }

            if (filesToRename.Count == 0) {
                Console.WriteLine("No files need to renamed.");
                Environment.Exit(1);
            } else {
                Console.WriteLine("Files to change have been found.");
            }

            var filesDictionary = countDateTimes(filesToRename);

            string fileName, directory, oldPath, extension, newPath;

            foreach (var pair in filesDictionary) {
                if (pair.Value.Count == 1)
                {
                    fileName = Convert.ToDateTime(pair.Key).ToString("yyyyMMdd-HHmmss") + "#001";
                    oldPath = filesToRename[pair.Value[0]][0];
                    directory = Path.GetDirectoryName(oldPath);
                    extension = Path.GetExtension(oldPath).ToUpper();
                    newPath = directory + "\\" + fileName + extension;
                    File.Move(oldPath, newPath);
                    Console.WriteLine("Changed {0} to {1}", oldPath, newPath);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Duplicate Time {0} ", pair.Value.Count);
                    string[] fileNames = new string[pair.Value.Count];

                    for (int i = 0; i < pair.Value.Count; i++) {
                        fileNames[i] = filesToRename[pair.Value[i]][0];
                    }

                    fileNames = fileNames.OrderBy(x => Path.GetFileName(x)).ToArray();
                    string hexID;

                    foreach (string a in fileNames) {
                        Console.WriteLine(a);
                    }

                    for (int i = 0; i < fileNames.Length; i++) {
                        hexID = (i + 1).ToString("x3").ToUpper();
                        fileName = Convert.ToDateTime(pair.Key).ToString("yyyyMMdd-HHmmss") + "#" + hexID;
                        oldPath = fileNames[i];
                        directory = Path.GetDirectoryName(oldPath);
                        extension = Path.GetExtension(oldPath).ToUpper();
                        newPath = directory + "\\" + fileName + extension;
                        File.Move(oldPath, newPath);
                        Console.WriteLine("Changed {0} to {1}", oldPath, newPath);
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine("All files have been renamed.");

        }
    }
}
