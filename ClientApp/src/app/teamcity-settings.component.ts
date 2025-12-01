// teamcity-settings.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatExpansionModule } from '@angular/material/expansion';
import { ConfigService } from './shared/services/config-service';
import { TeamCityConfig } from './shared/models/app-settings.model';

@Component({
  selector: 'app-teamcity-settings',
  template: `
    <mat-accordion>
      <mat-expansion-panel [expanded]="false">
        <mat-expansion-panel-header>
          <mat-panel-title>Настройки TeamCity</mat-panel-title>
        </mat-expansion-panel-header>

        <form [formGroup]="form" class="teamcity-form">
          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Базовый URL TeamCity</mat-label>
            <input matInput formControlName="baseUrl" required />
          </mat-form-field>

          <mat-form-field appearance="fill" class="full-width">
            <mat-label>ID конфигурации сборки</mat-label>
            <input matInput formControlName="buildConfigurationId" required />
          </mat-form-field>

          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Имя пользователя</mat-label>
            <input matInput formControlName="username" required />
          </mat-form-field>

          <mat-form-field appearance="fill" class="full-width">
            <mat-label>Пароль</mat-label>
            <input matInput type="password" formControlName="password" required />
          </mat-form-field>

          <div class="button-row">
            <button
              mat-raised-button
              color="primary"
              (click)="save()"
              [disabled]="form.invalid"
            >
              Сохранить TeamCity
            </button>
          </div>
        </form>
      </mat-expansion-panel>
    </mat-accordion>
  `,
  styles: [`
    .full-width {
      width: 100%;
    }
    .button-row {
      margin-top: 16px;
      display: flex;
      justify-content: flex-start;
    }
  `],
  imports: [
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatExpansionModule,
    ReactiveFormsModule
  ],
  standalone: true
})
export class TeamCitySettingsComponent implements OnInit {
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private configService: ConfigService
  ) {}

  ngOnInit(): void {
    this.loadConfig();
  }

  private loadConfig(): void {
    this.configService.getConfig().subscribe({
      next: config => this.initForm(config.teamCity),
      error: () => this.initForm()
    });
  }

  private initForm(config?: TeamCityConfig): void {
    this.form = this.fb.group({
      baseUrl: [config?.baseUrl || 'http://192.168.1.210:8111', Validators.required],
      buildConfigurationId: [config?.buildConfigurationId || 'HelloWorld_Deploy', Validators.required],
      username: [config?.username || 'admin', Validators.required],
      password: [config?.password || 'admin', Validators.required]
    });
  }

    save(): void {
    if (this.form.invalid) return;

    // Получаем текущую конфигурацию, обновляем только teamCity
    this.configService.getConfig().subscribe({
      next: currentConfig => {
        const updatedConfig = {
          ...currentConfig,
          teamCity: this.form.getRawValue() as TeamCityConfig
        };
        this.configService.saveConfig(updatedConfig).subscribe({
          next: () => console.log('TeamCity настройки сохранены'),
          error: err => console.error('Ошибка сохранения TeamCity:', err)
        });
      }
    });
  }
}