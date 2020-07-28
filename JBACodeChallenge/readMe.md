# JBACodeChallenge

A small console app that parses a .pre file and writes the values to a SQLite database.

For some reason, my .git directory held some huge files so I couldn’t push, so I had to re-initialise the repo.

## Explanation
To avoid permission/ environment variables issues I use SQLITE as the database. 
The program is a console application that takes your arguments as inputs. 


## Setup 
Download the code, open in Visual Studio. Install the dependencies in _JBACodeChallenge.csproj_ using nuget.

## How to run
To run, set the arguments for the program in the project properties - the first argument is the database path (a .db file) and the second is the .pre- file path. 

Change "commandLineArgs" in _launchSettings.json_ to valid values for your setup.
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

The first argument is the database (SQLite, extension .db) path. The second argument is the path of the pre (extension .pre) file.
The arguments are seperated by a space.

If there is no database at the path you specify, the program will attempt to create one for you.



## Speed
This progam is really bogged down by IO. Currently, on my (old, hard drive) PC, it takes ~1.5 minutes to run fully. Re-writing it in a way that uses less calls to streamReader.ReadLine() would greatly speed it up. Reading the entire file into memory
in one go would be the fastest way. However, we can't be sure how big the file will be; it could potentially be GB in size. 

Instead we could read in the file chunk by chunk, several blocks at a time. 
