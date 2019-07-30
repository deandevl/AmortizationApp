using System.IO;

namespace AmortizationApp {
  public class Program {
    static void Main(string[] args) {
      string currentDir = Directory.GetCurrentDirectory();
      string serverBaseFolder = Path.Combine(currentDir, "html", "dist");
      Handlers handlers = new Handlers(serverBaseFolder);
      handlers.StartServer();
    }
  }
}
