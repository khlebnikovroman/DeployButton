// sound.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { Sound } from '../models/sound-model';
import { BACKEND_CONFIG } from '../../core/BACKEND_CONFIG'
import { AudioConfig } from '../models/audio-config-model';

@Injectable({ providedIn: 'root' })
export class SoundService {
  private readonly API_URL = '/api/AudioSettings';
  private readonly FullUrl = BACKEND_CONFIG.api.baseUrl+this.API_URL
  private sounds$: Observable<Sound[]> | null = null;

  constructor(private http: HttpClient) {}

    getSounds(): Observable<Sound[]> {
        return this.http.get<Sound[]>(this.FullUrl+'/sounds').pipe(shareReplay(1));
    }

    // audio-settings.service.ts (новый сервис)
    getConfig(): Observable<AudioConfig> {
        return this.http.get<AudioConfig>(this.FullUrl+'/config');
    }

    saveConfig(config: AudioConfig): Observable<void> {
        return this.http.post<void>(this.FullUrl+'/config', config);
    }

      // ✅ Воспроизводит звук по URL
  playSound(url: string): void {
    if (!url) return;
    const audio = new Audio(url);
    audio.play().catch(err => console.warn('Ошибка воспроизведения:', err));
  }

  // ✅ Воспроизводит звук по ID (удобно из UI)
  playSoundById(sounds: Sound[], id: string): void {
    const sound = sounds.find(s => s.id === id);
    if (sound) {
      this.playSound(sound.url);
    }
  }
}