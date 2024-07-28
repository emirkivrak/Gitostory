using System;
using System.Collections.Generic;

namespace GitostorySpace
{
    public static class GitostoryUtilDiff
    {
        public enum DiffType
        {
            Inserted,
            Deleted,
            Unchanged
        }

        public class DiffResult
        {
            public DiffType Type { get; set; }
            public string Text { get; set; }
        }

        public static List<DiffResult> DiffText(string oldText, string newText)
        {
            // Simplified implementation based on the example repository
            var diffs = new List<DiffResult>();
            var oldLines = oldText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var newLines = newText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int oldIndex = 0, newIndex = 0;
            while (oldIndex < oldLines.Length || newIndex < newLines.Length)
            {
                if (oldIndex < oldLines.Length && newIndex < newLines.Length && oldLines[oldIndex] == newLines[newIndex])
                {
                    diffs.Add(new DiffResult { Type = DiffType.Unchanged, Text = oldLines[oldIndex] });
                    oldIndex++;
                    newIndex++;
                }
                else
                {
                    if (oldIndex < oldLines.Length)
                    {
                        diffs.Add(new DiffResult { Type = DiffType.Deleted, Text = oldLines[oldIndex] });
                        oldIndex++;
                    }
                    if (newIndex < newLines.Length)
                    {
                        diffs.Add(new DiffResult { Type = DiffType.Inserted, Text = newLines[newIndex] });
                        newIndex++;
                    }
                }
            }

            return diffs;
        }
    }
}