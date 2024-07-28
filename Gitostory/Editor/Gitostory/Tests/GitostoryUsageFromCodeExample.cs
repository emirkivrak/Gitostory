using GitostorySpace;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This file is an example of how to use Gitostory from code.
/// </summary>

public class GitostoryUsageFromCodeExample 
{
   public static void Test(string filePath)
    {
        Gitostory gitostory = new Gitostory();

        // Getting current file status.
        bool isFileModified = gitostory.IsFileModified(filePath);
        Debug.Log("Is File Modified: " + isFileModified);

        // Getting all past commits where this file changed.
        List<GitostoryPastCommitData> allCommitsForFile = gitostory.GetAllCommits(filePath);
        foreach (var commit in allCommitsForFile)
        {
            Debug.Log("Commit Hash: " + commit.CommitHash);
            Debug.Log("Commit Message: " + commit.CommitMessage);
            Debug.Log("Commit Date: " + commit.CommitDate);
            Debug.Log("Commit Author: " + commit.Author);
        }


        // Get a older version of file. uncomment for test.
        //var previousCommitFilePath = gitostory.GetFilePreviousVersion(allCommitsForFile[0].CommitHash, ExampleFilePath);
        //Debug.Log("previousCommitFile Path : " + previousCommitFilePath);
    }
}
