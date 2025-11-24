import { Component, OnInit } from '@angular/core';
import { DeviceStateService } from '../shared/services/device-state-service';
import { DeviceState } from '../shared/models/device-state.model';
import { AsyncPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-device-status',
  templateUrl: './device-status.html',
  styleUrls: ['./device-status.css'],
  standalone: true,
  imports: [AsyncPipe, MatCardModule]
})
export class DeviceStatusComponent {
  deviceState$;

  constructor(private deviceStateService: DeviceStateService) {
    this.deviceState$ = this.deviceStateService.getState();
  }

  getAvailablePorts(ports: string[] | undefined): string {
    return ports && ports.length > 0 ? ports.join(', ') : 'Нет';
  }
}
