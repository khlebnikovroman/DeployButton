// audio-settings.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Sound } from '../shared/models/sound-model';
import { SoundService } from '../shared/services/sound.service';
import { AudioConfig, ButtonSoundEventType } from '../shared/models/audio-config-model';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatLabel } from '@angular/material/form-field';
import { SoundEventControlComponent } from './sound-event-control.component';
import { MatSelectModule } from '@angular/material/select';
import { MatSliderModule } from '@angular/material/slider';
import { MatButtonModule } from '@angular/material/button';
@Component({
  selector: 'app-audio-settings',
  templateUrl: './sound-settings.html', // ⚠️ убедись, что имя файла совпадает!
  styleUrls: ['./sound-settings.css'],
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatLabel,
    SoundEventControlComponent,
    MatSelectModule,
    ReactiveFormsModule,
    MatSliderModule,
    MatButtonModule,
  ],
  standalone: true
})
export class AudioSettingsComponent implements OnInit {
  configForm!: FormGroup;
  sounds: Sound[] = [];
  readonly eventTypes = Object.values(ButtonSoundEventType);

  getEventLabel(type: ButtonSoundEventType): string {
    const labels: Record<ButtonSoundEventType, string> = {
      [ButtonSoundEventType.BuildQueued]: 'Сборка поставлена в очередь',
      [ButtonSoundEventType.BuildNotQueued]: 'Сборка не поставлена в очередь',
      [ButtonSoundEventType.BuildSucceeded]: 'Сборка успешна',
      [ButtonSoundEventType.BuildFailed]: 'Сборка завершилась с ошибкой'
    };
    return labels[type];
  }
  constructor(
    private fb: FormBuilder,
    private soundService: SoundService
  ) { }

  ngOnInit(): void {
    this.soundService.getSounds().subscribe(sounds => {
      this.sounds = sounds;
      this.loadConfig();
    });
  }

  private loadConfig(): void {
    this.soundService.getConfig().subscribe({
      next: config => {
        this.initForm(config);
      },
      error: () => {
        // Fallback на значения по умолчанию при ошибке
        this.initForm();
      }
    });
  }

  getEventGroup(eventType: ButtonSoundEventType): FormGroup {
    return this.configForm.get(eventType) as FormGroup;
  }

    private initForm(config?: AudioConfig): void {
    const volume = config?.volume ?? 15;
    const soundsConfig = config?.sounds ?? {} as Record<ButtonSoundEventType, string>;

    const eventGroups: Record<ButtonSoundEventType, FormGroup> = {} as any;
    for (const eventType of this.eventTypes) {
      const soundId = soundsConfig[eventType] || '';
      eventGroups[eventType] = this.createEventGroup(soundId);
    }

    this.configForm = this.fb.group({
      volume: [volume, [Validators.min(0), Validators.max(30)]],
      ...eventGroups
    });
  }

  private createEventGroup(defaultSoundId: string): FormGroup {
    return this.fb.group({
      enabled: [!!defaultSoundId],
      soundId: [defaultSoundId || '']
    });
  }

  save(): void {
    if (this.configForm.invalid) return;

    const raw = this.configForm.getRawValue();
    const sounds: Record<ButtonSoundEventType, string> = {} as any;

    for (const eventType of this.eventTypes) {
      const group = raw[eventType];
      sounds[eventType] = group.enabled ? group.soundId : '';
    }

    const config: AudioConfig = {
      volume: raw.volume,
      sounds
    };

    this.soundService.saveConfig(config).subscribe({
      next: () => console.log('Настройки сохранены'),
      error: err => console.error('Ошибка сохранения:', err)
    });
  }

}