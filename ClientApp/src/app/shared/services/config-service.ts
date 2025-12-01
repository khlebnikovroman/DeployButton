import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AppSettings } from '../models/app-settings.model';
import {BACKEND_CONFIG} from '../../core/BACKEND_CONFIG'
@Injectable({ providedIn: 'root' })
export class ConfigService {
  private readonly API_URL = '/api/config';
  private readonly FullUrl = BACKEND_CONFIG.api.baseUrl+this.API_URL
  constructor(private http: HttpClient) {}

  getConfig(): Observable<AppSettings> {
    return this.http.get<AppSettings>(this.FullUrl);
  }

  saveConfig(config: AppSettings) {
    return this.http.post<void>(this.FullUrl, config);
  }
}
