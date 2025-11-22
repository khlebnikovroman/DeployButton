const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
require('dotenv').config();

// Try to load serialport, but handle gracefully if not available
let SerialPort, ReadlineParser;
try {
  SerialPort = require('serialport');
  ReadlineParser = require('@serialport/parser-readline').ReadlineParser;
} catch (error) {
  console.warn('SerialPort module not available. Arduino connection will be simulated.');
  SerialPort = null;
  ReadlineParser = null;
}

const app = express();
const PORT = process.env.PORT || 3000;
const ARDUINO_PORT = process.env.ARDUINO_PORT || '/dev/ttyUSB0'; // Change this for your Arduino port

// Middleware
app.use(cors());
app.use(bodyParser.json());
app.use(express.static('frontend/dist'));

// Arduino connection
let arduinoPort = null;
let arduinoParser = null;

function connectToArduino() {
  if (!SerialPort) {
    console.log('SerialPort not available, using simulated connection');
    return;
  }
  
  try {
    arduinoPort = new SerialPort({ path: ARDUINO_PORT, baudRate: 9600 });
    arduinoParser = arduinoPort.pipe(new ReadlineParser({ delimiter: '\n' }));
    
    arduinoParser.on('data', (data) => {
      console.log('Arduino:', data.toString().trim());
    });

    arduinoPort.on('error', (err) => {
      console.error('Arduino connection error:', err.message);
    });

    arduinoPort.on('close', () => {
      console.log('Arduino connection closed');
      // Attempt to reconnect after 2 seconds
      setTimeout(connectToArduino, 2000);
    });

    console.log(`Connected to Arduino on ${ARDUINO_PORT}`);
  } catch (error) {
    console.error('Error connecting to Arduino:', error.message);
  }
}

// Initialize Arduino connection
connectToArduino();

// API Routes
app.get('/api/status', (req, res) => {
  res.json({ 
    connected: SerialPort ? (arduinoPort && arduinoPort.isOpen) : false,
    arduinoPort: ARDUINO_PORT,
    simulated: !SerialPort
  });
});

app.post('/api/play', (req, res) => {
  const { song } = req.body;
  
  // If SerialPort is not available, simulate the operation
  if (!SerialPort) {
    console.log(`[SIMULATED] Playing: ${song}`);
    return res.json({ success: true, message: `Simulated playing: ${song}`, simulated: true });
  }
  
  if (!arduinoPort || !arduinoPort.isOpen) {
    return res.status(500).json({ error: 'Arduino not connected' });
  }

  try {
    arduinoPort.write(song + '\n', (err) => {
      if (err) {
        console.error('Error writing to Arduino:', err.message);
        return res.status(500).json({ error: 'Failed to send command to Arduino' });
      }
      res.json({ success: true, message: `Playing: ${song}` });
    });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.post('/api/stop', (req, res) => {
  // If SerialPort is not available, simulate the operation
  if (!SerialPort) {
    console.log('[SIMULATED] Stopping playback');
    return res.json({ success: true, message: 'Simulated playback stopped', simulated: true });
  }
  
  if (!arduinoPort || !arduinoPort.isOpen) {
    return res.status(500).json({ error: 'Arduino not connected' });
  }

  try {
    arduinoPort.write('STOP\n', (err) => {
      if (err) {
        console.error('Error sending stop command:', err.message);
        return res.status(500).json({ error: 'Failed to send stop command to Arduino' });
      }
      res.json({ success: true, message: 'Playback stopped' });
    });
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});

app.get('/api/songs', (req, res) => {
  // In a real implementation, this would read from a database or file system
  // For now, returning a mock list of songs
  res.json({
    songs: [
      { id: 1, title: 'Twinkle Twinkle Little Star', file: 'twinkle_twinkle' },
      { id: 2, title: 'Happy Birthday', file: 'happy_birthday' },
      { id: 3, title: 'Jingle Bells', file: 'jingle_bells' },
      { id: 4, title: 'Ode to Joy', file: 'ode_to_joy' }
    ]
  });
});

// Serve Angular app for all other routes
app.get('*', (req, res) => {
  res.sendFile(__dirname + '/frontend/dist/index.html');
});

app.listen(PORT, () => {
  console.log(`Server running on port ${PORT}`);
  console.log(`API available at http://localhost:${PORT}/api`);
});