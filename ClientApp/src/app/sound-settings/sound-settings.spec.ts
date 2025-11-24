import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SoundSettings } from './sound-settings';

describe('SoundSettings', () => {
  let component: SoundSettings;
  let fixture: ComponentFixture<SoundSettings>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SoundSettings]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SoundSettings);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
