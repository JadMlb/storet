import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ToastIcon } from './toast-icon';

describe('ToastIcon', () => {
  let component: ToastIcon;
  let fixture: ComponentFixture<ToastIcon>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToastIcon]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ToastIcon);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
