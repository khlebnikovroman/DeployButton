// sound-event-control.component.ts
import { Component, Input } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { SoundService } from '../shared/services/sound.service';
import { Sound } from '../shared/models/sound-model';
import { ReactiveFormsModule } from '@angular/forms'; // 👈 add this
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button'; 
import { MatIconModule } from '@angular/material/icon';
@Component({
  selector: 'app-sound-event-control',
  template: `
    <div [formGroup]="group" class="sound-event">
    <h3>{{ label }}</h3>
    <mat-checkbox formControlName="enabled">Включить звук</mat-checkbox>

    @if (group.get('enabled')?.value) {
        <div class="sound-controls">
        <mat-form-field appearance="fill" class="sound-select">
            <mat-label>Звук</mat-label>
            <mat-select formControlName="soundId">
            @for (sound of sounds; track sound.id) {
                <mat-option [value]="sound.id">{{ sound.name }}</mat-option>
            }
            </mat-select>
        </mat-form-field>

        <button style="margin-left: 8px;"
            matMiniFab 
            aria-label="Проиграть выбранный звук"
            (click)="playSelectedSound()"
            [disabled]="!group.get('soundId')?.value"
        >
            <mat-icon>play_arrow</mat-icon>
        </button>
        </div>
    }
    </div>
  `,
  styles: [`
    .sound-event { margin: 16px 0; }
    .full-width { width: 100%; }
  `],
    imports: [MatFormFieldModule, MatSelectModule, MatCheckboxModule, ReactiveFormsModule, MatIconModule, MatButtonModule]
})
export class SoundEventControlComponent {
  @Input({ required: true }) group!: FormGroup;
  @Input({ required: true }) label!: string;
  @Input({ required: true }) sounds: Sound[] = [];

  constructor(private soundService: SoundService) {}

  playSelectedSound(): void {
    const soundId = this.group.get('soundId')?.value as string;
    if (soundId) {
      this.soundService.playSoundById(this.sounds, soundId);
    }
  }
}