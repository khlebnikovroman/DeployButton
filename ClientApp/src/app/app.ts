import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Button3dComponent } from "./button3d/button3d"
import { DeviceStatusComponent } from './device-status/device-status';
import { SoundSettingsComponent } from './sound-settings/sound-settings';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Button3dComponent, DeviceStatusComponent, SoundSettingsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('ClientApp');
}
