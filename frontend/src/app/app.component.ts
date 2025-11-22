import { Component, OnInit } from '@angular/core';
import { SongService } from './song.service';
import { ArduinoService } from './arduino.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  title = 'Arduino Music Control';
  songs: any[] = [];
  selectedSong: any = null;
  arduinoStatus: any = null;
  loading = false;

  constructor(
    private songService: SongService,
    private arduinoService: ArduinoService
  ) {}

  ngOnInit(): void {
    this.loadSongs();
    this.checkArduinoStatus();
  }

  loadSongs(): void {
    this.songService.getSongs().subscribe(
      (data: any) => {
        this.songs = data.songs;
      },
      error => {
        console.error('Error loading songs:', error);
      }
    );
  }

  checkArduinoStatus(): void {
    this.arduinoService.getStatus().subscribe(
      (data: any) => {
        this.arduinoStatus = data;
      },
      error => {
        console.error('Error checking Arduino status:', error);
      }
    );
  }

  playSong(song: any): void {
    this.loading = true;
    this.arduinoService.playSong(song.file).subscribe(
      response => {
        console.log('Play command sent:', response);
        this.selectedSong = song;
        this.loading = false;
      },
      error => {
        console.error('Error playing song:', error);
        this.loading = false;
      }
    );
  }

  stopPlayback(): void {
    this.loading = true;
    this.arduinoService.stopPlayback().subscribe(
      response => {
        console.log('Stop command sent:', response);
        this.selectedSong = null;
        this.loading = false;
      },
      error => {
        console.error('Error stopping playback:', error);
        this.loading = false;
      }
    );
  }
}