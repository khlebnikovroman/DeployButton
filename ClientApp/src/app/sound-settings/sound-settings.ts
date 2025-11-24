import { Component, OnInit } from '@angular/core';
import { ConfigService } from '../shared/services/config-service';
import { AppSettings } from '../shared/models/app-settings.model';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCardModule } from '@angular/material/card';
import { MatSliderModule } from '@angular/material/slider';

@Component({
  selector: 'app-sound-settings',
  templateUrl: './sound-settings.html',
  styleUrls: ['./sound-settings.css'],
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatSelectModule,
    MatCardModule,
    MatSliderModule
  ]
})
export class SoundSettingsComponent implements OnInit {
  configForm!: FormGroup;
  originalConfig!: AppSettings;

  constructor(
    private configService: ConfigService,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this.configService.getConfig().subscribe(config => {
      this.originalConfig = config;
      this.initForm(config);
    });
  }

  private initForm(config: AppSettings) {
    this.configForm = this.fb.group({
      audio: this.fb.group({
        volume: [config.audio.volume],
        deployStart: this.fb.group({
          enabled: [config.audio.deployStart.enabled],
          selectedSoundId: [config.audio.deployStart.selectedSoundId]
        }),
        buildSuccess: this.fb.group({
          enabled: [config.audio.buildSuccess.enabled],
          selectedSoundId: [config.audio.buildSuccess.selectedSoundId]
        }),
        buildFail: this.fb.group({
          enabled: [config.audio.buildFail.enabled],
          selectedSoundId: [config.audio.buildFail.selectedSoundId]
        })
      })
    });
  }

  save() {
    const updatedConfig = {
      ...this.originalConfig,
      audio: this.configForm.value.audio
    };
    this.configService.saveConfig(updatedConfig).subscribe(() => {
      alert('Настройки сохранены!');
    });
  }

  protected readonly Array = Array;
}
