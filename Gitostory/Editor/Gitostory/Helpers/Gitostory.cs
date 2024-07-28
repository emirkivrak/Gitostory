using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using LibGit2Sharp;
using System.Linq;

namespace GitostorySpace
{
    public class Gitostory : IDisposable
    {
        private string _repositoryPath;
        private Repository _repo;

        public Gitostory(string repositoryPath = "")
        {
            // Get the repository path from the config if it's not provided
            _repositoryPath = string.IsNullOrEmpty(repositoryPath) ? GitostoryConfig.Paths.REPOSITORY_ROOT : repositoryPath;
            _repo = new Repository(_repositoryPath);
        }

        public void Dispose()
        {
            _repo?.Dispose();
        }

        #region Git History Methods

        /// <summary>
        /// Retrieves all commits that modified the specified file.
        /// </summary>
        /// <param name="filePath">The relative path to the file within the repository.</param>
        /// <returns>A list of commit data for each commit that modified the file.</returns>
        public List<GitostoryPastCommitData> GetAllCommits(string filePath)
        {
            var storyElements = new List<GitostoryPastCommitData>();
            var filter = new CommitFilter
            {
                SortBy = CommitSortStrategies.Time,
                IncludeReachableFrom = _repo.Head
            };

            foreach (var commit in _repo.Commits.QueryBy(filter))
            {
                var targetTreeEntry = commit[filePath];

                // Skip if the file doesn't exist in this commit
                if (targetTreeEntry == null) continue;

                // Check if the file was modified in this commit
                bool wasModified = commit.Parents.All(parent =>
                {
                    var parentTreeEntry = parent[filePath];
                    return parentTreeEntry == null || parentTreeEntry.Target.Id != targetTreeEntry.Target.Id;
                });

                if (wasModified)
                {
                    storyElements.Add(new GitostoryPastCommitData
                    {
                        CommitHash = commit.Sha,
                        Author = commit.Author.Name,
                        CommitDate = commit.Author.When.DateTime.ToString(),
                        CommitMessage = commit.MessageShort
                    });
                }
            }
            return storyElements;
        }

        #endregion

        #region File Previous Version Methods

        /// <summary>
        /// Gets a file's previous version and stores it in a temporary directory.
        /// </summary>
        /// <param name="relativePath">The relative path to the file within the repository.</param>
        /// <param name="commitHash">The hash of the commit to retrieve the file from.</param>
        /// <param name="changeExtensionToTxt">Whether to change the file extension to .txt.</param>
        /// <param name="renameFile">Whether to rename the file.</param>
        /// <param name="newNameForFile">The new name for the file if renaming.</param>
        /// <returns>The path to the new file in the temporary directory.</returns>
        public string GetFilePreviousVersion(string relativePath, string commitHash, bool changeExtensionToTxt = false, bool renameFile = false, string newNameForFile = "")
        {
            string tempDirectory = GitostoryConfig.Paths.TEMP_ROOT;
            Directory.CreateDirectory(tempDirectory); // Ensure the directory exists
            string tempFileName = Path.GetFileName(relativePath);
            string tempFilePath = Path.Combine(tempDirectory, tempFileName);
            string relativeTempPath = string.Empty;

            if (changeExtensionToTxt)
            {
                Debug.Log("It's a script file");
                tempFilePath = Path.ChangeExtension(tempFilePath, ".txt");
            }

            // Retrieve the file content at the specified commit
            bool result = LoadFileAtCommit(relativePath, commitHash, tempFilePath);

            if (result)
            {
                tempFileName = renameFile ? newNameForFile : Path.GetFileName(tempFilePath);
                relativeTempPath = Path.Combine(GitostoryConfig.Paths.TEMP_ROOT, tempFileName);
                AssetDatabase.ImportAsset(relativeTempPath, ImportAssetOptions.ForceUpdate);
                AssetDatabase.Refresh();
            }

            return relativeTempPath;
        }

        /// <summary>
        /// Loads a file at a specified commit into a temporary file path.
        /// </summary>
        /// <param name="relativePath">The relative path to the file within the repository.</param>
        /// <param name="commitHash">The hash of the commit to retrieve the file from.</param>
        /// <param name="tempFilePath">The path to store the file content temporarily.</param>
        /// <returns>True if the file was successfully loaded; otherwise, false.</returns>
        public bool LoadFileAtCommit(string relativePath, string commitHash, string tempFilePath)
        {
            try
            {
                Commit commit = _repo.Commits.FirstOrDefault(c => c.Sha.StartsWith(commitHash));
                if (commit == null)
                {
                    Debug.LogError("Commit not found.");
                    return false;
                }

                TreeEntry entry = commit[relativePath];
                if (entry?.TargetType == TreeEntryTargetType.Blob)
                {
                    Blob blob = (Blob)entry.Target;
                    using (var contentStream = blob.GetContentStream())
                    {
                        using (var fileStream = File.Create(tempFilePath))
                        {
                            contentStream.CopyTo(fileStream);
                        }
                    }
                    return true;
                }
                else
                {
                    Debug.LogError("File not found in commit.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Git Add, Commit, Reset Methods

        /// <summary>
        /// Resets a single file to its state in the head commit.
        /// </summary>
        /// <param name="filePath">The path to the file to reset.</param>
        /// <returns>True if the reset was successful; otherwise, false.</returns>
        public bool GitResetSingleFile(string filePath)
        {
            var headCommit = _repo.Head.Tip;
            return Rollback(headCommit.Sha, filePath);
        }

        /// <summary>
        /// Stages a single file for commit.
        /// </summary>
        /// <param name="filePath">The path to the file to stage.</param>
        /// <returns>True if the file was successfully staged; otherwise, false.</returns>
        public bool GitAddSingleFile(string filePath)
        {
            Commands.Stage(_repo, filePath);
            return true;
        }

        /// <summary>
        /// Rolls back a single file to its state in a specified commit.
        /// </summary>
        /// <param name="commitHash">The hash of the commit to roll back to.</param>
        /// <param name="filePath">The path to the file to roll back.</param>
        /// <returns>True if the rollback was successful; otherwise, false.</returns>
        public bool Rollback(string commitHash, string filePath)
        {
            Commit commit = _repo.Commits.FirstOrDefault(c => c.Sha.StartsWith(commitHash));
            if (commit == null)
            {
                Debug.LogError("Commit not found.");
                return false;
            }

            var treeEntry = commit[filePath];
            if (treeEntry?.TargetType == TreeEntryTargetType.Blob)
            {
                Blob blob = (Blob)treeEntry.Target;
                string fullFilePath = Path.Combine(_repositoryPath, filePath).Replace("\\.git", "");

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath));
                    using (var contentStream = blob.GetContentStream())
                    {
                        using (var fileStream = File.Create(fullFilePath))
                        {
                            contentStream.CopyTo(fileStream);
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Exception occurred during rollback: {ex.Message}");
                    return false;
                }
            }
            else
            {
                Debug.LogError("File not found in commit.");
                return false;
            }
        }

        #endregion

        #region File Status Methods

        public bool IsFileModified(string filePath)
        {
            using (var repo = new Repository(GitostoryConfig.Paths.REPOSITORY_ROOT))
            {
                var status = repo.RetrieveStatus(filePath);
                return IsModified(status);
            }
        }

        private bool IsModified(FileStatus status)
        {
            return (status & (FileStatus.ModifiedInIndex | FileStatus.ModifiedInWorkdir)) != 0;
        }

        public bool IsFileNew(string filePath)
        {
            using (var repo = new Repository(GitostoryConfig.Paths.REPOSITORY_ROOT))
            {
                var status = repo.RetrieveStatus(filePath);
                return IsNew(status);
            }
        }

        private bool IsNew(FileStatus status)
        {
            return (status & (FileStatus.NewInIndex | FileStatus.NewInWorkdir)) != 0;
        }

        public bool IsRenamed(string filePath)
        {
            using (var repo = new Repository(GitostoryConfig.Paths.REPOSITORY_ROOT))
            {
                var status = repo.RetrieveStatus(filePath);
                return IsRenamed(status);
            }
        }

        private bool IsRenamed(FileStatus status)
        {
            return (status & (FileStatus.RenamedInIndex | FileStatus.RenamedInWorkdir)) != 0;
        }

        public bool IsDeleted(string filePath)
        {
            using (var repo = new Repository(GitostoryConfig.Paths.REPOSITORY_ROOT))
            {
                var status = repo.RetrieveStatus(filePath);
                return IsDeleted(status);
            }
        }

        private bool IsDeleted(FileStatus status)
        {
            return (status & (FileStatus.DeletedFromIndex | FileStatus.DeletedFromWorkdir)) != 0;
        }

 
        public bool Exists(string filePath)
        {
            using (var repo = new Repository(GitostoryConfig.Paths.TEMP_ROOT))
            {
                var status = repo.RetrieveStatus(filePath);
                return Exists(status);
            }
        }

        private bool Exists(FileStatus status)
        {
            const FileStatus doesNotExistFlags =
                FileStatus.Nonexistent | FileStatus.DeletedFromIndex | FileStatus.DeletedFromWorkdir;

            return (status & doesNotExistFlags) == 0;
        }

        public bool Exists(StatusEntry statusEntry)
        {
            return Exists(statusEntry.State);
        }

        #endregion
    }
}
