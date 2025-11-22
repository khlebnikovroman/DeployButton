import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ArduinoService {
  private apiUrl = '/api';

  constructor(private http: HttpClient) { }

  getStatus(): Observable<any> {
    return this.http.get(`${this.apiUrl}/status`);
  }

  playSong(song: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/play`, { song });
  }

  stopPlayback(): Observable<any> {
    return this.http.post(`${this.apiUrl}/stop`, {});
  }
}