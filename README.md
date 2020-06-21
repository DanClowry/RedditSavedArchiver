# RedditSavedArchiver

A .NET Core app for archiving your saved posts on Reddit. Automatically archives
your saved posts to a local database and can optionally crosspost your saved
posts to a subreddit for easy searching and filering.

## Installing / Getting started

Download the .NET Core 3.1 runtime from the
[Microsoft website](https://dotnet.microsoft.com/download) if you dont already
have it.

Download and extract the latest release for your operating system from the
[Github releases page](https://github.com/DanClowry/RedditSavedArchiver/releases/latest)

The program can then be run through your favourite terminal:

* For Windows
    ```bash
    cd path\to\installation
    RedditArchiver.exe
    ```
* For Linux
    ```bash
    cd path/to/installation
    ./RedditArchiver
    ```

However, before you can use the program you must first configure it so it can
access your account. See the following section for information on configuring
the program.

### Initial Configuration

For the program to view your saved posts it must have permission to access your
account.

#### Creating the app

To create an app, go to your [Reddit account preferences](https://reddit.com/prefs/apps/)
and click the "Create app" button at the bottom of the page.

Fill out the new application form with the following information:

* **Name:** Can be anything you want. "Saved Posts Archiver" is recommended so
you can quickly identify it
* **App type:** Installed app
* **Description:** You can leave this blank
* **About URL:** You can leave this blank
* **Redirect URI:** MUST BE http://localhost:9696/redditArchiver/callback

Once you have filled out the form click "Create app". The app should now appear
under your developed applications. Make a note of the 15 character app ID
located under the app's name as this will be used in the later steps.

#### Obtaining OAuth tokens

When you first run the program, or when Reddit returns an unauthorised/forbidden
response, the program will prompt you to run the credential setup.

To obtain your OAuth tokens using the credential setup:

1. Run the program. It will automatically prompt you to run the user credential
setup when Reddit returns an unauthorised/forbidden response (e.g. when you are
using the default appsettings.json). Press "Y" to start the user-credential
setup.
2. The program will ask for an app ID. Enter the app ID you copied down when
you [created the app](#creating-the-app).
3. The program will then display a URL. Copy this URL, open it in your browser
and click the "Allow" button.
4. After clicking allow you should then be redirected to a page with your
access and refresh tokens. Copy these down as they will be used in the next step.
    * You can verify that the app has been authorised by going to your
    [Reddit account preferences](https://reddit.com/prefs/apps/) and checking the
    "Authorised applications" list

If you have enabled crossposting in your settings but have disabled
crossposting using the user account, you will then be prompted to run the
bot-account credential setup. Follow the same steps as above however **make sure
you are signed into Reddit using the bot account before opening URL in your
browser**.


#### Adding your tokens to the archiver

*For more information on configuring the archiver see the
[configuration](#configuration) section.*

Now that you have your tokens, you need to tell the archiver to use them.

In the installation directory for the program there should be a file called
`appsettings.json`. Open the file in a text editor such as Notepad or vim and
look for the section called "`UserCredentials`". It will probably look something
 like this:

```json
...
"UserCredentials": {
      "AppID": "YourAppID",
      "AccessToken": "YourAccessToken",
      "RefreshToken": "YourRefreshToken"
    }
...
```

Replace:
* `YourAppID` with the app ID you copied down when [creating the app](#creating-the-app)
* `YourAccessToken` with the access token you copied down when you
[obtained your OAuth tokens](#obtaining-oauth-tokens)
* `YourRefreshToken` with the refresh token you copied down when you
[obtained your OAuth tokens](#obtaining-oauth-tokens)

It is also a good idea to replace `/u/YourName` with your Reddit username in
the `UserAgent` property so Reddit can easily contact you.

If you have enabled crossposting using a bot account, you should also enter
your app ID, the bot's access token, and the bot's refresh token into the
"`BotCredentials`" section.

```json
...
"BotCredentials": {
        "AppID": "YourAppID",
        "AccessToken": "BotAccessToken",
        "RefreshToken": "BotRefreshToken"
      }
...
```

Save and close the file. If you now run the program it should begin archiving
your posts to the file `savedPosts.db`. For information on configuring crossposting
or changing where the database is stored, see the [configuration](#configuration)
section.

## Configuration

There are three ways to configure the archiver:
* **Configuration file** - Settings are stored in the file
`appsettings.json` located in the same directory as the executable.
* **Environment variables** - Variables must be prefixed with `REDDIT_ARCHIVER_`.
Will override any values set in the configuration file. For more information on
using environment variables see the [.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#environment-variables).
This document will use the double hyphen format in its examples.
* **Command-line arguments** - Settings passed in using command-line arguments.
Will override any values set in the configuration file or environement variables.
For more information on using command-line arguments see the
[.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1#command-line). This document will use the
Windows commands in its examples.

### Default Appsettings.json

```json
{
  "ConnectionStrings": {
    "SqliteLocation": "savedPosts.db"
  },
  "Reddit": {
    "UserCredentials": {
      "AppID": "YourAppID",
      "AccessToken": "YourAccessToken",
      "RefreshToken": "YourRefreshToken"
    },
    "UserAgent": "windows:reddit-save-archiver:v0.1.0 (by /u/YourName)",
    "Crosspost": {
      "EnableCrossposting": false,
      "CrosspostSubreddit": "SubredditName",
      "UseUserAccount": false,
      "BotCredentials": {
        "AppID": "YourAppID",
        "AccessToken": "BotAccessToken",
        "RefreshToken": "BotRefreshToken"
      }
    }
  }
}
```

### ConnectionStrings

#### SqliteLocation
Type: `string`
Default: `'savedPosts.db'`

The location of the SQLite database used to archive posts to. File will be
automatically created if it doesn't exist.

:warning: Relative paths are relative to the current working directory.
Be careful if you plan to run the program from multiple locations (cron job,
another script, etc) as they will not detect a database stored in the program's
installation directory.

Examples:

```json
"ConnectionStrings": {
    "SqliteLocation": "savedPosts.db"
  }
```

```bash
RedditArchiver.exe --ConnectionStrings:SqliteLocation savedPosts.db
```

```bash
setx REDDIT_ARCHIVER_ConnectionStrings__SqliteLocation savedPosts.db
```

### Reddit

#### UserCredentials

The credentials used to access the user's account. The user's account is used
when getting saved posts.

##### AppID
Type: `string`  
Default: `'YourAppID'`

The ID of the application registered with Reddit. See [creating the app](#creating-the-app)
for more information.

Examples:

```json
{
  "Reddit": {
    "UserCredentials": {
      "AppID": "YourAppID",
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:UserCredentials:AppID YourAppID
```

```bash
setx REDDIT_ARCHIVER_Reddit__UserCredentials__AppID YourAppID
```

##### AccessToken
Type: `string`  
Default: `'YourAccessToken'`

The access token for the user's account. See [obtaining OAuth tokens](#obtaining-oauth-tokens)
for more information.

Examples:

```json
{
  "Reddit": {
    "UserCredentials": {
      "AccessToken": "YourAccessToken"
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:UserCredentials:AccessToken YourAccessToken
```

```bash
setx REDDIT_ARCHIVER_Reddit__UserCredentials__AccessToken YourAccessToken
```

##### RefreshToken
Type: `string`  
Default: `'YourRefreshToken'`

The refresh token for the user's account. See [obtaining OAuth tokens](#obtaining-oauth-tokens)
for more information.

Examples:

```json
{
  "Reddit": {
    "UserCredentials": {
      "RefreshToken": "YourRefreshToken"
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:UserCredentials:RefreshToken YourRefreshToken
```

```bash
setx REDDIT_ARCHIVER_Reddit__UserCredentials__RefreshToken YourRefreshToken
```

#### UserAgent
Type: `string`  
Default: `'windows:reddit-save-archiver:v0.1.0 (by /u/YourName)'`

The user agent used when accessing the Reddit API. Used so Reddit can more
easily identify and contact the owner of the program.

Examples:

```json
{
  "Reddit": {
    "UserAgent": "windows:reddit-save-archiver:v0.1.0 (by /u/YourName)"
  }
}
```

```bash
RedditArchiver.exe --Reddit:UserAgent "windows:reddit-save-archiver:v0.1.0 (by /u/YourName)"
```

```bash
setx REDDIT_ARCHIVER_Reddit__UserAgent "windows:reddit-save-archiver:v0.1.0 (by /u/YourName)"
```

#### Crosspost

Settings related to the automatic crossposting of saved posts to a subreddit.
If `EnableCrossposting` is false, all other crosspost settings are optional and
are ignored by the program.

##### EnableCrossposting
Type: `bool`  
Default: `false`

Whether posts should be automatically crossposted to another subreddit.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "EnableCrossposting": false
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:EnableCrossposting false
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__EnableCrossposting false
```

##### CrosspostSubreddit
Type: `string`  
Default: `'SubredditName'`

The subreddit to crosspost to without the /r/ prefix.
The crosspost account must be a moderator of the subreddit.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "CrosspostSubreddit": "SubredditName"
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:CrosspostSubreddit SubredditName
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__CrosspostSubreddit SubredditName
```

##### UseUserAccount
Type: `bool`  
Default: `false`

Whether posts should be crossposted using the account related to the
`UserCredentials` settings or another account.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "UseUserAccount": false
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:UseUserAccount false
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__UseUserAccount false
```

##### BotCredentials

The credentials used to access the crosspost bot account. If `UseUserAccount` is
true, bot credentials are optional and are ignored by the program.

###### AppID
Type: `string`  
Default: `'YourAppID'`

The ID of the application registered with Reddit. Can be the same as the app ID
set for the user account. See [creating the app](#creating-the-app) for more information.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "BotCredentials": {
        "AppID": "YourAppID"
      }
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:BotCredentials:AppID YourAppID
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__BotCredentials__AppID YourAppID
```

###### AccessToken
Type: `string`  
Default: `'BotAccessToken'`

The access token for the bot account. Log in to Reddit using the bot account
in your browser and repeat the steps in [obtaining OAuth tokens](#obtaining-oauth-tokens)
to get the access and refresh tokens for the bot account.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "BotCredentials": {
        "AccessToken": "BotAccessToken"
      }
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:BotCredentials:AccessToken BotAccessToken
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__BotCredentials__AccessToken BotAccessToken
```

###### RefreshToken
Type: `string`  
Default: `'BotRefreshToken'`

The refresh token for the bot account. Log in to Reddit using the bot account
in your browser and repeat the steps in [obtaining OAuth tokens](#obtaining-oauth-tokens)
to get the access and refresh tokens for the bot account.

Examples:

```json
{
  "Reddit": {
    "Crosspost": {
      "BotCredentials": {
        "RefreshToken": "BotRefreshToken"
      }
    }
  }
}
```

```bash
RedditArchiver.exe --Reddit:Crosspost:BotCredentials:RefreshToken BotRefreshToken
```

```bash
setx REDDIT_ARCHIVER_Reddit__Crosspost__BotCredentials__RefreshToken BotRefreshToken
```

## Licence

[GPL-v3](https://github.com/DanClowry/RedditSavedArchiver/blob/master/LICENSE)
