using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using LibGit2Sharp;
using UnityEngine;

namespace GitostorySpace
{
    public interface IRepositoryService
    {
        IRepository GetRepository(string directoryPath);

        IRepository GetProjectRepository();

        List<IRepository> GetPackageRepositories();

        bool IsProjectRepository(IRepository repository);

        string GetProjectRepositoryName();

        string GetRepositoryName(IRepository repository);

        bool AreRepositoriesEqual(IRepository repositoryOne, IRepository repositoryTwo);

        bool IsValid(IRepository repository);
    }

    public class GitostoryRepositoryService : IRepositoryService
    {
        [CanBeNull]
        public IRepository GetRepository(string repositoryPath)
        {
            if (!Directory.Exists(repositoryPath))
                return null;

            try
            {
                if (!Repository.IsValid(repositoryPath))
                    return null;
            }
            catch
            {
                return null;
            }

            return new Repository(repositoryPath);
        }

        [CanBeNull]
        public IRepository GetProjectRepository()
        {
            var gitRepositoryPath = Path.Combine(Directory.GetCurrentDirectory(), ".git");
            return GetRepository(gitRepositoryPath);
        }

        public List<IRepository> GetPackageRepositories()
        {
            var repositories = new List<IRepository>();
            var packagesFullPath = Path.GetFullPath("Packages");
            var packagesDirectoryInfo = new DirectoryInfo(packagesFullPath);

            AddRepositoriesInDirectory(packagesDirectoryInfo, repositories);

            return repositories;
        }

        public bool IsProjectRepository(IRepository repository)
        {
            return AreRepositoriesEqual(GetProjectRepository(), repository);
        }

        private static void AddRepositoriesInDirectory(DirectoryInfo directoryInfo,
            ICollection<IRepository> repositories)
        {
            var gitDirectories =
                directoryInfo.GetDirectories(".git", SearchOption.AllDirectories);

            foreach (var directory in gitDirectories)
            {
                var alreadyExists =
                    repositories.Any(repository => repository.Info.Path.Equals(directory.FullName));

                if (alreadyExists)
                    continue;

                if (Repository.IsValid(directory.FullName))
                    repositories.Add(new Repository(directory.FullName));
            }
        }

        public string GetProjectRepositoryName()
        {
            return Application.productName;
        }

        public string GetRepositoryName(IRepository repository)
        {
            var packagePath = repository.Info.Path;

            var packagePathParts = packagePath.Split(Path.DirectorySeparatorChar);

            int packageNameIndex;

            if (packagePath[packagePath.Length - 1] == Path.DirectorySeparatorChar)
                packageNameIndex = packagePathParts.Length - 3;
            else
                packageNameIndex = packagePathParts.Length - 2;

            return packagePathParts[packageNameIndex];
        }

        public bool AreRepositoriesEqual(IRepository repositoryOne, IRepository repositoryTwo)
        {
            if (repositoryOne == null || repositoryTwo == null)
                return false;

            return repositoryOne.Info.Path.Equals(repositoryTwo.Info.Path);
        }

        public bool IsValid(IRepository repository)
        {
            return Repository.IsValid(repository.Info.Path);
        }
    }
}