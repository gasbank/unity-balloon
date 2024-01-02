using System;
using System.Collections;
using System.Text;
#if !NO_GPGS
using GooglePlayGames;
using GooglePlayGames.BasicApi.SavedGame;
#endif
using UnityEngine;
using UnityEngine.UI;
//using GooglePlayGames.BasicApi;

[DisallowMultipleComponent]
public class PlatformAfterLogin : MonoBehaviour
{
    public int daysPlayed;
    public int leaderboardScore1;

    public int leaderboardScore2;

    //public CanvasGroup rootCanvasGroup;
    //public CanvasGroup afterLoginCanvasGroup;
    public Text loginStateText;

    void Awake()
    {
        //afterLoginCanvasGroup.interactable = false;
    }

    IEnumerator Start()
    {
        if (loginStateText) loginStateText.text = "INIT";

        while (true)
        {
            yield return new WaitUntil(() => Social.localUser.authenticated);

            if (loginStateText) loginStateText.text = "LOGGED IN";
            //afterLoginCanvasGroup.interactable = true;

            yield return new WaitUntil(() => !Social.localUser.authenticated);

            if (loginStateText) loginStateText.text = "LOGGED OUT";
            //afterLoginCanvasGroup.interactable = false;
        }
    }

    public void ShowLeaderboard()
    {
        if (Platform.instance.CheckLoadSavePrecondition(TextHelper.GetText("platform_logging_in"), () =>
                PlatformSaveUtil.StartLoginAndDoSomething(() =>
                {
                    ConfirmPopup.instance.Close();
                    ExecuteShowLeaderboard();
                }), ShowLoginFailed) == false) return;

        ExecuteShowLeaderboard();
        ProgressMessage.instance.Close();
    }

    void ExecuteShowLeaderboard()
    {
        BalloonLogManager.Add(BalloonLogEntry.Type.GameOpenLeaderboard, 0, 0);
        if (Application.isEditor)
            ShortMessage.instance.Show("Leaderboard not supported in Editor");
        else
            Social.ShowLeaderboardUI();
    }

    void ShowLoginFailed()
    {
        ConfirmPopup.instance.Open(TextHelper.GetText("platform_login_failed_popup"));
    }

    public void ShowAchievements()
    {
        if (Platform.instance.CheckLoadSavePrecondition(TextHelper.GetText("platform_logging_in"), () =>
                PlatformSaveUtil.StartLoginAndDoSomething(() =>
                {
                    ConfirmPopup.instance.Close();
                    ExecuteShowAchievements();
                }), ShowLoginFailed) == false) return;

        ExecuteShowAchievements();
        ProgressMessage.instance.Close();
    }

    void ExecuteShowAchievements()
    {
        BalloonLogManager.Add(BalloonLogEntry.Type.GameOpenAchievements, 0, 0);
        if (Application.isEditor)
            ShortMessage.instance.Show("Achievements not supported in Editor");
        else
            Social.ShowAchievementsUI();
    }

    public void UnlockAchievement1()
    {
        //rootCanvasGroup.interactable = false;
        var id = Application.platform == RuntimePlatform.Android ? "CgkI87XJzNYNEAIQAQ" : "finishTutorial1";
        Social.ReportProgress(id, 100.0f, success =>
        {
            BalloonDebug.LogFormat("Unlock achievement 1 result: {0}", success);
            //rootCanvasGroup.interactable = true;
        });
    }

    public void UnlockAchievement2()
    {
        //rootCanvasGroup.interactable = false;
        var id = Application.platform == RuntimePlatform.Android ? "CgkI87XJzNYNEAIQAQ" : "share_screenshot2";
        Social.ReportProgress(id, 100.0f, success =>
        {
            BalloonDebug.LogFormat("Unlock achievement 2 result: {0}", success);
            //rootCanvasGroup.interactable = true;
        });
    }

    public void PostLeaderboardScore1()
    {
        //rootCanvasGroup.interactable = false;
        leaderboardScore1++;
        Social.ReportScore(leaderboardScore1, "CgkI87XJzNYNEAIQAw", success =>
        {
            BalloonDebug.LogFormat("PostLeaderboardScore1 result: {0}, score: {1}", success, leaderboardScore1);
            //rootCanvasGroup.interactable = true;
        });
    }

    public void PostLeaderboardScore2()
    {
        //rootCanvasGroup.interactable = false;
        leaderboardScore2++;
        Social.ReportScore(leaderboardScore2, "CgkI87XJzNYNEAIQBA", success =>
        {
            BalloonDebug.LogFormat("PostLeaderboardScore2 result: {0}, score: {1}", success, leaderboardScore2);
            //rootCanvasGroup.interactable = true;
        });
    }

    public void ShowSavedGameSelectUI()
    {
        if (Application.isEditor || Application.platform != RuntimePlatform.Android)
        {
            ShortMessage.instance.Show("Saved Game Select UI not supported!");
            return;
        }
#if !NO_GPGS
        //rootCanvasGroup.interactable = false;

        uint maxNumToDisplay = 5;
        var allowCreateNew = false; // 새로운 세이브 파일 만드는 것 지원하지 않는다.
        var allowDelete = true; // 삭제는 지원하자.

        if (PlayGamesPlatform.Instance != null)
        {
            var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            if (savedGameClient != null)
                savedGameClient.ShowSelectSavedGameUI(
                    "Select saved game",
                    maxNumToDisplay,
                    allowCreateNew,
                    allowDelete,
                    OnSavedGameSelected);
            else
                Debug.LogWarning("PlayGamesPlatform.Instance.SavedGame null");
        }
        else
        {
            Debug.LogWarning("PlayGamesPlatform.Instance null");
        }
#endif
    }


#if !NO_GPGS
    public void OnSavedGameSelected(SelectUIStatus status, ISavedGameMetadata game)
    {
        if (status == SelectUIStatus.SavedGameSelected)
        {
            // handle selected game save
            BalloonDebug.LogFormat("Save game selection selected! Selected save filename: {0}", game.Filename);
            ShortMessage.instance.Show("ERROR: Not supported");
        }
        else
        {
            // handle cancel or error
            BalloonDebug.LogFormat("Save game selection canceled! - {0}", status);
        }

        //rootCanvasGroup.interactable = true;
    }
    
    public void OpenDefaultSavedGameAndWrite()
    {
        //rootCanvasGroup.interactable = false;
        OpenSavedGameAndWrite("default-saved-game");
    }

    public void OpenDefaultSavedGameAndRead()
    {
        //rootCanvasGroup.interactable = false;
        OpenSavedGameAndRead("default-saved-game");
    }

    void OpenSavedGameAndWrite(string filename)
    {
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null)
            PlatformAndroid.Open(
                savedGameClient,
                true,
                null,
                OnSavedGameOpenedAndWrite);
    }

    void OpenSavedGameAndRead(string filename)
    {
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        if (savedGameClient != null)
        {
            PlatformAndroid.Open(
                savedGameClient,
                true,
                null,
                OnSavedGameOpenedAndRead);
        }
    }


    public void OnSavedGameOpenedAndWrite(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.

            BalloonDebug.LogFormat("Save game open (write) success! Filename: {0}", game.Filename);

            daysPlayed++;

            SaveGame(game, Encoding.UTF8.GetBytes("Hello save file world!" + DateTime.Now),
                TimeSpan.FromDays(daysPlayed));
        }
        else
        {
            // handle error
            BalloonDebug.LogFormat("Save game open (write) failed! - {0}", status);
            //rootCanvasGroup.interactable = true;
        }
    }

    public void OnSavedGameOpenedAndRead(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            // handle reading or writing of saved game.

            BalloonDebug.LogFormat("Save game open (read) success! Filename: {0}", game.Filename);

            daysPlayed++;

            LoadGameData(game);
        }
        else
        {
            // handle error
            BalloonDebug.LogFormat("Save game open (read) failed! - {0}", status);

            //rootCanvasGroup.interactable = true;
        }
    }

    void SaveGame(ISavedGameMetadata game, byte[] savedData, TimeSpan totalPlaytime)
    {
#if !NO_GPGS
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        var builder = new SavedGameMetadataUpdate.Builder();
        builder = builder
            .WithUpdatedPlayedTime(totalPlaytime)
            .WithUpdatedDescription("Saved game at " + DateTime.Now);

        var savedImage = getScreenshot();
        if (savedImage != null)
        {
            // This assumes that savedImage is an instance of Texture2D
            // and that you have already called a function equivalent to
            // getScreenshot() to set savedImage
            // NOTE: see sample definition of getScreenshot() method below
            var pngData = savedImage.EncodeToPNG();
            builder = builder.WithUpdatedPngCoverImage(pngData);
        }

        var updatedMetadata = builder.Build();
        savedGameClient.CommitUpdate(game, updatedMetadata, savedData, OnSavedGameWritten);
#endif
    }

    public void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
            // handle reading or writing of saved game.

            BalloonDebug.LogFormat("OnSavedGameWritten success!");
        else
            // handle error

            BalloonDebug.LogFormat("OnSavedGameWritten failed! - {0}", status);

        //rootCanvasGroup.interactable = true;
    }

    public Texture2D getScreenshot()
    {
        // Create a 2D texture that is 1024x700 pixels from which the PNG will be
        // extracted
        var screenShot = new Texture2D(1024, 700);

        // Takes the screenshot from top left hand corner of screen and maps to top
        // left hand corner of screenShot texture
        screenShot.ReadPixels(
            new Rect(0, 0, Screen.width, Screen.width / 1024 * 700), 0, 0);

        return screenShot;
    }

    public void LoadGameData(ISavedGameMetadata game)
    {
        var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, OnSavedGameDataRead);
    }

    public void OnSavedGameDataRead(SavedGameRequestStatus status, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
            // handle processing the byte array data

            BalloonDebug.LogFormat("OnSavedGameDataRead success! - Data: " + Encoding.UTF8.GetString(data));
        else
            // handle error

            BalloonDebug.LogFormat("OnSavedGameDataRead failed! - {0}", status);

        //rootCanvasGroup.interactable = true;
    }

    //public void DeleteGameData(string filename)
    //{
    //    // Open the file to get the metadata.
    //    ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
    //    savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
    //        ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
    //}

    public void DeleteSavedGame(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            var savedGameClient = PlayGamesPlatform.Instance.SavedGame;
            savedGameClient.Delete(game);

            BalloonDebug.LogFormat("DeleteSavedGame success!");
        }
        else
        {
            // handle error

            BalloonDebug.LogFormat("DeleteSavedGame failed! - {0}", status);
        }
    }
#endif
}