// device-state.service.ts
import { Injectable } from '@angular/core';
import { SignalRService } from './signalr.service';
import { DeviceState } from '../models/device-state.model';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class DeviceStateService {
  constructor(private signalR: SignalRService) {}

  getState(): Observable<DeviceState> {
    return this.signalR.deviceState$;
  }

  getPressedState(): Observable<void> {
    return this.signalR.buttonPressed$;
  }
  
  getReleasedState(): Observable<void> {
    return this.signalR.buttonReleased$;
  }
}
