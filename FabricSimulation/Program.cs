using System.Windows.Forms;

Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
using var game = new FabricSimulation.FabricSimulationDemo();
game.Run();
