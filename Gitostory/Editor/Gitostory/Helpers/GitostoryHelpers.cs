using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitostorySpace
{
    public static class GitostoryHelpers
    {
        public static GitostorySupportedType GetGitostoryTypeOfAsset(string assetPath)
        {
            // Check for null or empty string
            if (string.IsNullOrEmpty(assetPath))
            {
                return GitostorySupportedType.Unsuported;
            }

            string extension = Path.GetExtension(assetPath).ToLower();

            switch (extension)
            {
                case ".prefab":
                    return GitostorySupportedType.Prefab;

                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    return GitostorySupportedType.Texture;

                case ".mat":
                    return GitostorySupportedType.Material;

                case ".unity":
                    return GitostorySupportedType.Scene;

                case ".cs":
                case ".txt":
                case ".json":
                case ".xml":
                    return GitostorySupportedType.Script;

                case ".anim":
                    return GitostorySupportedType.Animation;

                default:
                    return GitostorySupportedType.Unsuported;
            }
        }

        internal static void CleanUpTemp()
        {
            // Delete all assets under temp directory.
            var path = GitostoryConfig.Paths.TEMP_ROOT;

            if (Directory.Exists(path))
            {
                // Get all files and directories within the temp directory
                var files = Directory.GetFiles(path);
                var directories = Directory.GetDirectories(path);

                // Delete all files
                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException e)
                    {
                        // Handle the error (for example, log it)
                        Debug.LogError("Error deleting TEMP file: " + e.Message);
                    }
                }

                // Delete all directories
                foreach (var directory in directories)
                {
                    try
                    {
                        Directory.Delete(directory, true); // true for recursive delete
                    }
                    catch (IOException e)
                    {
                        // Handle the error (for example, log it)
                        Console.WriteLine("Error deleting directory: " + e.Message);
                    }
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist: " + path);
            }

            AssetDatabase.Refresh();
        }

        public static int? ExtractNumberInBrackets(string input)
        {
            int startIndex = input.IndexOf('[') + 1; // Find the index of '[' and move one step forward to skip it
            int endIndex = input.IndexOf(']', startIndex); // Find the index of ']' starting from startIndex

            if (startIndex < endIndex)
            {
                string numberStr = input.Substring(startIndex, endIndex - startIndex); // Extract the number as a string
                if (int.TryParse(numberStr, out int number))
                {
                    return number;
                }
            }

            // Return null if no number was found or if parsing failed
            return null;
        }

    }
}
