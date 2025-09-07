using System;
using System.Text;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

class WebSocketClient
{
    private static ClientWebSocket ws = new ClientWebSocket();
    
    private static async Task<string> GetPublicIP()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                string response = await client.GetStringAsync("https://api.ipify.org");
                return response.Trim();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"No se pudo obtener la IP pública: {ex.Message}");
            return "No disponible";
        }
    }
    
    private static async Task ConnectToServer(string serverAddress)
    {
        ws = new ClientWebSocket();
        Uri serverUri = new Uri(serverAddress);
        
        try
        {
            Console.WriteLine($"Conectando a: {serverAddress}");
            await ws.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Conectado al servidor WebSocket");

            // Start a task to listen for incoming messages
            _ = Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                while (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.Clear();
                        Console.WriteLine("=== MENSAJES DEL CHAT ===");
                        Console.WriteLine(receivedMessage);
                        Console.WriteLine("========================");
                        Console.Write("Envie un mensaje (o 'exit' para salir): ");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error recibiendo mensaje: {ex.Message}");
                        break;
                    }
                }
            });
        }
        catch (Exception e)
        {
            Console.WriteLine("Error de conexión: " + e.Message);
            Console.WriteLine("Verifique que:");
            Console.WriteLine("1. La dirección IP/dominio sea correcta");
            Console.WriteLine("2. El puerto esté abierto en el firewall del servidor");
            Console.WriteLine("3. El router tenga port forwarding configurado (para conexiones de internet)");
            throw;
        }
    }

    private static async Task SendMessage(string message)
    {
        try
        {
            if (ws.State == WebSocketState.Open)
            {
                byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
                await ws.SendAsync(new ArraySegment<byte>(bytesToSend), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            else
            {
                Console.WriteLine("No se puede enviar el mensaje. La conexión WebSocket no está abierta.");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Error al enviar el mensaje: " + e.Message);
        }
    }

    private static async Task StartLocalServer()
    {
        try
        {
            // Get the solution root directory and build the correct path
            string currentDirectory = Directory.GetCurrentDirectory();
            string projectRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(currentDirectory)));
            string serverPath = Path.Combine(projectRoot, "servidor-juego", "server.js");
            
            // Alternative: Use absolute path based on your project structure
            // string serverPath = @"c:\Users\Pro360\Documents\Workspace\JS-HTML-CSS\ServidorTest\servidor-juego\server.js";
            
            Console.WriteLine($"Buscando servidor en: {serverPath}");
            
            if (!File.Exists(serverPath))
            {
                Console.WriteLine("Error: No se encontró el archivo server.js");
                Console.WriteLine($"Ruta buscada: {serverPath}");
                Console.WriteLine("Verifique que el archivo existe en la ubicación correcta.");
                return;
            }
            
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "node",
                Arguments = $"\"{serverPath}\"", // Add quotes for paths with spaces
                UseShellExecute = false,
                CreateNoWindow = false,
                WorkingDirectory = Path.GetDirectoryName(serverPath) // Set working directory
            };

            Process serverProcess = Process.Start(startInfo);
            Console.WriteLine("Servidor local iniciado. Esperando 5 segundos para que se inicialice...");
            
            // Get and display IP addresses
            string publicIP = await GetPublicIP();
            Console.WriteLine("\n=== INFORMACIÓN DEL SERVIDOR ===");
            Console.WriteLine($"IP Pública: {publicIP}");
            Console.WriteLine($"Puerto: 8080");
            Console.WriteLine("\nPara conexiones desde internet, comparte:");
            Console.WriteLine($"ws://{publicIP}:8080");
            Console.WriteLine("\nNOTA: Debes configurar port forwarding en tu router para el puerto 8080");
            Console.WriteLine("=====================================\n");
            
            Thread.Sleep(5000); // Wait for server to start
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error iniciando servidor local: {ex.Message}");
            Console.WriteLine("Asegúrate de tener Node.js instalado y el archivo server.js en la ubicación correcta.");
            Console.WriteLine("\nPosibles soluciones:");
            Console.WriteLine("1. Verifica que Node.js esté instalado: 'node --version'");
            Console.WriteLine("2. Verifica la ruta del archivo server.js");
            Console.WriteLine("3. Ejecuta desde la carpeta raíz del proyecto");
        }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("=== CLIENTE WEBSOCKET GLOBAL ===");
        Console.WriteLine("Seleccione una opción:");
        Console.WriteLine("1. Iniciar servidor local (para hosting internacional)");
        Console.WriteLine("2. Conectarse a servidor por IP pública");
        Console.WriteLine("3. Conectarse a servidor por dominio/URL");
        Console.WriteLine("4. Conectarse a servidor local");
        
        string choice = Console.ReadLine();
        string serverAddress = "";

        switch (choice)
        {
            case "1":
                await StartLocalServer();
                serverAddress = "ws://localhost:8080";
                break;
            case "2":
                Console.Write("Ingrese la IP pública del servidor (ej: 203.0.113.123): ");
                string publicIP = Console.ReadLine();
                Console.Write("Ingrese el puerto (por defecto 8080): ");
                string port = Console.ReadLine();
                if (string.IsNullOrEmpty(port)) port = "8080";
                serverAddress = $"ws://{publicIP}:{port}";
                break;
            case "3":
                Console.Write("Ingrese el dominio o URL (ej: myserver.ddns.net o ws://example.com:8080): ");
                string domain = Console.ReadLine();
                if (!domain.StartsWith("ws://") && !domain.StartsWith("wss://"))
                {
                    serverAddress = $"ws://{domain}:8080";
                }
                else
                {
                    serverAddress = domain;
                }
                break;
            case "4":
                serverAddress = "ws://localhost:8080";
                break;
            default:
                Console.WriteLine("Opción inválida");
                return;
        }

        try
        {
            await ConnectToServer(serverAddress);
            
            Console.Write("Ingrese su nombre de jugador: ");
            string playerName = Console.ReadLine();
            await SendMessage(playerName);

            string userInput = "";
            while (userInput != "exit")
            {
                Console.Write("Envie un mensaje (o 'exit' para salir): ");
                userInput = Console.ReadLine();
                
                if (userInput != "exit")
                {
                    await SendMessage(userInput + "," + playerName);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        if (ws.State == WebSocketState.Open)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }
        
        Console.WriteLine("Presione cualquier tecla para salir...");
        Console.ReadKey();
    }
}
