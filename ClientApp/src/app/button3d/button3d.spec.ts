import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Button3d } from './button3d';

describe('Button3d', () => {
  let component: Button3d;
  let fixture: ComponentFixture<Button3d>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Button3d]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Button3d);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
