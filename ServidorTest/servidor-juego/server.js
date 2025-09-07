const WebSocket = require('ws');
const os = require('os');

// Get local IP address
function getLocalIPAddress() {
    const interfaces = os.networkInterfaces();
    for (const devName in interfaces) {
        const iface = interfaces[devName];
        for (let i = 0; i < iface.length; i++) {
            const alias = iface[i];
            if (alias.family === 'IPv4' && alias.address !== '127.0.0.1' && !alias.internal) {
                return alias.address;
            }
        }
    }
    return '0.0.0.0';
}

const PORT = 8080;
const localIP = getLocalIPAddress();

// Create server that listens on all interfaces (0.0.0.0)
const server = new WebSocket.Server({ 
    port: PORT,
    host: '0.0.0.0'  // Listen on all network interfaces
});

let clients = [];
let mensajes = "";

server.on('connection', (ws, req) => {
    const clientIP = req.socket.remoteAddress;
    console.log(`Nuevo cliente conectado desde: ${clientIP}\n`);

    // Agregar cliente a la lista
    clients.push(ws);
    
    // Manejo de mensajes recibidos
    ws.on('message', (message) => {
        mensajeRecibido = Buffer.from(message).toString();
        mensajePartes = mensajeRecibido.split(",");
        
        if(mensajePartes.length != 2){
            // si el mensaje no tiene coma es porque es jugador es nuevo.
            console.log("Bienvenido ", mensajeRecibido);
            return;
        }
        
        mensaje = mensajePartes[0];
        jugador = mensajePartes[1];
        
        // aqui se muestra basicamente el mensaje en la terminal del servidor
        console.log(jugador,': ', mensaje);

        notificacion = jugador+': '+mensaje;
        mensajes += notificacion + "\n";
        
        var buf = Buffer.from(mensajes ,'utf8');
        
        // Reenviar el mensaje a todos los clientes excepto al remitente
        clients.forEach(client => {
            if (client !== ws && client.readyState === WebSocket.OPEN) {
                client.send(buf);
            }
        });
    });

    // Manejo de desconexión
    ws.on('close', () => {
        console.log(`Cliente desconectado desde: ${clientIP}`);
        clients = clients.filter(client => client !== ws);
    });
});

console.log(`Servidor WebSocket corriendo en:`);
console.log(`- Local: ws://localhost:${PORT}`);
console.log(`- Red: ws://${localIP}:${PORT}`);
console.log(`\nComparte la dirección de red con otros clientes para que se conecten desde ubicaciones remotas.`);
console.log(`Asegúrate de que el puerto ${PORT} esté abierto en el firewall.`);
