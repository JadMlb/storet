import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToastProgress } from './toast-progress';

describe('ToastProgress', () => {
  let component: ToastProgress;
  let fixture: ComponentFixture<ToastProgress>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastProgress]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToastProgress);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
