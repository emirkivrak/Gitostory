using UnityEditor;

namespace GitostorySpace
{
    public static class GitostoryConfig
    {
        public static class Paths
        {
            public const string TEMP_ROOT_KEY_DEFAULT = "Gitostory_TempRoot";
            public const string REPOSITORY_ROOT_KEY_DEFAULT = "Gitostory_RepositoryRoot";
            public const string GITIGNORE_PATH_KEY_DEFAULT = "Gitostory_GitignorePath";

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

            public static string GITIGNORE_PATH
            {
                get => EditorPrefs.GetString(GITIGNORE_PATH_KEY_DEFAULT, ".gitignore");
                set => EditorPrefs.SetString(GITIGNORE_PATH_KEY_DEFAULT, value);
            }
        }

        public static class Preferences
        {
            public const string RESET_WITH_META_FILE_KEY_DEFAULT = "Gitostory_ResetWithMetaFile";
            public const string TRACK_RENAMES_AND_FOLDER_CHANGES_KEY_DEFAULT = "Gitostory_TrackRenamesAndFolderChanges";

            public static bool ResetWithMetaFile
            {
                get => EditorPrefs.GetBool(RESET_WITH_META_FILE_KEY_DEFAULT, true);
                set => EditorPrefs.SetBool(RESET_WITH_META_FILE_KEY_DEFAULT, value);
            }

            public static bool TrackRenamesAndFolderChanges
            {
                get => EditorPrefs.GetBool(TRACK_RENAMES_AND_FOLDER_CHANGES_KEY_DEFAULT, false);
                set => EditorPrefs.SetBool(TRACK_RENAMES_AND_FOLDER_CHANGES_KEY_DEFAULT, value);
            }
        }
    }
}
