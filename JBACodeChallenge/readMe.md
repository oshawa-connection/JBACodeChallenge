# JBACodeChallenge

A small console app that parses a .pre file and writes the values to a SQLite database.

## Setup 
Download the code, open in Visual Studio. Install the dependencies in _JBACodeChallenge.csproj_ using nuget.

## How to run
Change "commandLineArgs" in _launchSettings.json_ to valid values for your setup.
The first argument is the database (SQLite, extension .db) path. The second argument is the path of the pre (extension .pre) file.
The arguments are seperated by a space.

If there is no database at the path you specify, the program will attempt to create one for you.

For example: 
```javascript 
{
  "profiles": {
    "JBACodeChallenge": {
      "commandName": "Project",
      "commandLineArgs": "C:\\Users\\James\\source\\repos\\JBACodeChallenge\\JBACodeChallenge\\files\\JBATestDatabase.db C:\\Users\\James\\source\\repos\\JBACodeChallenge\\JBACodeChallenge\\files\\JBATestFile.pre"
    }
  }
}
```

## Async vs Sync
This progam is 100% synchronous. Processing the stream piece by piece with Tasks or [asynchronous streams](https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/generate-consume-asynchronous-stream) could greatly speed it up.
Currently, on my (old) PC, it takes ~1.5 minutes to run fully.
