using JetBrains.Annotations;
using LibGit2Sharp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GitostorySpace
{
    public static class GitostoryConfig
    {
        public static class Paths
        {
            public const string TEMP_ROOT_KEY_DEFAULT = "Gitostory_TempRoot";
            public const string REPOSITORY_ROOT_KEY_DEFAULT = "Gitostory_RepositoryRoot";

            public static string TEMP_ROOT
            {
                get => EditorPrefs.GetString(TEMP_ROOT_KEY_DEFAULT, "Assets/Gitostory/Editor/Temp");
                set => EditorPrefs.SetString(TEMP_ROOT_KEY_DEFAULT, value);
            }

            public static string REPOSITORY_ROOT
            {
                get
                {
                    if (!EditorPrefs.HasKey(REPOSITORY_ROOT_KEY_DEFAULT))
                    {
                        var defaultPath = new GitostoryRepositoryService().GetProjectRepository().Info.Path;
                        EditorPrefs.SetString(REPOSITORY_ROOT_KEY_DEFAULT, defaultPath);
                    }
                    return EditorPrefs.GetString(REPOSITORY_ROOT_KEY_DEFAULT);
                }
                set => EditorPrefs.SetString(REPOSITORY_ROOT_KEY_DEFAULT, value);
            }
        }
    }
}
