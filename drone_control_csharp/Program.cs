using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using drone_control_csharp;
using TelloCommander.CommandDictionaries;
using TelloCommander.Commander;
using TelloCommander.Connections;
using TelloCommander.Interfaces;


const string simFilePath = "TelloSimulator";
const ConnectionType DroneMode = ConnectionType.Mock; //3 modes, drone = real drone, simulation = use with sim program, mock = will just accept all verified commands
const bool streamVideo = true;
VideoStream stream = new VideoStream();

Console.WriteLine("Starting connection...");
ITelloConnection connection; //use the drones ip address and drone mode

CommandDictionary? dictionary = CommandDictionary.ReadStandardDictionary("1.3.0.0"); //used to see what commands are valid for this version
switch (DroneMode)
{
    case ConnectionType.Mock:
        connection = new MockTelloConnection(dictionary);
        break; 
    case ConnectionType.Simulator:
        connection = new TelloConnection(IPAddress.Loopback.ToString(), TelloConnection.DefaultTelloPort, DroneMode);
        break;
    case ConnectionType.Drone:
        connection = new TelloConnection();
        break;
    default:
        connection = new TelloConnection();
        break;
}

DroneCommander commander = new(connection, dictionary); //commander is the drone

//lots of exception handling
try
{
    commander.Connect();
    if (streamVideo)
    {
        commander.RunCommand("streamon");
        stream.StartStream();
    }
}
catch (SocketException ex)
{
    if (ex.ErrorCode == 111) {Console.WriteLine("Connection refused. The simulator or drone might not be running or needs to restart.");}
    Console.WriteLine(ex.Message);
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    Environment.Exit(1);
}

bool isEmpty = false;
while (!isEmpty)
{
    Console.Write("Please enter a command or press [ENTER] to quit : ");
    string command = Console.ReadLine()!.Trim(); //allowed to be null (I think)
    isEmpty = string.IsNullOrEmpty(command);
    if (!isEmpty)
    {
        try
        {
            Console.WriteLine($"Command  : {command}");
            commander.RunCommand(command); //sends a command to the drone. All commands can be found here*: https://dl-cdn.ryzerobotics.com/downloads/tello/20180910/Tello%20SDK%20Documentation%20EN_1.3.pdf
            Console.WriteLine($"Response : {commander.LastResponse}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}


try
{
    Console.WriteLine("Landing...");
    commander.RunCommand("land");
    Console.WriteLine($"{commander.LastResponse}");
    stream.StopRecording();
    if (DroneMode == ConnectionType.Simulator)
    {
        Console.WriteLine("Stopping simulation...");
        commander.RunCommand("stopsimulator"); 
        Console.WriteLine($"Response : {commander.LastResponse}"); //might be unneeded
    }
}
catch (Exception ex)
{ 
    Console.WriteLine(ex);
}

commander.Disconnect();

//* I do not know if the drone supports the 2.0.0 commands found here: https://dl-cdn.ryzerobotics.com/downloads/Tello/Tello%20SDK%202.0%20User%20Guide.pdf
//the website only links to the 1.3.0 version, so I'm using that.
//the 1.3.0 doc at page 5 also mentions a command to let the drone fly to a specific place in cm. (go command)
// user manual: https://terra-1-g.djicdn.com/2d4dce68897a46b19fc717f3576b7c6a/%E4%BA%A7%E5%93%81Info%20%E6%96%87%E4%BB%B6/Tello_User_Manual_V1.2_EN.pdf
//This is the repo with the nuget package: https://github.com/davewalker5/TelloCommander
//This is the wiki page on how to use the simulator: https://github.com/davewalker5/TelloCommander/wiki/Drone-Simulator
//for building the simulator, download the full GitHub repo and build TelloSimulator (only that one).
//If it complains about a missing Content folder, copy the one from this project to the bin folder.