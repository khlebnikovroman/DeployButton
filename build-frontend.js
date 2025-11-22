// Simple script to create basic frontend dist files for the Express server
const fs = require('fs');
const path = require('path');

// Create dist directory if it doesn't exist
const distPath = path.join(__dirname, 'frontend', 'dist');
if (!fs.existsSync(distPath)) {
  fs.mkdirSync(distPath, { recursive: true });
}

// Create a basic index.html file
const htmlContent = `<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Arduino Music Control</title>
  <base href="/">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <link rel="icon" type="image/x-icon" href="favicon.ico">
  <style>
    body {
      margin: 0;
      font-family: Roboto, "Helvetica Neue", sans-serif;
      background-color: #f5f5f5;
    }
  </style>
</head>
<body>
  <div id="app">
    <div style="max-width: 800px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;">
      <header style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px; border-bottom: 2px solid #eee; padding-bottom: 15px;">
        <h1>Arduino Music Control</h1>
        <div id="status" style="padding: 8px 15px; border-radius: 20px; font-weight: bold;">Arduino: <span>Connecting...</span></div>
      </header>

      <main>
        <section style="margin-bottom: 25px;">
          <h2>Music Player</h2>
          <div id="current-song" style="display: none; background-color: #e9f7ef; padding: 15px; border-radius: 8px; margin-bottom: 25px; display: flex; justify-content: space-between; align-items: center;">
            <p>Currently playing: <strong id="current-song-title"></strong></p>
            <button id="stop-btn" style="background-color: #dc3545; color: white; padding: 8px 16px; border: none; border-radius: 5px; cursor: pointer;">Stop</button>
          </div>
          
          <div class="song-list">
            <h3>Available Songs</h3>
            <div id="songs-container" style="margin-top: 15px;">
              <!-- Songs will be loaded here -->
            </div>
          </div>
        </section>
      </main>
      
      <footer style="margin-top: 40px; text-align: center; color: #666; border-top: 1px solid #eee; padding-top: 20px;">
        <p>Arduino Music Control System</p>
      </footer>
    </div>
  </div>

  <script>
    // Simple script to interact with the API
    document.addEventListener('DOMContentLoaded', function() {
      const statusEl = document.getElementById('status');
      const currentSongEl = document.getElementById('current-song');
      const currentSongTitleEl = document.getElementById('current-song-title');
      const stopBtn = document.getElementById('stop-btn');
      const songsContainer = document.getElementById('songs-container');
      
      let currentSong = null;
      
      // Check Arduino status
      function checkStatus() {
        fetch('/api/status')
          .then(response => response.json())
          .then(data => {
            if (data.connected) {
              statusEl.innerHTML = 'Arduino: <span style="color: #155724;">Connected</span>';
              statusEl.style.backgroundColor = '#d4edda';
              statusEl.style.color = '#155724';
            } else {
              statusEl.innerHTML = 'Arduino: <span style="color: #721c24;">Disconnected</span>';
              statusEl.style.backgroundColor = '#f8d7da';
              statusEl.style.color = '#721c24';
            }
          })
          .catch(error => {
            console.error('Error checking status:', error);
            statusEl.innerHTML = 'Arduino: <span style="color: #721c24;">Error</span>';
            statusEl.style.backgroundColor = '#f8d7da';
            statusEl.style.color = '#721c24';
          });
      }
      
      // Load songs
      function loadSongs() {
        fetch('/api/songs')
          .then(response => response.json())
          .then(data => {
            songsContainer.innerHTML = '';
            data.songs.forEach(song => {
              const songEl = document.createElement('div');
              songEl.className = 'song-item';
              songEl.style = 'display: flex; justify-content: space-between; align-items: center; padding: 12px; border: 1px solid #ddd; border-radius: 6px; margin-bottom: 10px; background-color: #f9f9f9;';
              
              const titleSpan = document.createElement('span');
              titleSpan.className = 'song-title';
              titleSpan.textContent = song.title;
              titleSpan.style = 'font-size: 16px; font-weight: 500;';
              
              const playBtn = document.createElement('button');
              playBtn.textContent = 'Play';
              playBtn.style = 'background-color: #28a745; color: white; padding: 10px 20px; border: none; border-radius: 5px; cursor: pointer; font-size: 14px;';
              playBtn.onclick = () => playSong(song);
              
              songEl.appendChild(titleSpan);
              songEl.appendChild(playBtn);
              songsContainer.appendChild(songEl);
            });
          })
          .catch(error => {
            console.error('Error loading songs:', error);
            songsContainer.innerHTML = '<p>Error loading songs</p>';
          });
      }
      
      // Play song
      function playSong(song) {
        fetch('/api/play', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ song: song.file })
        })
        .then(response => response.json())
        .then(data => {
          if (data.success) {
            currentSong = song;
            currentSongTitleEl.textContent = song.title;
            currentSongEl.style.display = 'flex';
          } else {
            alert('Error playing song: ' + data.error);
          }
        })
        .catch(error => {
          console.error('Error playing song:', error);
          alert('Error playing song');
        });
      }
      
      // Stop playback
      stopBtn.onclick = function() {
        fetch('/api/stop', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({})
        })
        .then(response => response.json())
        .then(data => {
          if (data.success) {
            currentSong = null;
            currentSongEl.style.display = 'none';
          } else {
            alert('Error stopping playback: ' + data.error);
          }
        })
        .catch(error => {
          console.error('Error stopping playback:', error);
          alert('Error stopping playback');
        });
      };
      
      // Initial load
      checkStatus();
      loadSongs();
      
      // Refresh status every 5 seconds
      setInterval(checkStatus, 5000);
    });
  </script>
</body>
</html>`;

fs.writeFileSync(path.join(distPath, 'index.html'), htmlContent);

console.log('Frontend dist files created successfully!');