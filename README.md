# Dotnet-Security
This repo has a an example how to use .NET Identity to make a MVC project with user login/logout functionality with permission levels. This project implements a custom user class with a new claim for the user's name. Permission levels are implemented using Roles and those roles are what protect action methods from running. This also has some built in data using a local SqLite database which populates data to the new database when you run this project on a new machine.

## Before Running
When you clone this repo down you should run an update database command to register both database contexts and databases.
- if you are using `Nuget Package Manager` in Visual Studio run these 2 commands
  - `update-database -context MovieDbContext`
  - `update-database -context AppDbContext`
- if you are using Visual Studio Code the open the integrated terminal(or any other terminal) and run these: (If these commands don't work for you use [this link](https://docs.microsoft.com/en-us/ef/core/cli/dotnet) to install dotnet Entity Framework tools on your machine) 
  - `dotnet ef database update --context MovieDbContext`
  - `dotnet ef database update --context AppDbContext`
  
## Contributions
I appreciate your time to go through this. Feel free to leave an Issue for something you'd like changed or leave a Pull Request to guide me how. Thank you!
  
