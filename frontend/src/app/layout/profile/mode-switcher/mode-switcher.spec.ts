import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ModeSwitcher } from './mode-switcher';

describe('ModeSwitcher', () => {
  let component: ModeSwitcher;
  let fixture: ComponentFixture<ModeSwitcher>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ModeSwitcher]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ModeSwitcher);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
