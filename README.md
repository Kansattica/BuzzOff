# Buzz Off!
## A simple little quiz show "who buzzed in first" thing.

Try it out at [buzzoff.princess.software](https://buzzoff.princess.software)!

### Run Your Own

1. Make sure you have the latest [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) SDK.

2. Clone the repo to your machine.

### Command Line

3. `cd Server`. You'll be doing the rest of this from in here.

4. Run `dotnet run --configuration Release` to compile and run the server. You'll be given your choice of two addresses to point your browser at. If you want to use the HTTPS endpoint, you'll have to run `dotnet dev-certs https --trust` first.

5. If you want to deploy this to some other server with just the .NET runtime, you'll want to `dotnet publish --configuration Release`, copy the folder to where you want it, and and run `dotnet BuzzOff.Server.dll` there.

### Visual Studio

3. Open `BuzzOff.sln`.

4. Hit F5 and watch it go.

### Notes

- If you want the git hash and build date on the landing page to be accurate, have your CI system replace the strings `@@GIT_HASH@@` and `@@BUILD_TIME@@` in BuildHash.cs. I use two `sed` commands.

- If you want to run this in an Azure app service, you can enable Application Insights by giving it an application setting named `APPINSIGHTS_INSTRUMENTATIONKEY` with your App Insights instrumentation key as the value.
