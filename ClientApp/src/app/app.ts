import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Button3dComponent } from "./button3d/button3d"
import { DeviceStatusComponent } from './device-status/device-status';
import { AudioSettingsComponent } from './sound/sound-settings';
import { TeamCitySettingsComponent } from "./teamcity-settings.component";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Button3dComponent, DeviceStatusComponent, AudioSettingsComponent, TeamCitySettingsComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('ClientApp');
}
