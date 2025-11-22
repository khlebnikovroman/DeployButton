using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;

namespace ArduinoMusicControl.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MusicController : ControllerBase
    {
        private static SerialPort _arduinoPort;
        private static readonly string MusicDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "music");
        private static string _currentTrack = string.Empty;
        private static bool _isPlaying = false;

        static MusicController()
        {
            // Initialize music directory
            if (!Directory.Exists(MusicDirectory))
            {
                Directory.CreateDirectory(MusicDirectory);
            }
        }

        [HttpGet("tracks")]
        public IActionResult GetTracks()
        {
            if (!Directory.Exists(MusicDirectory))
            {
                return NotFound("Music directory not found");
            }

            var tracks = new List<string>();
            var files = Directory.GetFiles(MusicDirectory, "*.mp3");
            foreach (var file in files)
            {
                tracks.Add(Path.GetFileName(file));
            }

            return Ok(new { tracks });
        }

        [HttpPost("play")]
        public async Task<IActionResult> PlayTrack([FromBody] TrackRequest request)
        {
            if (string.IsNullOrEmpty(request.TrackName))
            {
                return BadRequest("Track name is required");
            }

            var trackPath = Path.Combine(MusicDirectory, request.TrackName);
            if (!System.IO.File.Exists(trackPath))
            {
                return NotFound($"Track {request.TrackName} not found");
            }

            try
            {
                // Connect to Arduino if not already connected
                if (_arduinoPort == null || !_arduinoPort.IsOpen)
                {
                    ConnectToArduino();
                }

                // Send play command to Arduino
                _arduinoPort.WriteLine($"PLAY:{request.TrackName}");
                _currentTrack = request.TrackName;
                _isPlaying = true;

                return Ok(new { message = $"Playing {request.TrackName}", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpPost("stop")]
        public IActionResult StopTrack()
        {
            try
            {
                if (_arduinoPort != null && _arduinoPort.IsOpen)
                {
                    _arduinoPort.WriteLine("STOP");
                }

                _isPlaying = false;
                _currentTrack = string.Empty;

                return Ok(new { message = "Playback stopped", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message, success = false });
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            var isConnected = _arduinoPort != null && _arduinoPort.IsOpen;
            return Ok(new { 
                isConnected, 
                isPlaying = _isPlaying, 
                currentTrack = _currentTrack 
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadTrack(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            if (!file.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only MP3 files are allowed");
            }

            var filePath = Path.Combine(MusicDirectory, file.FileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { message = $"File {file.FileName} uploaded successfully", success = true });
        }

        private void ConnectToArduino()
        {
            // Try to find available COM port
            string[] ports = SerialPort.GetPortNames();
            
            foreach (string port in ports)
            {
                try
                {
                    _arduinoPort = new SerialPort(port, 9600);
                    _arduinoPort.Open();
                    
                    // Send test command to verify connection
                    _arduinoPort.WriteLine("TEST");
                    System.Threading.Thread.Sleep(100);
                    
                    if (_arduinoPort.IsOpen)
                    {
                        Console.WriteLine($"Connected to Arduino on {port}");
                        break;
                    }
                }
                catch
                {
                    _arduinoPort?.Close();
                    _arduinoPort = null;
                }
            }

            if (_arduinoPort == null || !_arduinoPort.IsOpen)
            {
                throw new InvalidOperationException("Could not connect to Arduino on any available port");
            }
        }
    }

    public class TrackRequest
    {
        public string TrackName { get; set; }
    }
}