
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { DeviceState } from '../models/device-state.model';
import { Subject } from 'rxjs'; // ← RxJS Subject
import {BACKEND_CONFIG} from '../../core/BACKEND_CONFIG'

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  private baseUrl = BACKEND_CONFIG.signalr.baseUrl;
  private readonly deviceStateSubject = new Subject<DeviceState>();
  private readonly buttonPressedSubject = new Subject<void>();
  private readonly buildStatusSubject = new Subject<string>();

  constructor() {
    this.createConnection();
    this.registerCallbacks();
    this.startConnection();
  }

  private createConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.baseUrl+'/hubs/deviceHub', {withCredentials: false})
      .withAutomaticReconnect()
      .build();
  }

  private registerCallbacks() {
    this.hubConnection.on('DeviceStateChanged', (state: DeviceState) => {
      this.deviceStateSubject.next(state);
    });

    this.hubConnection.on('ButtonPressed', () => {
      this.buttonPressedSubject.next();
    });

    this.hubConnection.on('BuildStatusChanged', (status: string) => {
      this.buildStatusSubject.next(status);
    });
  }

  private startConnection() {
    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.error('SignalR Connection Error: ', err));
  }

  // Экспортируем Observable (через asObservable — опционально, но безопасно)
  get deviceState$() {
    return this.deviceStateSubject.asObservable();
  }

  get buttonPressed$() {
    return this.buttonPressedSubject.asObservable();
  }

  get buildStatus$() {
    return this.buildStatusSubject.asObservable();
  }
}
