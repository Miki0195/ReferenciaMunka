# Nitro Knights - Multiplayer bomberman

Now playable at [nitro-knights.ddns.net](https://nitro-knights.ddns.net) !

- Concept
- Docker
- Files / Running the source code
- Documentation

## Concept
Bomberman is a 2D real-time multiplayer game, where the goal of each player is to survive the longest. On some maps, other than the other players, monsters are trying to make the situation harder for everybody. You can collect different power-ups and negative modifiers, that help you win.<br>
**Please enjoy :)**

## Docker

### Images
The 2 docker images needed are uploaded to Docker Hub, the exact repository can be viewed [here](https://hub.docker.com/r/danimre/nitro-knights)

### Usage
- Install the docker engine [here](https://docs.docker.com/engine/install/)
- Navigate to the ```./Docker``` folder, there is a ```compose.yaml``` file, which contains the instructions for the docker engine, to pull, build and run the containers.
- Open a terminal or powershell window there and type ```docker compose up``` *(use the ```-d``` option if you want it to run in detached mode)*
- After a short delay the web app should be running on port 8080, [localhost link](http://localhost:8080)

## Files / Running the source code on a local machine
### Bomberman
This folder contains the source code for the game and the unit tests for it.<br>
The project 'Bomberman' has all the files for the game, the project BombermanTest has the unit tests for the game model, the controllers used and the SignalR hub.

### Documentation
This folder has all the documentation for the game.

### Docker
This folder contains the compose.yml file for the docker images.

### Others
- *publish_to_server.sh* - Shell script for publishing a build to the server used during the demo
- *.gitlab-ci.yml* - GitLab CI/CD file
- *.gitignore* - gitignore :)
- *README.md* - this file

### Running the app
The project was created with C# ASP.NET + SignalR for handling real-time communication between the clients and the server. To compile the code you need to have the .NET 8 SDK and ASP.NET 8 SDK. If you have everything you need to:
- configure settings in the *appsettings.json* files for your database provider
- configure network settings to work on your webserver (or run on localhost)
- go to directory *Bomberman/Bomberman*
- run `dotnet build` and `dotnet run`

If you have Visual Studio just open the solution and click on the big green debug button :)

## Documentation
[Documentation...](Home)