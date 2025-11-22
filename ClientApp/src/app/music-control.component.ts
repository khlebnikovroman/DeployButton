import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Track {
  name: string;
}

interface Status {
  isConnected: boolean;
  isPlaying: boolean;
  currentTrack: string;
}

@Component({
  selector: 'app-music-control',
  template: `
    <div class="container">
      <h1>Music Control Panel</h1>
      
      <div class="status-section">
        <h2>Status</h2>
        <div class="status-info">
          <p>Arduino Connection: <span [class]="status.isConnected ? 'connected' : 'disconnected'">{{ status.isConnected ? 'Connected' : 'Disconnected' }}</span></p>
          <p>Current Track: {{ status.currentTrack || 'None' }}</p>
          <p>Playing Status: <span [class]="status.isPlaying ? 'playing' : 'stopped'">{{ status.isPlaying ? 'Playing' : 'Stopped' }}</span></p>
        </div>
      </div>
      
      <div class="upload-section">
        <h2>Upload Music</h2>
        <input type="file" (change)="onFileSelected($event)" accept=".mp3" #fileInput>
        <button (click)="uploadFile()" [disabled]="!selectedFile">Upload</button>
        <div *ngIf="uploadStatus" class="upload-status">{{ uploadStatus }}</div>
      </div>
      
      <div class="tracks-section">
        <h2>Available Tracks</h2>
        <div *ngIf="tracks.length === 0" class="no-tracks">No tracks available</div>
        <ul *ngIf="tracks.length > 0" class="tracks-list">
          <li *ngFor="let track of tracks" class="track-item">
            <span class="track-name">{{ track }}</span>
            <button (click)="playTrack(track)" [disabled]="status.isPlaying && status.currentTrack === track" class="play-btn">Play</button>
            <button (click)="stopTrack()" *ngIf="status.isPlaying && status.currentTrack === track" class="stop-btn">Stop</button>
          </li>
        </ul>
      </div>
      
      <div class="controls-section">
        <h2>Controls</h2>
        <button (click)="stopTrack()" [disabled]="!status.isPlaying">Stop Playback</button>
      </div>
    </div>
  `,
  styles: [`
    .container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
      font-family: Arial, sans-serif;
    }
    
    h1 {
      text-align: center;
      color: #333;
    }
    
    h2 {
      color: #555;
      border-bottom: 1px solid #ddd;
      padding-bottom: 5px;
    }
    
    .status-info {
      background-color: #f9f9f9;
      padding: 15px;
      border-radius: 5px;
      margin: 10px 0;
    }
    
    .connected {
      color: green;
      font-weight: bold;
    }
    
    .disconnected {
      color: red;
      font-weight: bold;
    }
    
    .playing {
      color: green;
      font-weight: bold;
    }
    
    .stopped {
      color: red;
      font-weight: bold;
    }
    
    .upload-section, .tracks-section, .controls-section {
      margin: 20px 0;
      padding: 15px;
      border: 1px solid #ddd;
      border-radius: 5px;
    }
    
    .track-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px;
      border-bottom: 1px solid #eee;
    }
    
    .track-name {
      flex-grow: 1;
    }
    
    button {
      padding: 8px 15px;
      margin: 0 5px;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      background-color: #007bff;
      color: white;
    }
    
    button:hover {
      background-color: #0056b3;
    }
    
    button:disabled {
      background-color: #cccccc;
      cursor: not-allowed;
    }
    
    .play-btn {
      background-color: #28a745;
    }
    
    .play-btn:hover {
      background-color: #218838;
    }
    
    .stop-btn {
      background-color: #dc3545;
    }
    
    .stop-btn:hover {
      background-color: #c82333;
    }
    
    .tracks-list {
      list-style: none;
      padding: 0;
    }
    
    .no-tracks {
      text-align: center;
      color: #666;
      font-style: italic;
    }
    
    .upload-status {
      margin-top: 10px;
      padding: 10px;
      border-radius: 4px;
      background-color: #d4edda;
      color: #155724;
    }
  `]
})
export class MusicControlComponent implements OnInit {
  tracks: Track[] = [];
  status: Status = { isConnected: false, isPlaying: false, currentTrack: '' };
  selectedFile: File | null = null;
  uploadStatus: string | null = null;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadTracks();
    this.loadStatus();
    // Refresh status every 5 seconds
    setInterval(() => {
      this.loadStatus();
    }, 5000);
  }

  loadTracks(): void {
    this.http.get<{ tracks: string[] }>('/api/music/tracks').subscribe(
      response => {
        this.tracks = response.tracks.map(track => ({ name: track }));
      },
      error => {
        console.error('Error loading tracks:', error);
      }
    );
  }

  loadStatus(): void {
    this.http.get<Status>('/api/music/status').subscribe(
      response => {
        this.status = response;
      },
      error => {
        console.error('Error loading status:', error);
      }
    );
  }

  onFileSelected(event: any): void {
    if (event.target.files && event.target.files[0]) {
      this.selectedFile = event.target.files[0];
    }
  }

  uploadFile(): void {
    if (!this.selectedFile) {
      return;
    }

    const formData = new FormData();
    formData.append('file', this.selectedFile);

    this.http.post<any>('/api/music/upload', formData).subscribe(
      response => {
        this.uploadStatus = response.message;
        setTimeout(() => {
          this.uploadStatus = null;
          this.loadTracks(); // Reload tracks after successful upload
        }, 3000);
      },
      error => {
        this.uploadStatus = 'Upload failed: ' + error.error?.message || 'Unknown error';
        setTimeout(() => {
          this.uploadStatus = null;
        }, 3000);
      }
    );
  }

  playTrack(trackName: string): void {
    this.http.post<any>('/api/music/play', { trackName }).subscribe(
      response => {
        console.log('Play response:', response);
        this.loadStatus();
      },
      error => {
        console.error('Error playing track:', error);
      }
    );
  }

  stopTrack(): void {
    this.http.post<any>('/api/music/stop', {}).subscribe(
      response => {
        console.log('Stop response:', response);
        this.loadStatus();
      },
      error => {
        console.error('Error stopping track:', error);
      }
    );
  }
}